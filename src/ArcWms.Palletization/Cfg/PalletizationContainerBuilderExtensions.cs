using Autofac;

namespace ArcWms.Cfg;

public static class PalletizationContainerBuilderExtensions
{
    public static ContainerBuilder AddPalletization(this ContainerBuilder builder, Action<PalletizationModule> configure)
    {
        PalletizationModule module = new PalletizationModule();
        configure?.Invoke(module);
        builder.RegisterModule(module);
        return builder;
    }
}
