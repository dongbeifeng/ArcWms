namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 将出库单附加到出口的操作参数。
/// </summary>
public class OutboundOrderAttachToPortsArgs
{
    /// <summary>
    /// 出库单 Id。
    /// </summary>
    public int? OutboundOrderId { get; set; }

    /// <summary>
    /// 出口编码。
    /// </summary>
    public string[]? Ports { get; set; }
}


