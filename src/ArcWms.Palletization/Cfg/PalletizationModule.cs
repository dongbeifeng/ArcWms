using Autofac;
using NHibernateUtils;
using Module = Autofac.Module;

namespace ArcWms.Cfg;

/// <summary>
/// 用于向容器注册类型的扩展方法。在 Startup.ConfigureContainer 方法中调用。
/// </summary>
public class PalletizationModule : Module
{
    Func<Unitload> _createUnitload = () => new Unitload();
    Func<UnitloadItem> _createUnitloadItem = () => new UnitloadItem();
    Func<UnitloadSnapshot> _createUnitloadSnapshot = () => new UnitloadSnapshot();
    Func<UnitloadItemSnapshot> _createUnitloadItemSnapshot = () => new UnitloadItemSnapshot();
    IPalletCodeValidator? _palletCodeValidator;
    Type _unitloadStorageInfoProviderType = typeof(DefaultUnitloadStorageInfoProvider);


    internal PalletizationModule()
    {
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PalletizationHelper>();
        builder.RegisterType<UnitloadHelper>();
        builder.RegisterType(_unitloadStorageInfoProviderType).As<IUnitloadStorageInfoProvider>();

        builder.RegisterInstance(_palletCodeValidator ?? throw new())
            .As<IPalletCodeValidator>()
            .SingleInstance();

        builder.Register(c => _createUnitload()).As<Unitload>().InstancePerDependency();
        builder.Register(c => _createUnitloadItem()).As<UnitloadItem>().InstancePerDependency();
        builder.Register(c => _createUnitloadSnapshot()).As<UnitloadSnapshot>().InstancePerDependency();
        builder.Register(c => _createUnitloadItemSnapshot()).As<UnitloadItemSnapshot>().InstancePerDependency();

        builder.RegisterType<PalletizationModelMapperConfigurator>().As<IModelMapperConfigurator>();
    }


    public PalletizationModule UseUnitload<T>()
        where T : Unitload, new()
    {
        _createUnitload = () => new T();
        return this;
    }

    public PalletizationModule UseUnitloadItem<T>()
        where T : UnitloadItem, new()
    {
        _createUnitloadItem = () => new T();
        return this;
    }

    public PalletizationModule UseUnitloadSnapshot<T>()
        where T : UnitloadSnapshot, new()
    {
        _createUnitloadSnapshot = () => new T();
        return this;
    }

    public PalletizationModule UseUnitloadItemSnapshot<T>()
        where T : UnitloadItemSnapshot, new()
    {
        _createUnitloadItemSnapshot = () => new T();
        return this;
    }


    public PalletizationModule UsePalletCodeValidator(IPalletCodeValidator palletCodeValidator)
    {
        _palletCodeValidator = palletCodeValidator;
        return this;
    }

    public PalletizationModule UseUnitloadStorageInfoProvider<T>()
        where T : IUnitloadStorageInfoProvider
    {
        _unitloadStorageInfoProviderType = typeof(T);
        return this;
    }

}
