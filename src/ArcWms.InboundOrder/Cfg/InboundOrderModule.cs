using Autofac;
using NHibernateUtils;

namespace ArcWms.Cfg;

/// <summary>
/// 
/// </summary>
public class InboundOrderModule : Module
{
    Func<InboundOrder> _createInboundOrder = () => new InboundOrder();
    Func<InboundLine> _createInboundLine = () => new InboundLine();

    internal InboundOrderModule()
    {
    }

    public InboundOrderModule UseInboundOrder<T>()
        where T : InboundOrder, new()
    {
        _createInboundOrder = () => new T();
        return this;
    }

    public InboundOrderModule UseInboundLine<T>()
        where T : InboundLine, new()
    {
        _createInboundLine = () => new T();
        return this;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => _createInboundLine()).As<InboundLine>().InstancePerDependency();
        builder.Register(c => _createInboundOrder()).As<InboundOrder>().InstancePerDependency();

        builder.RegisterType<InboundOrderModelMapperConfigurator>().As<IModelMapperConfigurator>();
    }
}
