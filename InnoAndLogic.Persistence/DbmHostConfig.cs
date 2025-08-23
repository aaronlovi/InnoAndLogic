using System;
using System.Collections.Generic;
using System.Reflection;
using InnoAndLogic.Persistence.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Provides configuration methods for setting up database persistence services.
/// </summary>
public static class DbmHostConfig {
    /// <summary>
    /// Configures services for the DbmService or DbmInMemoryService based on the database provider specified in <see cref="DatabaseOptions"/>.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The IConfiguration instance to bind options from.</param>
    /// <param name="sectionName">The name of the configuration section for DatabaseOptions.</param>
    /// <param name="externalMigrationAssemblies">
    /// An optional collection of assemblies containing additional embedded migration scripts.
    /// If not provided, only the default assembly is used.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the configured services.</returns>
    public static IServiceCollection ConfigurePersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        IEnumerable<Assembly>? externalMigrationAssemblies = null) {
        var databaseOptions = new DatabaseOptions();
        IConfigurationSection section = configuration.GetSection(sectionName);
        section.Bind(databaseOptions);

        _ = services.AddSingleton(databaseOptions);

        return databaseOptions.Provider switch {
            DatabaseProvider.InMemory => ConfigureInMemoryServices(services),
            DatabaseProvider.Postgres => ConfigurePostgresServices(services, databaseOptions, externalMigrationAssemblies),
            _ => throw new InvalidOperationException($"Unsupported database provider: {databaseOptions.Provider}")
        };
    }

    private static IServiceCollection ConfigureInMemoryServices(IServiceCollection services) =>
        services.AddSingleton<IDbmService, DbmInMemoryService>(provider => {
            ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return new DbmInMemoryService(loggerFactory);
        });

    private static IServiceCollection ConfigurePostgresServices(
        IServiceCollection services,
        DatabaseOptions databaseOptions,
        IEnumerable<Assembly>? externalMigrationAssemblies) =>
        services
            .AddSingleton<PostgresExecutor>()
            .AddSingleton(provider => new DbMigrations(
                provider.GetRequiredService<ILoggerFactory>(),
                databaseOptions,
                externalMigrationAssemblies))
            .AddSingleton<IDbmService, DbmService>(provider => {
                ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                PostgresExecutor executor = provider.GetRequiredService<PostgresExecutor>();
                DbMigrations migrations = provider.GetRequiredService<DbMigrations>();

                return new DbmService(loggerFactory, executor, databaseOptions, migrations);
            });
}
