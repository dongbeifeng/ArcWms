using ArcWms.WebApi.MetaData;
using Autofac.Features.Indexed;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using NHibernateAspNetCoreFilters;
using OperationTypeAspNetCoreAuthorization;
using System.Diagnostics.CodeAnalysis;

namespace ArcWms.WebApi.Controllers;


[Route("api/[controller]")]
[ApiController]
public class DebugController : ControllerBase
{
    readonly ILogger<DebugController> _logger;
    readonly ISession _session;
    readonly IIndex<string, ICompletionHandler> _completionHandlers;
    readonly IIndex<string, IRequestHandler> _requestHandlers;


    public DebugController(ISession session, IIndex<string, ICompletionHandler> completionHandlers, IIndex<string, IRequestHandler> requestHandlers, ILogger<DebugController> logger)
    {
        _completionHandlers = completionHandlers;
        _requestHandlers = requestHandlers;
        _logger = logger;
        _session = session;
    }



    /// <summary>
    /// 模拟请求。
    /// </summary>
    /// <param name="requestInfo"></param>
    /// <returns></returns>
    [HttpPost("simulate-request")]
    [OperationType(OperationTypes.模拟请求)]
    [Transaction]
    public async Task<ApiData> SimulateRequest(RequestInfo requestInfo)
    {
        _logger.LogInformation("收到模拟请求 {requestInfo}", requestInfo);
        var handler = GetRequestHandler(requestInfo.RequestType);
        await handler.ProcessRequestAsync(requestInfo).ConfigureAwait(false);
        var op = await this.SaveOpAsync("{0}", requestInfo);
        _logger.LogInformation("模拟请求处理成功");
        return this.Success();
    }

    /// <summary>
    /// 模拟完成。
    /// </summary>
    /// <param name="taskInfo"></param>
    /// <returns></returns>
    [HttpPost("simulate-completion")]
    [OperationType(OperationTypes.模拟完成)]
    [Transaction]
    public async Task<ApiData> SimulateCompletion(CompletedTaskInfo taskInfo)
    {
        _logger.LogInformation("收到模拟任务完成 {taskInfo}", taskInfo);

        var task = await _session.Query<TransportTask>()
            .Where(x => x.TaskCode == taskInfo.TaskCode)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
        if (task is not null)
        {
            var handler = GetCompletionHandler(taskInfo.TaskType);
            await handler.ProcessCompletedTaskAsync(taskInfo, task).ConfigureAwait(false);
            _logger.LogInformation("模拟任务完成处理成功");
        }
        else
        {
            _logger.LogWarning("任务不存在");
        }
        var op = await this.SaveOpAsync("{0}", taskInfo);
        return this.Success();
    }


    [return: NotNull]
    internal IRequestHandler GetRequestHandler(string? requestType)
    {
        Guard.IsNotNullOrWhiteSpace(requestType, nameof(requestType));

        if (_requestHandlers.TryGetValue(requestType, out var handler) == false)
        {
            throw new InvalidRequestException($"不支持的请求类型 {requestType}。");
        }

        return handler;
    }


    [return: NotNull]
    internal ICompletionHandler GetCompletionHandler(string? taskType)
    {
        Guard.IsNotNullOrWhiteSpace(taskType, nameof(taskType));

        if (_completionHandlers.TryGetValue(taskType, out var handler) == false)
        {
            throw new CompleteTaskFailedException($"不支持的任务类型 {taskType}");
        }

        return handler;
    }

}

