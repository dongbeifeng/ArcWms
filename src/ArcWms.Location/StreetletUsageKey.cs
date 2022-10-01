using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 巷道使用信息的 Key。
/// </summary>
public record StreetletUsageKey
{
    /// <summary>
    /// 获取统计信息的货位存储分组。
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string? StorageGroup { get; init; }

    /// <summary>
    /// 获取统计信息的货位规格。
    /// </summary>
    [Required]
    [MaxLength(16)]
    public string? Specification { get; init; }

    /// <summary>
    /// 获取统计信息的货位限重。
    /// </summary>
    public float WeightLimit { get; init; }

    /// <summary>
    /// 获取统计信息的货位限高。
    /// </summary>
    public float HeightLimit { get; init; }

}
