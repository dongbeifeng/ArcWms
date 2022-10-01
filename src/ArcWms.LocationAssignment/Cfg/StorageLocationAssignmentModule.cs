using Autofac;
using Microsoft.Extensions.Logging;

namespace ArcWms.Cfg;

/// <summary>
/// 向容器注册货位分配的服务。
/// </summary>
public class StorageLocationAssignmentModule : Module
{
    ILogger<StorageLocationAssignmentModule> _logger;

    List<Type> _ruleTypes = new List<Type>();

    internal StorageLocationAssignmentModule(ILogger<StorageLocationAssignmentModule> logger)
    {
        _logger = logger;
    }


    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SAllocationHelper>().AsSelf();
        foreach (var ruleType in _ruleTypes)
        {
            builder.RegisterType(ruleType).As<IRule>();
            _logger.LogInformation("已注册分配货位规则：{ruleType}", ruleType);
        }
    }

    public StorageLocationAssignmentModule AddRule<T>()
        where T : IRule
    {
        _ruleTypes.Add(typeof(T));
        return this;
    }
}


