namespace ArcWms;

/// <summary>
/// 表示库存状态。例如：待检、合格。
/// </summary>
/// <param name="Value"></param>
public sealed record InventoryStatus(string Value);
