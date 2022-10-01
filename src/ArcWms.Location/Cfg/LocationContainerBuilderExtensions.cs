using Autofac;

namespace ArcWms.Cfg;

public static class LocationContainerBuilderExtensions
{
    public static ContainerBuilder AddLocation(this ContainerBuilder builder, Action<LocationModule> configure)
    {
        LocationModule module = new LocationModule();
        configure?.Invoke(module);
        builder.RegisterModule(module);
        return builder;
    }
}
