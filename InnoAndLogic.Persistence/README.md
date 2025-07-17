# InnoAndLogic.Persistence

## Overview
`InnoAndLogic.Persistence` is a library designed to simplify database persistence and migrations for .NET projects using PostgreSQL. It provides tools for managing database connections, executing statements, handling transactions, and performing migrations.

## Features
- **Database Statement Execution**: Execute queries, non-queries, and batch commands with retry logic.
- **ID Generation**: Efficient ID generation using `DbmService`.
- **Database Migrations**: Apply migrations using the Evolve library.
- **Transaction Management**: Manage PostgreSQL transactions with `PostgresTransaction`.
- **Utilities**: Helper methods for nullable types and binary import/export.

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
```

#### Database Migrations
```csharp
using InnoAndLogic.Persistence.Migrations;

var migrations = new DbMigrations(loggerFactory, databaseOptions);
migrations.Up();
```

## License
This library is licensed under the MIT License. See the LICENSE file for details.

## Repository
For more information, visit the [GitHub repository](https://github.com/aaronlovi/InnoAndLogic).