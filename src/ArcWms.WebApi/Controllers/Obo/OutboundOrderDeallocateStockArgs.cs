namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 为出库单取消分配库存的操作参数。
/// </summary>
public class OutboundOrderDeallocateStockArgs
{
    /// <summary>
    /// 要取消分配库存的出库单 Id。
    /// </summary>
    public int? OutboundOrderId { get; set; }
}

