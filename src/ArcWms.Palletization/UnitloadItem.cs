using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示单元货物明细。
/// </summary>
public class UnitloadItem : IHasInventoryKey
{
    public UnitloadItem()
    {
    }

    public virtual int UnitloadItemId { get; set; }

    [Required]
    public virtual Unitload? Unitload { get; internal protected set; }


    [Required]
    public virtual Material? Material { get; set; }

    /// <summary>
    /// 批号
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.BATCH)]
    public virtual string? Batch { get; set; }

    [Required]
    [MaxLength(DATA_LENGTH.INVENTORY_STATUS)]
    public virtual string? InventoryStatus { get; set; }

    public virtual decimal Quantity { get; set; }

    [Required]
    [MaxLength(DATA_LENGTH.UOM)]
    public virtual string? Uom { get; set; }

    /// <summary>
    /// 生产时间
    /// </summary>
    public virtual DateTime ProductionTime { get; set; }

    /// <summary>
    /// 库龄基线
    /// </summary>
    public virtual DateTime AgeBaseline { get; set; }

    [MaxLength(20)]
    [Required]
    public virtual string? Fifo { get; set; }

    /// <summary>
    /// 获取此货载项的分配信息
    /// </summary>
    public virtual ISet<UnitloadItemAllocation> Allocations { get; protected set; } = new HashSet<UnitloadItemAllocation>();

    public override string? ToString()
    {
        return $"{this.Unitload?.PalletCode}#{this.UnitloadItemId}";
    }
}


