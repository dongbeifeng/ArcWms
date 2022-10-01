using Autofac;

namespace ArcWms.Cfg;

public static class InboundOrderContainerBuilderExtensions
{
    public static ContainerBuilder AddInboundOrder(this ContainerBuilder builder, Action<InboundOrderModule> configure)
    {
        InboundOrderModule module = new InboundOrderModule();
        configure?.Invoke(module);
        builder.RegisterModule(module);
        return builder;
    }
}
