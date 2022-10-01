namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 侧视图数据。
/// </summary>
public class SideViewData
{
    /// <summary>
    /// 巷道编码
    /// </summary>
    public string StreetletCode { get; set; } = default!;

    /// <summary>
    /// 巷道是否禁入。
    /// </summary>
    public bool IsInboundDisabled { get; set; }


    /// <summary>
    /// 巷道禁入的备注
    /// </summary>
    public string? InboundDisabledComment { get; set; }


    /// <summary>
    /// 巷道是否禁出。
    /// </summary>
    public bool IsOutboundDisabled { get; set; }


    /// <summary>
    /// 巷道禁出的备注
    /// </summary>
    public string? OutboundDisabledComment { get; set; }


    /// <summary>
    /// 巷道的货位数，不包含 <see cref="Location.Exists"/> 为 false 的货位。
    /// </summary>
    public int LocationCount { get; set; }

    /// <summary>
    /// 巷道的可用货位数，即存在、无货、无入站任务、未禁止入站的货位
    /// </summary>
    public int AvailableCount { get; set; }


    /// <summary>
    /// 巷道的货位，包含 <see cref="Location.Exists"/> 为 false 的货位。
    /// </summary>
    public List<SideViewLocation> Locations { get; set; } = default!;
}
