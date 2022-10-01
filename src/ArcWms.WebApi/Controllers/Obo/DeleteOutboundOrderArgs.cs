namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 删除出库单的操作参数。
/// </summary>
public class DeleteOutboundOrderArgs
{
    /// <summary>
    /// 要删除的出库单 Id。
    /// </summary>
    public int? OutboundOrderId { get; set; }
}


