using Microsoft.Extensions.DependencyInjection;

namespace CamCorder.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InjectableAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }
        public Type[] ServiceTypes { get; }

        public InjectableAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            Lifetime = lifetime;
            ServiceTypes = [];
        }

        public InjectableAttribute(Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            Lifetime = lifetime;
            ServiceTypes = [serviceType];
        }

        public InjectableAttribute(ServiceLifetime lifetime, params Type[] serviceTypes)
        {
            Lifetime = lifetime;
            ServiceTypes = serviceTypes ?? [];
        }
    }
    public class ScopedServiceAttribute(Type? serviceType = null) : InjectableAttribute(serviceType ?? typeof(object), ServiceLifetime.Scoped)
    {
    }

    public class SingletonServiceAttribute(Type? serviceType = null) : InjectableAttribute(serviceType ?? typeof(object), ServiceLifetime.Singleton)
    {
    }
}
