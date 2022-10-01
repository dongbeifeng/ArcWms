namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 为出库单分配库存的操作参数。
/// </summary>
public class OutboundOrderAllocateStockArgs
{
    /// <summary>
    /// 要分配库存的出库单 Id。
    /// </summary>
    public int? OutboundOrderId { get; set; }

    /// <summary>
    /// 分配选项。
    /// </summary>
    public AllocateStockOptions Options { get; set; } = new AllocateStockOptions();
}

