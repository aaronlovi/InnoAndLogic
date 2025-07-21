# InnoAndLogic.Persistence

## Overview
`InnoAndLogic.Persistence` is a library designed to simplify database persistence and migrations for .NET projects using PostgreSQL. It provides tools for managing database connections, executing statements, handling transactions, and performing migrations.

## Features
- **Database Statement Execution**: Execute queries, non-queries, and batch commands with retry logic.
- **ID Generation**: Efficient ID generation using `DbmService`.
- **Database Migrations**: Apply migrations using the Evolve library.
- **Transaction Management**: Manage PostgreSQL transactions with `PostgresTransaction`.
- **Utilities**: Helper methods for nullable types and binary import/export.

## Public Types
The library includes several public types to facilitate database operations. Below is a detailed description of the key types:

### `DbUtils`
A static utility class that provides helper methods for working with Npgsql parameters.

#### Methods
- `CreateNullableDateTimeParam(string paramName, DateTimeOffset? nullableDate)`: Creates an Npgsql parameter for a nullable `DateTimeOffset` value.

### `DbStmtBase`
An abstract base class for database statements that bind parameters.

#### Methods
- `GetBoundParameters()`: Returns a read-only collection of Npgsql parameters bound to the database statement.

### `QueryDbStmtBase`
An abstract base class for query database statements executed against a PostgreSQL database.

#### Key Features
- Executes SQL queries asynchronously.
- Processes result sets row by row.

#### Methods
- `Execute(NpgsqlConnection conn, CancellationToken ct)`: Executes the SQL query and processes the result set.
- `ClearResults()`: Clears any results or state from a previous query execution.
- `BeforeRowProcessing(NpgsqlDataReader reader)`: Performs setup before processing rows.
- `AfterLastRowProcessing()`: Performs cleanup after processing all rows.
- `ProcessCurrentRow(NpgsqlDataReader reader)`: Processes the current row in the result set.

### `NonQueryDbStmtBase`
An abstract base class for non-query database statements executed against a PostgreSQL database.

#### Methods
- `Execute(NpgsqlConnection conn, CancellationToken ct)`: Executes the non-query statement asynchronously.

### `NonQueryBatchedDbStmtBase`
An abstract base class for batched non-query database statements executed against a PostgreSQL database.

#### Methods
- `Execute(NpgsqlConnection conn, CancellationToken ct)`: Executes the batch of non-query statements asynchronously.
- `AddCommandToBatch(string sql, IReadOnlyCollection<NpgsqlParameter> boundParams)`: Adds a command to the batch for execution.

### `BulkInsertDbStmtBase<T>`
An abstract base class for bulk insert database statements executed against a PostgreSQL database.

#### Generic Type Parameter
- `T`: The type of the items to be inserted.

#### Methods
- `GetCopyCommand()`: Returns the SQL COPY command used for the bulk insert operation.
- `WriteItemAsync(NpgsqlBinaryImporter writer, T item)`: Writes an individual item to the binary importer.
- `Execute(NpgsqlConnection conn, CancellationToken ct)`: Executes the bulk insert operation asynchronously.

### `IDbmService`
An interface that defines database management services, including ID generation.

#### Methods
- `GetNextId64(CancellationToken ct)`: Gets the next available 64-bit ID.
- `GetIdRange64(uint count, CancellationToken ct)`: Gets a range of 64-bit IDs.

### `DbmService`
A concrete implementation of `IDbmService` that provides database management services, including ID generation and database migrations.

#### Key Features
- Efficient ID generation with thread safety.
- Automatic database migrations during initialization.

#### Methods
- `GetNextId64(CancellationToken ct)`: Gets the next available 64-bit ID.
- `GetIdRange64(uint count, CancellationToken ct)`: Gets a range of 64-bit IDs.

## Usage
This library is published as a NuGet package and can be used in various .NET applications.

### Installation
To install the NuGet package, use the following command:

```bash
dotnet add package InnoAndLogic.Persistence
```

### Example Usage
#### Configuring Services
```csharp
using InnoAndLogic.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

services.ConfigurePersistenceServices(configuration, "DatabaseOptions");
```

#### ID Generation
```csharp
using InnoAndLogic.Persistence;

var dbmService = serviceProvider.GetRequiredService<IDbmService>();
var nextId = await dbmService.GetNextId64(CancellationToken.None);
var idRangeStart = await dbmService.GetIdRange64(10, CancellationToken.None);
```

#### Extending `IDbmService` and `DbmService`
You can extend `IDbmService` and `DbmService` to add custom functionality for your application. For example:

```csharp
public interface ICustomDbmService : IDbmService {
    Task<string> GenerateCustomIdAsync(CancellationToken ct);
}

public class CustomDbmService : DbmService, ICustomDbmService {
    public CustomDbmService(
        ILoggerFactory loggerFactory,
        PostgresExecutor exec,
        DatabaseOptions options,
        DbMigrations migrations)
        : base(loggerFactory, exec, options, migrations) { }

    public async Task<string> GenerateCustomIdAsync(CancellationToken ct) {
        ulong nextId = await GetNextId64(ct);
        return $"CUSTOM-{nextId}";
    }
}
```

#### Database Migrations
```csharp
using InnoAndLogic.Persistence.Migrations;

var migrations = new DbMigrations(loggerFactory, databaseOptions);
migrations.Up();
```

#### Executing a Query Statement
```csharp
using InnoAndLogic.Persistence.Statements;
using Npgsql;

public class SampleQuery : QueryDbStmtBase {
    public SampleQuery() : base("SELECT * FROM SampleTable", nameof(SampleQuery)) { }

    protected override IReadOnlyCollection<NpgsqlParameter> GetBoundParameters() => Array.Empty<NpgsqlParameter>();

    protected override bool ProcessCurrentRow(NpgsqlDataReader reader) {
        // Process each row here
        return true;
    }

    protected override void ClearResults() {
        // Clear any previous results
    }
}

var query = new SampleQuery();
var result = await query.Execute(connection, CancellationToken.None);
```

#### Executing a Non-Query Statement
```csharp
using InnoAndLogic.Persistence.Statements;
using Npgsql;

public class SampleNonQuery : NonQueryDbStmtBase {
    public SampleNonQuery() : base("UPDATE SampleTable SET Column = @Value", nameof(SampleNonQuery)) { }

    protected override IReadOnlyCollection<NpgsqlParameter> GetBoundParameters() =>
        new[] { new NpgsqlParameter("@Value", 42) };
}

var nonQuery = new SampleNonQuery();
var result = await nonQuery.Execute(connection, CancellationToken.None);
```

#### Bulk Insert
```csharp
using InnoAndLogic.Persistence.Statements;
using Npgsql;

public class SampleBulkInsert : BulkInsertDbStmtBase<MyEntity> {
    public SampleBulkInsert(IReadOnlyCollection<MyEntity> items) : base(nameof(SampleBulkInsert), items) { }

    protected override string GetCopyCommand() => "COPY SampleTable (Column1, Column2) FROM STDIN (FORMAT BINARY)";

    protected override async Task WriteItemAsync(NpgsqlBinaryImporter writer, MyEntity item) {
        await writer.WriteAsync(item.Column1);
        await writer.WriteAsync(item.Column2);
    }
}

var bulkInsert = new SampleBulkInsert(myEntities);
var result = await bulkInsert.Execute(connection, CancellationToken.None);
```

## License
This library is licensed under the MIT License. See the LICENSE file for details.

## Repository
For more information, visit the [GitHub repository](https://github.com/aaronlovi/InnoAndLogic).