# QueryTune.Core Library Documentation

## Overview
QueryTune.Core is a .NET library designed to analyze SQL queries and provide optimization suggestions. The library performs comprehensive analysis of query execution plans, statistics, and indexing strategies to improve query performance.

## Core Components

### 1. SqlQueryOptimizer
The main entry point of the library, located in `SqlQueryOptimizer.cs`. It orchestrates the analysis process through these steps:
- Gets execution plan
- Collects performance metrics
- Analyzes execution plan
- Recommends indexes
- Checks statistics
- Generates HTML report

### 2. Analysis Components

#### QueryAnalyzer
Located in `QueryAnalyzer.cs`, provides:
- Execution plan retrieval
- Query performance metrics collection
- Real-time statistics gathering

#### ExecutionPlanAnalyzer
Located in `ExecutionPlanAnalyzer.cs`, identifies:
- Table scans
- Key lookups
- Expensive sort operations
- Implicit conversions

#### IndexRecommender
Located in `IndexRecommender.cs`, provides:
- Index recommendations based on query patterns
- Analysis of predicate columns
- Generation of CREATE INDEX statements

#### StatisticsAnalyzer
Located in `StatisticsAnalyzer.cs`, performs:
- Statistics freshness analysis
- Statistics update recommendations
- Statistics usage analysis

### 3. Reporting

#### HtmlReportGenerator
Located in `HtmlReportGenerator.cs`, generates:
- Formatted HTML reports
- Performance metrics visualization
- Optimization suggestions with impact levels
- Recommended actions

### 4. Models

#### OptimizationSuggestion
Located in `OptimizationSuggestion.cs`, defines:
- Suggestion types (CreateIndex, IncludeColumns, UpdateStatistics, QueryRewrite)
- Impact levels (Low, Medium, High)
- Detailed descriptions and recommended actions

## Key Features

1. **Query Analysis**
   - Execution plan analysis
   - Performance metrics collection
   - Statistics evaluation

2. **Optimization Recommendations**
   - Index creation suggestions
   - Statistics updates
   - Query rewrite recommendations
   - Performance impact assessment

3. **Report Generation**
   - Detailed HTML reports
   - Visual presentation of metrics
   - Prioritized recommendations
   - Actionable suggestions

## Dependencies
- Microsoft.Data.SqlClient (v6.0.2) for SQL Server connectivity
- .NET 8.0 framework

## Usage Example

```csharp
var optimizer = new SqlQueryOptimizer("connection_string");
string report = optimizer.AnalyzeAndOptimize("SELECT * FROM Users WHERE LastLoginDate > @date");
```

The library will analyze the query and return an HTML report containing performance metrics and optimization suggestions.