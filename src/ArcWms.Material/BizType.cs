namespace ArcWms;

/// <summary>
/// 表示业务类型。例如：采购入库、生产领料。
/// </summary>
public abstract record BizType(string Value);


/// <summary>
/// 表示入库类的业务类型。例如：采购入库、销售退货。
/// </summary>
/// <param name="Value"></param>
public sealed record InboundBizType(string Value) : BizType(Value);


/// <summary>
/// 表示出库类的业务类型。例如：销售出库、采购退货。
/// </summary>
/// <param name="Value"></param>
public sealed record OutboundBizType(string Value) : BizType(Value);

/// <summary>
/// 表示状态转换类的业务类型。例如：待检转合格。
/// </summary>
/// <param name="Value"></param>
/// <param name="IssuingStatus">转出的库存状态。</param>
/// <param name="ReceivingStatus">转入的库存状态。</param>
public sealed record StatusChangingBizType(string Value, InventoryStatus IssuingStatus, InventoryStatus ReceivingStatus) : BizType(Value);
