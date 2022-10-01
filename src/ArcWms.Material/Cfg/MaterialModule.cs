using Autofac;
using NHibernateUtils;

namespace ArcWms.Cfg;

/// <summary>
/// 
/// </summary>
public class MaterialModule : Module
{
    InventoryKeyType _inventoryKeyType = InventoryKeyType.Create<InventoryKey>();
    Func<Material> _createMaterial = () => new Material();
    Func<Flow> _createFlow = () => new Flow();
    Type _batchServiceType = typeof(DefaultBatchService);
    Action<MaterialOptions>? _configureMaterialOptions;

    internal MaterialModule()
    {
    }


    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(_inventoryKeyType).SingleInstance();
        builder.RegisterType<FlowHelper>();

        builder.Register(c => _createMaterial()).As<Material>().InstancePerDependency();
        builder.Register(c => _createFlow()).As<Flow>().InstancePerDependency();


        builder.Register(c =>
        {
            var materialOptions = new MaterialOptions();
            _configureMaterialOptions?.Invoke(materialOptions);
            return materialOptions;
        });
        builder.RegisterType(_batchServiceType ?? throw new()).As<IBatchService>();
        builder.RegisterType<MaterialModelMapperConfigurator>().As<IModelMapperConfigurator>();
    }


    public MaterialModule UseInventoryKey<T>() where T : InventoryKey
    {
        _inventoryKeyType = InventoryKeyType.Create<T>();
        return this;
    }


    public MaterialModule UseMaterial<T>()
        where T : Material, new()
    {
        _createMaterial = () => new T();
        return this;
    }

    public MaterialModule UseBatchService<T>()
        where T : IBatchService
    {
        _batchServiceType = typeof(T);
        return this;
    }

    public MaterialModule Configure(Action<MaterialOptions>? configureMaterialOptions)
    {
        _configureMaterialOptions = configureMaterialOptions;
        return this;
    }

    public MaterialModule UseFlow<T>()
        where T : Flow, new()
    {
        _createFlow = () => new T();
        return this;
    }
}

