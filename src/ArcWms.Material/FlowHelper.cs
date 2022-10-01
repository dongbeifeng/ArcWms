using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;

namespace ArcWms;

/// <summary>
/// 操作流水的辅助函数。
/// </summary>
public sealed class FlowHelper
{
    readonly ISession _session;
    readonly Func<Flow> _createFlow;
    readonly ILogger<FlowHelper> _logger;

    public FlowHelper(ISession session, Func<Flow> createFlow, ILogger<FlowHelper> logger)
    {
        _session = session;
        _createFlow = createFlow;
        _logger = logger;
    }


    /// <summary>
    /// 创建流水并保存到数据库。
    /// </summary>
    /// <param name="inventoryKey">库存键。</param>
    /// <param name="direction">流动方向。</param>
    /// <param name="quantity">流水数量。负数表示冲销业务。</param>
    /// <param name="bizType">业务类型。</param>
    /// <param name="operationType">操作类型。</param>
    /// <param name="palletCode">托盘号。</param>
    /// <param name="orderCode">WMS单号。</param>
    /// <param name="bizOrder">ERP单号。</param>
    /// <param name="acceptNegativeBalance">指示是否接受负数的库存余额。</param>
    /// <returns></returns>
    /// <exception cref="NegativeBalanceException">当余额为负数，且 <paramref name="acceptNegativeBalance"/> 为 false 时引发。</exception>
    public async Task<Flow> GenerateFlowAsync(InventoryKey inventoryKey,
                                              FlowDirection direction,
                                              decimal quantity,
                                              BizType bizType,
                                              string? operationType,
                                              string? palletCode,
                                              string? orderCode = null,
                                              string? bizOrder = null,
                                              bool acceptNegativeBalance = false
                                              )
    {
        ArgumentNullException.ThrowIfNull(inventoryKey);
        ArgumentNullException.ThrowIfNull(bizType);

        if (direction == FlowDirection.NotSet)
        {
            throw new ArgumentException("参数值无效。", nameof(direction));
        }

        _logger.LogDebug("正在生成流水");

        _logger.LogDebug("库存键：{inventoryKey}", inventoryKey);
        _logger.LogDebug("业务类型：{bizType}", bizType);
        _logger.LogDebug("操作类型：{operationType}", operationType);
        _logger.LogDebug("方向：{flowDirection}", direction);
        _logger.LogDebug("托盘号：{palletCode}", palletCode);
        _logger.LogDebug("单号：{orderCode}", orderCode);
        _logger.LogDebug("业务单号：{bizCode}", bizOrder);

        var flow = _createFlow.Invoke();
        flow.SetInventoryKey(inventoryKey);
        flow.Quantity = quantity;
        flow.Direction = direction;
        flow.BizType = bizType.Value;
        flow.OperationType = operationType;
        flow.PalletCode = palletCode;
        flow.OrderCode = orderCode;
        flow.BizOrder = bizOrder;

        var prevBalance = await _session.Query<Flow>()
            .OfInventoryKey(inventoryKey)
            .OrderByDescending(x => x.FlowId)
            .Select(x => x.Balance)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        flow.Balance = prevBalance + flow.Direction switch
        {
            FlowDirection.Inbound => flow.Quantity,
            FlowDirection.Outbound => -flow.Quantity,
            _ => throw new(),
        };

        await _session.SaveAsync(flow).ConfigureAwait(false);

        _logger.LogInformation("已生成流水 {flowId}", flow.FlowId);

        if (flow.Balance < 0)
        {
            _logger.LogWarning("流水余额小于 0", flow.FlowId);

            if (!acceptNegativeBalance)
            {
                throw new NegativeBalanceException(inventoryKey);
            }
        }

        return flow;
    }
}
