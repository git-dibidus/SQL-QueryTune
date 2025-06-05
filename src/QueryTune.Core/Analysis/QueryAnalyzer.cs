using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace QueryTune.Core.Analysis
{
    public class QueryAnalyzer
    {
        private readonly string _connectionString;

        public QueryAnalyzer(string connectionString) => _connectionString = connectionString;

        public DataTable GetExecutionPlan(string sqlQuery)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Enable statistics and get execution plan
            using var cmd = new SqlCommand(
                "SET STATISTICS XML ON;\n" +
                sqlQuery + "\n" +
                "SET STATISTICS XML OFF;",
                connection);

            using var reader = cmd.ExecuteReader();

            // The execution plan is returned as XML after the result sets
            while (reader.NextResult())
            {
                if (reader.GetName(0) == "Microsoft SQL Server 2005 XML Showplan")
                {
                    var planTable = new DataTable();
                    planTable.Load(reader);
                    return planTable;
                }
            }

            return null;
        }        public DataTable GetQueryMetrics(string sqlQuery)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Clear any existing metrics
            using var clearCmd = new SqlCommand(
                "DBCC FREEPROCCACHE; DBCC DROPCLEANBUFFERS;",
                connection);
            clearCmd.ExecuteNonQuery();

            // Create a more reliable way to track the query in DMVs
            string queryId = Guid.NewGuid().ToString("N");
            string taggedQuery = $"/* QueryTune ID: {queryId} */\n{sqlQuery}";
            
            // Execute and track the query
            var trackingQuery = $@"
            -- Enable statistics IO and time
            SET STATISTICS IO ON;
            SET STATISTICS TIME ON;
            
            -- Execute the query to collect metrics
            {taggedQuery}
            
            -- Disable statistics
            SET STATISTICS IO OFF;
            SET STATISTICS TIME OFF;
            
            -- Get the metrics from DMVs with more reliable tracking
            SELECT
                'Execution Count' AS MetricName, 1 AS MetricValue, 'count' AS MetricUnit
            UNION ALL
            SELECT 
                'CPU Time', 
                (SELECT MAX(total_worker_time) FROM sys.dm_exec_query_stats qs 
                CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st 
                WHERE st.text LIKE '%QueryTune ID: {queryId}%'), 
                'microseconds'
            UNION ALL
            SELECT 
                'Elapsed Time', 
                (SELECT MAX(total_elapsed_time) FROM sys.dm_exec_query_stats qs 
                CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st 
                WHERE st.text LIKE '%QueryTune ID: {queryId}%'), 
                'microseconds'
            UNION ALL
            SELECT 
                'Logical Reads', 
                (SELECT MAX(total_logical_reads) FROM sys.dm_exec_query_stats qs 
                CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st 
                WHERE st.text LIKE '%QueryTune ID: {queryId}%'), 
                'pages'
            UNION ALL
            SELECT 
                'Logical Writes', 
                (SELECT MAX(total_logical_writes) FROM sys.dm_exec_query_stats qs 
                CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st 
                WHERE st.text LIKE '%QueryTune ID: {queryId}%'), 
                'pages'
            UNION ALL
            SELECT 
                'Rows Returned', 
                (SELECT MAX(last_rows) FROM sys.dm_exec_query_stats qs 
                CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st 
                WHERE st.text LIKE '%QueryTune ID: {queryId}%'), 
                'count'";

            using var cmd = new SqlCommand(trackingQuery, connection);
            
            var metricsTable = new DataTable();
            using var reader = cmd.ExecuteReader();
            
            // Skip any result sets from the original query execution
            while (reader.NextResult())
            {
                // Keep looking for the metrics result set
                if (reader.FieldCount == 3 && 
                    reader.GetName(0) == "MetricName" && 
                    reader.GetName(1) == "MetricValue" && 
                    reader.GetName(2) == "MetricUnit")
                {
                    metricsTable.Load(reader);
                    break;
                }
            }
            
            return metricsTable;
        }
    }
}
