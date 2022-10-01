using Autofac;
using NHibernateUtils;

namespace ArcWms.Cfg;

/// <summary>
/// 
/// </summary>
public class LocationModule : Module
{
    Func<Location> _createLocation = () => new Location();
    Func<Streetlet> _createStreetlet = () => new Streetlet();
    Func<Cell> _createCell = () => new Cell();

    internal LocationModule()
    {
    }

    public LocationModule UseLocation<T>() where T : Location, new()
    {
        _createLocation = () => new T();
        return this;
    }

    public LocationModule UseStreetlet<T>() where T : Streetlet, new()
    {
        _createStreetlet = () => new T();
        return this;
    }

    public LocationModule UseCell<T>() where T : Cell, new()
    {
        _createCell = () => new T();
        return this;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LocationHelper>();
        
        builder.Register(c => _createLocation()).As<Location>().InstancePerDependency();
        builder.Register(c => _createStreetlet()).As<Streetlet>().InstancePerDependency();
        builder.Register(c => _createCell()).As<Cell>().InstancePerDependency();
        
        builder.RegisterType<LocationModelMapperConfigurator>().As<IModelMapperConfigurator>();
    }
}
