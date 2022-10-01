namespace ArcWms;

/// <summary>
/// 提供货位类型常数。
/// </summary>
public static class LocationTypes
{
    /// <summary>
    /// S 位置，表示可以储存货物的位置。
    /// </summary>
    public const string S = "S";

    /// <summary>
    /// K 位置，表示关键点，是托盘运输中的起点、终点或途经点，与 S 位置的区别是托盘不能在 K 位置上长期停留。K 位置通常是自动化的。
    /// </summary>
    public const string K = "K";

    /// <summary>
    /// N 位置，表示一个不存在的特殊位置。N 位置在整个系统中只有一个实例。
    /// 货载刚刚注册时，在 N 位置上。N 位置的编码始终为 <see cref="NLocationCode.Value"/>。
    /// </summary>
    public const string N = "N";
}

/// <summary>
/// 提供 N 位置的货位编码常数。
/// </summary>
public static class NLocationCode
{
    public const string Value = "N";
}