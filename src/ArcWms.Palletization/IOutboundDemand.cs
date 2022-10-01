namespace ArcWms;

/// <summary>
/// 表示一个出库需求。
/// </summary>
public interface IOutboundDemand
{
    /// <summary>
    /// 获取需求数量
    /// </summary>
    decimal QuantityDemanded { get; }

    /// <summary>
    /// 获取或设置已分配数量
    /// </summary>
    decimal GetQuantityAllocated();

    /// <summary>
    /// 获取或设置已完成数量（已产生出库流水的数量）
    /// </summary>
    decimal QuantityFulfilled { get; set; }

}
