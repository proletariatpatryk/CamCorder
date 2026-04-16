using CamCorder.Common.DependencyInjection;
using CamCorder.Data;
using CamCorder.Data.Entities;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace CamCorder.WebApp;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCamCorderDependencies(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var sqliteDbPath = Path.Combine(environment.ContentRootPath, "App_Data", "hangfire.db");
        var sqliteDbDirectory = Path.GetDirectoryName(sqliteDbPath);

        if (!string.IsNullOrWhiteSpace(sqliteDbDirectory))
        {
            Directory.CreateDirectory(sqliteDbDirectory);
        }

        if (!File.Exists(sqliteDbPath))
        {
            using var _ = File.Create(sqliteDbPath);
        }

        var appDataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(appDataDirectory);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? $"Data Source={Path.Combine(appDataDirectory, "camcorder.db")}";

        services.AddDbContext<CamCorderContext>(options =>
            options.UseSqlite(connectionString));

        services.AddAttributedServices(
            typeof(DependencyInjectionExtensions).Assembly,
            typeof(CamCorderContext).Assembly,
            typeof(Performer).Assembly);

        services.AddHangfire(configurationBuilder =>
        {
            configurationBuilder
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(sqliteDbPath, new SQLiteStorageOptions());
        });

        services.AddHangfireServer(options =>
        {
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
        });

        return services;
    }

    private static IServiceCollection AddAttributedServices(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var implementationTypes = assemblies
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Select(type => type.AsType());

        foreach (var implementationType in implementationTypes)
        {
            var injectableAttributes = implementationType.GetCustomAttributes<InjectableAttribute>(inherit: false);

            foreach (var attribute in injectableAttributes)
            {
                var candidateServiceTypes = attribute.ServiceType == typeof(object)
                    ? implementationType.GetInterfaces()
                    : [attribute.ServiceType];

                var serviceTypes = candidateServiceTypes.Any()
                    ? candidateServiceTypes
                    : [implementationType];

                foreach (var serviceType in serviceTypes)
                {
                    if (!serviceType.IsAssignableFrom(implementationType))
                    {
                        continue;
                    }

                    services.TryAdd(ServiceDescriptor.Describe(serviceType, implementationType, attribute.Lifetime));
                }
            }
        }

        return services;
    }
}
