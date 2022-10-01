using Autofac;
using NHibernateUtils;

namespace ArcWms.Cfg;

/// <summary>
/// 
/// </summary>
public class OutboundOrderModule : Module
{

    Func<OutboundOrder> _createOutboundOrder = () => new OutboundOrder();
    Func<OutboundLine> _createOutboundLine = () => new OutboundLine();
    Type _outboundOrderAllocatorType = typeof(DefaultOutboundOrderAllocator);


    internal OutboundOrderModule()
    {
    }


    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<OutboundOrderPickHelper>();

        builder.Register(c => _createOutboundLine()).As<OutboundLine>().InstancePerDependency();
        builder.Register(c => _createOutboundOrder()).As<OutboundOrder>().InstancePerDependency();

        builder.RegisterType(_outboundOrderAllocatorType).As<IOutboundOrderAllocator>();

        builder.RegisterType<OutboundOrderModelMapperConfigurator>().As<IModelMapperConfigurator>();
    }


    public OutboundOrderModule UseOutboundOrder<T>()
        where T : OutboundOrder, new()
    {
        _createOutboundOrder = () => new T();
        return this;
    }

    public OutboundOrderModule UseOutboundLine<T>()
        where T : OutboundLine, new()
    {
        _createOutboundLine = () => new T();
        return this;
    }

    public OutboundOrderModule UseOutboundOrderAllocator<T>()
        where T : IOutboundOrderAllocator
    {
        _outboundOrderAllocatorType = typeof(T);
        return this;
    }
}
