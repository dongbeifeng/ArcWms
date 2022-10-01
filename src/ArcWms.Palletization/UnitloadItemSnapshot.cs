using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示单元货物明细的快照。
/// </summary>
public class UnitloadItemSnapshot
{
    public UnitloadItemSnapshot()
    {
    }

    public virtual int UnitloadItemSnapshotId { get; internal protected set; }


    [Required]
    public virtual UnitloadSnapshot? Unitload { get; internal protected set; }

    [Required]
    public virtual Material Material { get; set; } = default!;

    [Required]
    [MaxLength(DATA_LENGTH.BATCH)]
    public virtual string? Batch { get; set; }

    [Required]
    [MaxLength(DATA_LENGTH.INVENTORY_STATUS)]
    public virtual string? InventoryStatus { get; set; }


    public virtual decimal Quantity { get; set; }

    [MaxLength(DATA_LENGTH.UOM)]
    [Required]
    public virtual string? Uom { get; set; }


    public virtual DateTime ProductionTime { get; set; }



}
