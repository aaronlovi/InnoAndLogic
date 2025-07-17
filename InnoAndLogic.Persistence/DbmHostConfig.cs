using System.Collections.Generic;
using System.Reflection;
using InnoAndLogic.Persistence.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InnoAndLogic.Persistence;

public static class DbmHostConfig {
    /// <summary>
    /// Configures services for the DbmService and its dependencies.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The IConfiguration instance to bind options from.</param>
    /// <param name="sectionName">The name of the configuration section for DatabaseOptions.</param>
    public static IServiceCollection ConfigurePersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        IEnumerable<Assembly>? externalMigrationAssemblies = null) {
        var databaseOptions = new DatabaseOptions();
        IConfigurationSection section = configuration.GetSection(sectionName);
        section.Bind(databaseOptions);

        // Register DatabaseOptions
        return services.
            AddSingleton(databaseOptions).
            AddSingleton<PostgresExecutor>().
            AddSingleton(provider => new DbMigrations(
                provider.GetRequiredService<ILoggerFactory>(),
                databaseOptions,
                externalMigrationAssemblies)).
            AddSingleton<IDbmService, DbmService>(provider => {
                ILoggerFactory loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                PostgresExecutor executor = provider.GetRequiredService<PostgresExecutor>();
                DatabaseOptions options = provider.GetRequiredService<DatabaseOptions>();
                DbMigrations migrations = provider.GetRequiredService<DbMigrations>();

                return new DbmService(loggerFactory, executor, options, migrations);
            });
    }
}
