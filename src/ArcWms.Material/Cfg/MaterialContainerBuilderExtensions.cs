using Autofac;

namespace ArcWms.Cfg;

public static class MaterialContainerBuilderExtensions
{
    public static ContainerBuilder AddMaterial(this ContainerBuilder builder, Action<MaterialModule> configure)
    {
        MaterialModule module = new MaterialModule();
        configure?.Invoke(module);
        builder.RegisterModule(module);
        return builder;
    }
}

