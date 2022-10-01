namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 从托盘中为出库单拣货的操作参数。
/// </summary>
public class OutboundOrderPickArgs
{
    /// <summary>
    /// 要拣货的托盘号。
    /// </summary>
    public string PalletCode { get; set; }

    /// <summary>
    /// 拣货信息。
    /// </summary>
    public OutboundOrderPickInfo[]? PickInfos { get; set; }
}

