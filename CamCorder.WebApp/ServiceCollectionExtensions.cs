using CamCorder.Common;
using Scrutor;
using System.Reflection;

namespace CamCorder.WebApp
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInjectables(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(classes => classes.WithAttribute<InjectableAttribute>())
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .As(type =>
                {
                    var attr = type.GetCustomAttribute<InjectableAttribute>();

                    if (attr?.ServiceTypes?.Length > 0)
                        return attr.ServiceTypes;

                    var interfaces = type.GetInterfaces();
                    return interfaces.Length > 0 ? interfaces : [type];
                })
                .WithLifetime(type =>
                {
                    var attr = type.GetCustomAttribute<InjectableAttribute>();
                    return attr?.Lifetime ?? ServiceLifetime.Scoped;
                }));

            return services;
        }
    }
}