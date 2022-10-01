using ArcWms.WebApi.MetaData;
using Autofac.Features.Indexed;
using Autofac.Features.OwnedInstances;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using NHibernateAspNetCoreFilters;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;

namespace ArcWms.WebApi.Controllers;


/// <summary>
/// 提供任务 api
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TskController : ControllerBase
{
    readonly ILogger _logger;
    readonly ISession _session;
    readonly TaskHelper _taskHelper;
    readonly ITaskSender _taskSender;
    readonly TaskTypesProvider _taskTypesProvider;

    /// <summary>
    /// 初始化新实例
    /// </summary>
    /// <param name="session"></param>
    /// <param name="taskHelper"></param>
    /// <param name="taskSender"></param>
    /// <param name="logger"></param>
    /// <param name="taskTypesProvider"></param>
    public TskController(ISession session, TaskHelper taskHelper, ITaskSender taskSender, TaskTypesProvider taskTypesProvider, ILogger logger)
    {
        _logger = logger;
        _taskHelper = taskHelper;
        _session = session;
        _taskSender = taskSender;
        _taskTypesProvider = taskTypesProvider;
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
        return await Task.FromResult(this.OptionsData(_taskTypesProvider.TaskTypes.ToList()));
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


    readonly IIndex<string, IAutoMoveDownHandler> _autoMoveDownHandlers;

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
        var handler = _autoMoveDownHandlers[uat.GetOrderType()];
        return await handler.CheckAsync(uat, outlet).ConfigureAwait(false);
    }
}

/// <summary>
/// 自动下架处理程序。
/// </summary>
public interface IAutoMoveDownHandler
{
    /// <summary>
    /// 检查一次。
    /// </summary>
    /// <param name="unitloadAllocationTable"></param>
    /// <param name="outlet"></param>
    /// <returns></returns>
    Task<string> CheckAsync(IUnitloadAllocationTable unitloadAllocationTable, Outlet outlet);
}


/// <summary>
/// 默认的出货口检查程序。此类只提供了最基础的实现：
/// * 未考虑不同单据类型
/// * 未考虑托盘下架次序
/// * 仅向 KP1 下发任务
/// </summary>
public class OboMoveDownHandler : IAutoMoveDownHandler
{
    readonly ILogger<OboMoveDownHandler> _logger;
    readonly ISession _session;
    readonly Owned<ISession> _sessionToHandleError;
    readonly TaskHelper _taskHelper;
    readonly ITaskSender _taskSender;

    public OboMoveDownHandler(TaskHelper taskHelper, ITaskSender taskSender, ISession session, Owned<ISession> sessionToHandleError, ILogger<OboMoveDownHandler> logger)
    {
        _sessionToHandleError = sessionToHandleError;
        _taskHelper = taskHelper;
        _taskSender = taskSender;
        _session = session;
        _logger = logger;
    }


