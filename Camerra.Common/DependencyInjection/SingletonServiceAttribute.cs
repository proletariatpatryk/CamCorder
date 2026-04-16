using Microsoft.Extensions.DependencyInjection;

namespace CamCorder.Common.DependencyInjection;

public class SingletonServiceAttribute : InjectableAttribute
{
    public SingletonServiceAttribute(Type? serviceType = null)
        : base(serviceType ?? typeof(object), ServiceLifetime.Singleton) { }
}
