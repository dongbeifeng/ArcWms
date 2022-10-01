namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 关键点信息。
/// </summary>
public class KeyPointInfo
{
    /// <summary>
    /// 货位 Id
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// 货位编码
    /// </summary>
    public string LocationCode { get; set; } = default!;


    /// <summary>
    /// 入站数
    /// </summary>
    public int InboundCount { get; set; }

    /// <summary>
    /// 禁止入站
    /// </summary>
    public bool IsInboundDisabled { get; set; }

    /// <summary>
    /// 禁止入站备注
    /// </summary>
    public string? InboundDisabledComment { get; set; }

    /// <summary>
    /// 入站数限制
    /// </summary>
    public int InboundLimit { get; set; }


    /// <summary>
    /// 出站数
    /// </summary>
    public int OutboundCount { get; set; }

    /// <summary>
    /// 禁止出站
    /// </summary>
    public bool IsOutboundDisabled { get; set; }

    /// <summary>
    /// 禁止出站备注
    /// </summary>
    public string? OutboundDisabledComment { get; set; }

    /// <summary>
    /// 出站数限制
    /// </summary>
    public int OutboundLimit { get; set; }


    /// <summary>
    /// 标记
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 请求类型
    /// </summary>
    public string? RequestType { get; set; }

    /// <summary>
    /// 货载数
    /// </summary>
    public int UnitloadCount { get; set; }

}

