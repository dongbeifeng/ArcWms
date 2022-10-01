namespace ArcWms;

/// <summary>
/// 巷道的使用数据。
/// </summary>
[Serializable]
public record StreetletUsageData
{
    /// <summary>
    /// 获取或设置总货位数。
    /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// 获取或设置当前可用的货位数。
    /// 可用货位是指 <see cref="Location.IsAvailable"/> 方法返回 true 的货位。
    /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
    /// </summary>
    public int Available { get; init; }

    /// <summary>
    /// 获取或设置当前有货的货位数。
    /// 有货的货位是指 <see cref="Location.IsLoaded"/> 方法返回 true 的货位。
    /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
    /// </summary>
    public int Loaded { get; init; }

    /// <summary>
    /// 获取或设置当前已禁止入站的货位数，
    /// <see cref="Location.Exists"/> 为 false 的货位不参与统计。
    /// </summary>
    public int InboundDisabled { get; init; }



}
