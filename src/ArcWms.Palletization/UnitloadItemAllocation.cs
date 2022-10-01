using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示为出库需求在货载明细的一次分配信息
/// </summary>
public class UnitloadItemAllocation
{
    /// <summary>
    /// 分配信息的Id
    /// </summary>
    public virtual int UnitloadItemAllocationId { get; set; }

    /// <summary>
    /// 此分配属于哪个货载明细
    /// </summary>
    [Required]
    public virtual UnitloadItem? UnitloadItem { get; set; }

    /// <summary>
    /// 此分配属于哪个出库需求
    /// </summary>
    [Required]
    public virtual IOutboundDemand? OutboundDemand { get; set; }

    // TODO 重构
    /// <summary>
    /// 获取或设置出库需求的根类型
    /// </summary>
    [Required]
    [MaxLength(50)]
    public virtual string? OutboundDemandRootType { get; set; }

    /// <summary>
    /// 获取或设置本次分配的数量。
    /// </summary>
    public virtual decimal QuantityAllocated { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Comment { get; set; }

    public override string? ToString()
    {
        return $"UnitloadItemAllocation#{UnitloadItemAllocationId}, {UnitloadItem}({QuantityAllocated:0.####})--->{OutboundDemand}";
    }
}
