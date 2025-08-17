using System;
using System.Collections.Generic;
using System.Reflection;
using EvolveDb;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace InnoAndLogic.Persistence.Migrations;

/// <summary>
/// Handles database migrations for the application.
/// </summary>
/// <remarks>
/// This class uses the Evolve library to apply database migrations.
/// Migrations are scripts that update the structure of the database,
/// and are located in the "Migrations" directory.
/// The connection string and database schema are provided via the application's configuration.
/// </remarks>
public class DbMigrations {
    private readonly ILogger<DbMigrations> _logger;
    private readonly string _connectionString;
    private readonly string _databaseSchema;
    private readonly List<Assembly> _externalMigrationAssemblies;
    private readonly Dictionary<string, string> _placeholders;

    /// <summary>
    /// Handles database migrations for the application.
    /// </summary>
    /// <remarks>
    /// This class uses the Evolve library to apply database migrations.
    /// Migrations are scripts that update the structure of the database,
    /// and are located in the "Migrations" directory.
    /// The connection string and database schema are provided via the application's configuration.
    /// </remarks>
    /// <param name="logsFactory">The <see cref="ILoggerFactory"/> used to create loggers for migration operations.</param>
    /// <param name="options">The <see cref="DatabaseOptions"/> containing the connection string and schema information.</param>
    /// <param name="externalMigrationAssemblies">
    /// An optional collection of assemblies containing additional embedded migration scripts.
    /// If not provided, only the default assembly is used.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the connection string or database schema is empty.
    /// </exception>
    public DbMigrations(
        ILoggerFactory logsFactory,
        DatabaseOptions options,
        IEnumerable<Assembly>? externalMigrationAssemblies = null) {
        _logger = logsFactory.CreateLogger<DbMigrations>();

        _connectionString = options.ConnectionString;
        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException("Connection string is empty");

        _databaseSchema = options.DatabaseSchema;
        if (string.IsNullOrEmpty(_databaseSchema))
            throw new InvalidOperationException("Database schema is empty");

        _externalMigrationAssemblies = [typeof(DbMigrations).Assembly];
        if (externalMigrationAssemblies != null)
            _externalMigrationAssemblies.AddRange(externalMigrationAssemblies);

        _placeholders = new Dictionary<string, string>(options.Placeholders);
    }

    /// <summary>
    /// Applies all pending database migrations.
    /// </summary>
    /// <remarks>
    /// This method uses the Evolve library to apply migrations. It logs the progress and reloads
    /// database types after migrations are applied to ensure proper introspection.
    /// </remarks>
    /// <exception cref="Exception">
    /// Thrown if the migration process fails.
    /// </exception>
    public void Up() {
        try {
            using var connection = new NpgsqlConnection(_connectionString);
            var evolve = new Evolve(connection, msg => _logger.LogInformation("Evolve: {msg}", msg)) {
                EmbeddedResourceAssemblies = _externalMigrationAssemblies,
                Schemas = [_databaseSchema],
                IsEraseDisabled = true,
                EnableClusterMode = true,
                Placeholders = _placeholders
            };
            evolve.Migrate();

            // Reload types to get proper introspection
            connection.Open();
            connection.ReloadTypes();
        } catch (Exception ex) {
            _logger.LogError(ex, "Database migration failed");
            throw;
        }
    }
}
