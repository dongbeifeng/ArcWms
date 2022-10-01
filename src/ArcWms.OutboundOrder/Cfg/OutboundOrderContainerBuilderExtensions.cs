using Autofac;

namespace ArcWms.Cfg;

public static class OutboundOrderContainerBuilderExtensions
{
    public static ContainerBuilder AddOutboundOrder(this ContainerBuilder builder, Action<OutboundOrderModule> configure)
    {
        OutboundOrderModule module = new OutboundOrderModule();
        configure?.Invoke(module);
        builder.RegisterModule(module);
        return builder;
    }
}
