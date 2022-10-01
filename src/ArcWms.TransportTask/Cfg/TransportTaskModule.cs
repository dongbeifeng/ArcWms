using Autofac;
using Microsoft.Extensions.Logging;
using NHibernateUtils;

namespace ArcWms.Cfg;

/// <summary>
/// 
/// </summary>
public class TransportTaskModule : Module
{
    ILogger<TransportTaskModule> _logger;

    public TransportTaskModule(ILogger<TransportTaskModule> logger)
    {
        _logger = logger;
    }


    List<(string requestType, Type handlerType)> _requestHandlers = new List<(string requestType, Type handlerType)>();
    List<(string taskType, Type handlerType)> _completionHandlers = new List<(string taskType, Type handlerType)>();
    Func<TransportTask> _createTransportTask = () => new TransportTask();
    Func<ArchivedTransportTask> _createArchivedTransportTask = () => new ArchivedTransportTask();
    Type _taskSenderType = typeof(DummyTaskSender);


    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TaskHelper>();
        builder.RegisterBuildCallback(c =>
        {
            TaskHelper.SupportedRequestTypes = _completionHandlers
                .Select(x => x.taskType)
                .ToList()
                .AsReadOnly();

            TaskHelper.SupportedRequestTypes = _requestHandlers
                .Select(x => x.requestType)
                .ToList()
                .AsReadOnly();
        });

        builder.RegisterType(_taskSenderType).As<ITaskSender>();

        builder.Register(c => _createTransportTask()).As<TransportTask>().InstancePerDependency();
        builder.Register(c => _createArchivedTransportTask()).As<ArchivedTransportTask>().InstancePerDependency();

        ConfigureRequestHandlers(builder);
        ConfigureCompletedTaskHandlers(builder);

        builder.RegisterType<TransportTaskModelMapperConfigurator>().As<IModelMapperConfigurator>();


    }


    public TransportTaskModule UseTransportTask<T>() where T : TransportTask, new()
    {
        _createTransportTask = () => new T();
        return this;
    }
    public TransportTaskModule UseArchivedTransportTask<T>() where T : ArchivedTransportTask, new()
    {
        _createArchivedTransportTask = () => new T();
        return this;
    }

    public TransportTaskModule AddRequestHandler<T>(string requestType) where T : IRequestHandler
    {
        if (string.IsNullOrWhiteSpace(requestType))
        {
            throw new ArgumentException("请求类型不能为空", nameof(requestType));
        }
        _requestHandlers.Add((requestType.Trim(), typeof(T)));
        return this;
    }

    public TransportTaskModule AddCompletedTaskHandler<T>(string taskType) where T : ICompletionHandler
    {
        if (string.IsNullOrWhiteSpace(taskType))
        {
            throw new ArgumentException("任务类型不能为空", nameof(taskType));
        }
        _completionHandlers.Add((taskType.Trim(), typeof(T)));
        return this;
    }

    public TransportTaskModule UseTaskSender<T>() where T : ITaskSender
    {
        _taskSenderType = typeof(T);
        return this;
    }


    void ConfigureRequestHandlers(ContainerBuilder builder)
    {
        _logger.LogInformation("正在配置请求处理程序");

        foreach (var (requestType, handlerType) in _requestHandlers)
        {
            builder.RegisterType(handlerType).Named<IRequestHandler>(requestType);

            _logger.LogInformation("  请求类型 {requestType} --> 处理程序 {handlerType}", requestType, handlerType);
        }

        _logger.LogInformation("已配置请求处理程序。");
    }


    void ConfigureCompletedTaskHandlers(ContainerBuilder builder)
    {
        _logger.LogInformation("正在配置完成处理程序");

        foreach (var (taskType, handlerType) in _completionHandlers)
        {
            builder.RegisterType(handlerType).Named<ICompletionHandler>(taskType);

            _logger.LogInformation("  任务类型 {taskType} --> 处理程序 {handlerType}", taskType, handlerType);
        }

        _logger.LogInformation("已配置完成处理程序");
    }
}
