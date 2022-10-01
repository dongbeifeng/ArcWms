namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 出库单取消分配的操作参数。
/// </summary>
public class OutboundOrderDeallocatePalletsArgs
{
    /// <summary>
    /// 要取消分配库存的出库单 Id。
    /// </summary>
    public int? OutboundOrderId { get; set; }

    /// <summary>
    /// 要从出库单取消分配的托盘号。
    /// </summary>
    public string[]? PalletCodes { get; set; }
}

