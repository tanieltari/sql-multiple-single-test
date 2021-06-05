## Useful tools
- [Bombardier](https://github.com/codesenberg/bombardier)
- [SQL Server Profiler](https://docs.microsoft.com/en-us/sql/tools/sql-server-profiler/sql-server-profiler)

## About
A simple test to check if constructing entity with a single query (using JOIN) is much more effective than doing it with multiple queries.\
This is only for my own testing purposes, in real world application I would use EF Core for data access.

## Multiple queries and caching
This approach seems to be the best for me. Code is much more readable and the performance is same to single query if caching is implemented.
