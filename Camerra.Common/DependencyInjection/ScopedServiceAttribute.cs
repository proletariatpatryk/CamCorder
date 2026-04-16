using Microsoft.Extensions.DependencyInjection;

namespace CamCorder.Common.DependencyInjection;

public class ScopedServiceAttribute : InjectableAttribute
{
    public ScopedServiceAttribute(Type? serviceType = null)
        : base(serviceType ?? typeof(object), ServiceLifetime.Scoped) { }
}
