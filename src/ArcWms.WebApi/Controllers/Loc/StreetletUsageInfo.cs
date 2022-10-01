namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 巷道使用数据
/// </summary>
public class StreetletUsageInfo
{
    /// <summary>
    /// 货位存储分组
    /// </summary>
    public string? StorageGroup { get; set; }

    /// <summary>
    /// 货位规格
    /// </summary>
    public string? Specification { get; set; }

    /// <summary>
    /// 货位限重
    /// </summary>
    public float WeightLimit { get; set; }

    /// <summary>
    /// 货位限高
    /// </summary>
    public float HeightLimit { get; set; }

    /// <summary>
    /// 总货位数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前可用的货位数
    /// </summary>
    public int Available { get; set; }

    /// <summary>
    /// 当前有货的货位数
    /// </summary>
    public int Loaded { get; set; }

    /// <summary>
    /// 当前已禁止入站的货位数
    /// </summary>
    public int InboundDisabled { get; set; }

}

