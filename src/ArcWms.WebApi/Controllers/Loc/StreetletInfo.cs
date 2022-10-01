namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 巷道信息
/// </summary>
public class StreetletInfo
{
    /// <summary>
    /// 巷道Id
    /// </summary>
    public int StreetletId { get; init; }

    /// <summary>
    /// 巷道编码
    /// </summary>
    public string? StreetletCode { get; init; }

    /// <summary>
    /// 是否双深
    /// </summary>
    public bool IsDoubleDeep { get; init; }

    /// <summary>
    /// 是否禁入
    /// </summary>
    public bool IsInboundDisabled { get; init; }

    /// <summary>
    /// 禁入备注
    /// </summary>
    public string? InboundDisabledComment { get; init; }


    /// <summary>
    /// 是否禁出
    /// </summary>
    public bool IsOutboundDisabled { get; init; }

    /// <summary>
    /// 禁出备注
    /// </summary>
    public string? OutboundDisabledComment { get; init; }

    /// <summary>
    /// 货位总数
    /// </summary>
    public int TotalLocationCount { get; init; }

    /// <summary>
    /// 可用货位数
    /// </summary>
    public int AvailableLocationCount { get; init; }

    /// <summary>
    /// 保留货位数
    /// </summary>
    public int ReservedLocationCount { get; init; }

    /// <summary>
    /// 货位使用率
    /// </summary>
    public float UsageRate { get; init; }

    /// <summary>
    /// 货位使用数据
    /// </summary>
    public StreetletUsageInfo[]? UsageInfos { get; init; }

    /// <summary>
    /// 可到达的出货口
    /// </summary>
    public OutletInfo[] Outlets { get; init; } = default!;

}

