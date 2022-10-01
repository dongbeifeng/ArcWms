using Autofac.Features.OwnedInstances;
using NHibernate.Linq;

namespace ArcWms.WebApi.Handlers;

/// <summary>
/// 默认的出库单自动下架处理程序。此类只提供了最基础的实现：
/// * 未考虑拼板的托盘；
/// * 未考虑双深巷道的存货形态；
/// * 仅向 KP1 下发任务；
/// </summary>
public class DefaultOboMoveDownHandler : IAutoMoveDownHandler
{
    readonly ILogger<DefaultOboMoveDownHandler> _logger;
    readonly ISession _session;
    readonly Owned<ISession> _sessionToHandleError;
    readonly TaskHelper _taskHelper;
    readonly ITaskSender _taskSender;

    public DefaultOboMoveDownHandler(TaskHelper taskHelper, ITaskSender taskSender, ISession session, Owned<ISession> sessionToHandleError, ILogger<DefaultOboMoveDownHandler> logger)
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
            throw new NotSupportedException($"{nameof(DefaultOboMoveDownHandler)} 仅支持出库单自动下架。");
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

    /// <summary>
    /// 从货架上选取一个可以下架到指定出口的货载
    /// </summary>
    /// <param name="outlet"></param>
    /// <param name="unitloadAllocationTable"></param>
    /// <returns></returns>
    public async Task<Unitload?> SelectOneUnitloadAsync(IUnitloadAllocationTable unitloadAllocationTable, Outlet outlet)
    {
        var unitloads = await _session.Query<UnitloadItem>()
            .Where(x => x.Unitload.CurrentUat == unitloadAllocationTable)
            .OrderBy(x => x.Fifo)
            .Select(x => x.Unitload)
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


}