    /// <summary>
    /// 检查出口下架。
    /// </summary>
    /// <returns></returns>
    public async Task<string> CheckAsync(IUnitloadAllocationTable unitloadAllocationTable, Outlet outlet)
    {
        _logger.LogDebug("出库单自动下架");

        if (unitloadAllocationTable is not OutboundOrder)
        {
            throw new NotSupportedException($"{nameof(OboMoveDownHandler)} 仅支持出库单自动下架。");
        }

        int outletId = outlet.OutletId;

        try
        {
            _logger.LogDebug("{outletCode} - {uat}", outlet.OutletCode, outlet.CurrentUat);

            Location[] keypoints = new[] { outlet.KP1, outlet.KP2 }.Where(x => x != null).ToArray()!;
            if (keypoints.All(x => x.IsInboundDisabled))
            {
                outlet.LastCheckTime = DateTime.Now;
                outlet.LastCheckMessage = "所有关键点都已禁止入站";
                _logger.LogWarning(outlet.LastCheckMessage);
                return outlet.LastCheckMessage;
            }

            if (keypoints.All(x => x.InboundCount >= x.InboundLimit))
            {
                outlet.LastCheckTime = DateTime.Now;
                outlet.LastCheckMessage = "所有关键点都已达到最大入站数";
                _logger.LogInformation(outlet.LastCheckMessage);
                return outlet.LastCheckMessage;
            }

            Unitload? unitload = await SelectOneUnitloadAsync(unitloadAllocationTable, outlet);
            if (unitload == null)
            {
                outlet.LastCheckTime = DateTime.Now;
                outlet.LastCheckMessage = "没有可从此出口下架的托盘";
                _logger.LogDebug(outlet.LastCheckMessage);
                return outlet.LastCheckMessage;
            }

            TransportTask transportTask = await _taskHelper.CreateAsync(
                "下架", // TODO 处理写死的任务类型
                unitload.CurrentLocation ?? throw new(),
                outlet.KP1 ?? throw new(),  // TODO 处理双排出货口
                unitload
                )
                .ConfigureAwait(false);
            transportTask.OrderCode = outlet.CurrentUat?.ToString();
            await _taskSender.SendTaskAsync(transportTask).ConfigureAwait(false);

            outlet.LastCheckTime = DateTime.Now;
            outlet.LastCheckMessage = $"已下发任务 {transportTask.TaskCode}";
            _logger.LogInformation(outlet.LastCheckMessage);

            return outlet.LastCheckMessage;
        }
        catch (Exception ex)
        {
            // 出现错误时，用第二个 session 将问题记录到出口上
            return await WriteErrorMessageAsync(ex, outlet.OutletId).ConfigureAwait(false);
        }

    }

    private async Task<string> WriteErrorMessageAsync(Exception ex, int outletId)
    {
        _logger.LogError(ex, "出库单自动下架时出现错误");

        using var tx = _sessionToHandleError.Value.BeginTransaction();

        var outlet = await _sessionToHandleError.Value.GetAsync<Outlet>(outletId).ConfigureAwait(false);

        outlet.LastCheckTime = DateTime.Now;
        outlet.LastCheckMessage = "错误：" + ex.Message;
        _logger.LogInformation(outlet.LastCheckMessage);

        await tx.CommitAsync().ConfigureAwait(false);
        _sessionToHandleError.Dispose();

        return outlet.LastCheckMessage;
    }

    /// <summary>
    /// 从货架上选取一个可以下架到指定出货口的货载
    /// </summary>
    /// <param name="outlet"></param>
    /// <param name="unitloadAllocationTable"></param>
    /// <returns></returns>
    public virtual async Task<Unitload?> SelectOneUnitloadAsync(IUnitloadAllocationTable unitloadAllocationTable, Outlet outlet)
    {
        var unitloads = await _session.Query<Unitload>()
            .Where(x => x.CurrentUat == unitloadAllocationTable)
            .ToListAsync()
            .ConfigureAwait(false);
        foreach (var unitload in unitloads)
        {
            _logger.LogDebug("查看托盘 {palletCode}", unitload.PalletCode);

            if (unitload.HasTask)
            {
                _logger.LogDebug("托盘已有任务");
                continue;
            }

            if (unitload.CurrentLocation == null)
            {
                throw new();
            }

            _logger.LogDebug("所在位置 {locationCode}", unitload.CurrentLocation.LocationCode);

            if (unitload.CurrentLocation.LocationType != LocationTypes.S)
            {
                _logger.LogDebug("托盘不在库内");
                continue;
            }

            if (unitload.CurrentLocation.IsOutboundDisabled)
            {
                _logger.LogWarning("托盘所在位置 {locationCode} 已禁止出站", unitload.CurrentLocation.LocationCode);
            }

            var streetlet = unitload.CurrentLocation.Streetlet;
            if (streetlet == null)
            {
                throw new();
            }

            if (streetlet.IsOutboundDisabled)
            {
                _logger.LogWarning("托盘所在巷道 {streetletCode} 已禁止出站", streetlet.StreetletCode);
                continue;
            }

            if (streetlet.Outlets.Contains(outlet) == false)
            {
                _logger.LogWarning("从 {streetletCode} 不能到达 {outletCode}", streetlet.StreetletCode, outlet.OutletCode);
                continue;
            }

            return unitload;
        }

        _logger.LogDebug("已查看单据下所有托盘，没有可下架的托盘");
        return null;

    }


}



