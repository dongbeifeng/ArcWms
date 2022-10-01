namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 储位信息。
/// </summary>
public class StorageLocationInfo
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
    /// 巷道 Id
    /// </summary>
    public int StreetletId { get; set; }

    /// <summary>
    /// 巷道编码
    /// </summary>
    public string StreetletCode { get; set; } = default!;

    /// <summary>
    /// 限重
    /// </summary>
    public float WeightLimit { get; set; }

    /// <summary>
    /// 限高
    /// </summary>
    public float HeightLimit { get; set; }

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
    /// 存储分组
    /// </summary>
    public string? StorageGroup { get; set; }

    /// <summary>
    /// 货载数
    /// </summary>
    public int UnitloadCount { get; set; }

}

