using ArcWms.WebApi.MetaData;
using Autofac.Features.Indexed;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using NHibernateAspNetCoreFilters;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;
using System.Diagnostics.CodeAnalysis;

namespace ArcWms.WebApi.Controllers;


/// <summary>
/// 提供任务 api
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TskController : ControllerBase
{
    readonly ILogger<TskController> _logger;
    readonly ISession _session;
    readonly TaskHelper _taskHelper;
    readonly ITaskSender _taskSender;
    readonly IIndex<string, IAutoMoveDownHandler> _autoMoveDownHandlers;

    /// <summary>
    /// 初始化新实例
    /// </summary>
    /// <param name="session"></param>
    /// <param name="taskHelper"></param>
    /// <param name="taskSender"></param>
    /// <param name="logger"></param>
    public TskController(ISession session, TaskHelper taskHelper, ITaskSender taskSender, IIndex<string, IAutoMoveDownHandler> autoMoveDownHandlers, ILogger<TskController> logger)
    {
        _logger = logger;
        _taskHelper = taskHelper;
        _session = session;
        _taskSender = taskSender;
        _autoMoveDownHandlers = autoMoveDownHandlers;
    }

    /// <summary>
    /// 任务列表
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-task-list")]
    [OperationType(OperationTypes.查看任务)]
    public async Task<ListData<TaskInfo>> GetTaskList(TaskListArgs args)
    {
        var pagedList = await _session.Query<TransportTask>().SearchAsync(args, "TaskId DESC", 1, 999);

        return this.ListData(pagedList, x => new TaskInfo
        {
            TaskId = x.TaskId,
            TaskCode = x.TaskCode,
            TaskType = x.TaskType,
            PalletCode = x.Unitload.PalletCode,
            StartLocationCode = x.Start.LocationCode,
            EndLocationCode = x.End.LocationCode,
            SendTime = x.SendTime,
            OrderCode = x.OrderCode,
            Comment = x.Comment,
            Items = x.Unitload.Items.Select(i => DtoConvert.ToUnitloadItemInfo(i)).ToList(),
        });
    }

    /// <summary>
    /// 历史任务列表
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-archived-task-list")]
    [OperationType(OperationTypes.查看任务)]
    public async Task<ListData<ArchivedTaskInfo>> GetArchivedTaskList(ArchivedTaskListArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Sort))
        {
            args.Sort = "TaskId DESC";
        }

        var pagedList = await _session.Query<ArchivedTransportTask>().SearchAsync(args, args.Sort, args.Current, args.PageSize).ConfigureAwait(false);

        return this.ListData(pagedList, x => new ArchivedTaskInfo
        {
            TaskId = x.TaskId,
            TaskCode = x.TaskCode,
            TaskType = x.TaskType,
            PalletCode = x.Unitload.PalletCode,
            StartLocationCode = x.Start.LocationCode,
            EndLocationCode = x.End.LocationCode,
            SendTime = x.SendTime,
            OrderCode = x.OrderCode,
            Comment = x.Comment,
            ArchivedAt = x.ArchivedAt,
            Cancelled = x.Cancelled,
            Items = x.Unitload.Items.Select(i => DtoConvert.ToUnitloadItemInfo(i)).ToList(),
        });
    }

    /// <summary>
    /// 更改货载位置。
    /// </summary>
    /// <param name="args">操作参数。</param>
    /// <returns></returns>
    [Transaction]
    [OperationType(OperationTypes.更改货载位置)]
    [HttpPost("change-unitload-location")]
    public async Task<ApiData> ChangeUnitloadLocation(ChangeLocationArgs args)
    {
        string palletCode = args.PalletCode;

        Unitload unitload = await _session.Query<Unitload>().Where(x => x.PalletCode == palletCode).SingleOrDefaultAsync().ConfigureAwait(false);

        if (unitload == null)
        {
            throw new InvalidOperationException("托盘号不存在。");
        }

        Location dest = await _session.Query<Location>().Where(x => x.LocationCode == args.DestinationLocationCode).SingleOrDefaultAsync().ConfigureAwait(false);
        if (dest == null)
        {
            throw new Exception("货位号不存在。");
        }

        var originalLocationCode = unitload.CurrentLocation?.LocationCode;
        if (originalLocationCode == null)
        {
            originalLocationCode = LocationCodes.N;
        }

        var archived = await _taskHelper.ChangeUnitloadLocationAsync(unitload, dest, args.Comment + string.Format("user: {0}", this.User?.Identity?.Name ?? "-")).ConfigureAwait(false);

        _ = await this.SaveOpAsync("任务号 {0}", archived.TaskCode).ConfigureAwait(false);

        _logger.LogInformation("已将托盘 {palletCode} 的位置从 {originalLocationCode} 改为 {destinationLocationCode}", palletCode, originalLocationCode, args.DestinationLocationCode);

        return this.Success();
    }


    /// <summary>
    /// 获取任务类型
    /// </summary>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-task-type-options")]
    public async Task<OptionsData<string>> GetTaskTypeOptions()
    {
        return await Task.FromResult(this.OptionsData(_taskHelper.GetTaskTypes().ToList()));
    }

    /// <summary>
    /// 创建手工任务。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [Transaction]
    [OperationType(OperationTypes.手工任务)]
    [HttpPost("create-manual-task")]
    public async Task<ApiData> CreateManualTask(CreateManualTaskArgs args)
    {
        Unitload unitload = await _session.Query<Unitload>().Where(x => x.PalletCode == args.PalletCode).SingleOrDefaultAsync();

        if (unitload == null)
        {
            throw new InvalidOperationException("托盘号不存在。");
        }

        Location start = await _session.Query<Location>().Where(x => x.LocationCode == args.FromLocationCode).SingleOrDefaultAsync();
        if (start == null)
        {
            throw new Exception("起点不存在。");
        }

        Location dest = await _session.Query<Location>()
            .Where(x => x.LocationCode == args.ToLocationCode)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
        if (dest == null)
        {
            throw new Exception("终点不存在。");
        }

        TransportTask transportTask = await _taskHelper.CreateAsync(
            args.TaskType ?? throw new Exception(),
            start,
            dest,
            unitload
            ).ConfigureAwait(false);
        transportTask.Comment = $"手工任务：{args.Comment}，user: {this.User?.Identity?.Name ?? "-"}";
        await _taskSender.SendTaskAsync(transportTask);

        return this.Success();
    }


    /// <summary>
    /// 检查出口，实现自动下架
    /// </summary>
    /// <returns></returns>
    [Transaction]
    [HttpPost("check-outlet")]
    public async Task<string> CheckOutlet(string outletCode)
    {
        Outlet? outlet = await _session.Query<Outlet>()
            .SingleOrDefaultAsync(x => x.OutletCode == outletCode)
            .ConfigureAwait(false);
        if (outlet is null)
        {
            return $"出口 {outletCode} 不存在";
        }
        if (outlet.CurrentUat is null)
        {
            return $"出口 {outletCode} 上没有单据";
        }

        IUnitloadAllocationTable uat = (IUnitloadAllocationTable)outlet.CurrentUat;
        var handler = GetAutoMoveDownHandler(uat.GetOrderType());
        return await handler.CheckAsync(uat, outlet).ConfigureAwait(false);
    }

    [return: NotNull]
    internal IAutoMoveDownHandler GetAutoMoveDownHandler(string? orderType)
    {
        Guard.IsNotNullOrWhiteSpace(orderType, nameof(orderType));

        if (_autoMoveDownHandlers.TryGetValue(orderType, out var handler) == false)
        {
            throw new CompleteTaskFailedException($"不支持的单据类型 {orderType}");
        }

        return handler;
    }
    
}



