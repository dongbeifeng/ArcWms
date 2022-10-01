namespace ArcWms;

/// <summary>
/// 出库单拣选信息
/// </summary>
public class OutboundOrderPickInfo
{

    /// <summary>
    /// 分配信息Id
    /// </summary>
    public int UnitloadItemAllocationId { get; set; }

    /// <summary>
    /// 拣选数量
    /// </summary>
    public decimal QuantityPicked { get; set; }

}
