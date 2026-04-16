using Microsoft.Extensions.DependencyInjection;

namespace CamCorder.Common.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class InjectableAttribute : Attribute
{
    protected InjectableAttribute(Type serviceType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
    }

    public Type ServiceType { get; }

    public ServiceLifetime Lifetime { get; }
}
