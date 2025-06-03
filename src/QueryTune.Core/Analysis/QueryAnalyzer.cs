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
        }

        public DataTable GetQueryMetrics(string sqlQuery)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Clear any existing metrics
            using var clearCmd = new SqlCommand(
                "DBCC FREEPROCCACHE; DBCC DROPCLEANBUFFERS;",
                connection);
            clearCmd.ExecuteNonQuery();

            // Get performance metrics from DMVs
            var metricsQuery = $@"
            -- First execute the query
            {sqlQuery}
            
            -- Then get metrics
            SELECT TOP 1
                qs.execution_count,
                qs.total_logical_reads,
                qs.total_logical_writes,
                qs.total_worker_time,
                qs.total_elapsed_time,
                qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time,
                qs.last_elapsed_time,
                qs.min_elapsed_time,
                qs.max_elapsed_time,
                qs.total_rows,
                qs.last_rows,
                qs.min_rows,
                qs.max_rows
            FROM sys.dm_exec_query_stats qs
            CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
            WHERE st.text LIKE '%' + @QueryPart + '%'
            ORDER BY qs.last_execution_time DESC";

            using var cmd = new SqlCommand(metricsQuery, connection);
            cmd.Parameters.AddWithValue("@QueryPart", sqlQuery.Substring(0, Math.Min(50, sqlQuery.Length)));

            var metricsTable = new DataTable();
            metricsTable.Load(cmd.ExecuteReader());
            return metricsTable;
        }
    }
}
