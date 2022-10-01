namespace ArcWms;

/// <summary>
/// 出库单库存分配程序。
/// </summary>
public interface IOutboundOrderAllocator
{
    /// <summary>
    /// 为出库单分配库存。
    /// </summary>
    /// <param name="outboundOrder">要分配库存的出库单</param>
    /// <param name="options">分配选项</param>
    Task AllocateAsync(OutboundOrder outboundOrder, AllocateStockOptions options);


    /// <summary>
    /// 解除出库单在货架上的分配，货架外的分配使用 <see cref="DeallocateAsync(OutboundOrder, Unitload)"/> 方法单独处理。
    /// </summary>
    /// <param name="outboundOrder"></param>
    Task DeallocateInRackAsync(OutboundOrder outboundOrder);


    /// <summary>
    /// 从出库单取消特定货载的分配。
    /// </summary>
    /// <param name="outboundOrder"></param>
    /// <param name="unitload"></param>
    Task DeallocateAsync(OutboundOrder outboundOrder, Unitload unitload);
}
