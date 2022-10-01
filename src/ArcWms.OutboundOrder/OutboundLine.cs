using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示出库单明细。
/// </summary>
public class OutboundLine : IOutboundDemand
{
    /// <summary>
    /// 出库单明细作为出库需求时的根类型常量
    /// </summary>
    public const string OutboundDemandRootType = nameof(OutboundLine);


    public OutboundLine()
    {
    }

    /// <summary>
    /// 出库单明细Id。
    /// </summary>
    public virtual int OutboundLineId { get; set; }

    /// <summary>
    /// 所属出库单。
    /// </summary>
    [Required]
    public virtual OutboundOrder? OutboundOrder { get; internal protected set; }

    /// <summary>
    /// 要出库的物料。
    /// </summary>
    [Required]
    public virtual Material? Material { get; set; }


    /// <summary>
    /// 要出库的批号，可以为空
    /// </summary>
    [MaxLength(DATA_LENGTH.BATCH)]
    public virtual string? Batch { get; set; }


    /// <summary>
    /// 要出库的库存状态。
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.INVENTORY_STATUS)]
    public virtual string? InventoryStatus { get; set; }

    /// <summary>
    /// 计量单位。
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.UOM)]
    public virtual string? Uom { get; set; }

    /// <summary>
    /// 获取或设置需求数量。
    /// </summary>
    public virtual decimal QuantityDemanded { get; set; }

    /// <summary>
    /// 获取或设置已分配数量
    /// </summary>
    public virtual decimal GetQuantityAllocated()
    {
        return this.Allocations.Sum(x => x.QuantityAllocated);
    }

    /// <summary>
    /// 获取或设置已完成数量（产生出库流水）
    /// </summary>
    public virtual decimal QuantityFulfilled { get; set; }

    /// <summary>
    /// 未出数量，MAX(应出-已出, 0)
    /// </summary>
    public virtual decimal GetQuantityUnfulfilled()
    {
        return Math.Max(QuantityDemanded - QuantityFulfilled, 0);
    }

    /// <summary>
    /// 计算此出库单明细的分配欠数
    /// </summary>
    /// <returns></returns>
    public virtual decimal ComputeShortage()
    {
        return QuantityDemanded - QuantityFulfilled - GetQuantityAllocated();
    }


    // TODO 在拣货时维护此属性
    /// <summary>
    /// 指示此明细行是否发生过出库操作。发生过出库操作的出库明细不能被删除。
    /// 在第一次拣货时，此属性变为 true，此后不会变回 false。
    /// </summary>
    public virtual bool Dirty { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Comment { get; set; }

    // TODO 重命名
    /// <summary>
    /// 获取此出库单明细当前的分配信息。此属性由分配程序使用，不要直接添加和移除元素。
    /// </summary>
    public virtual ISet<UnitloadItemAllocation> Allocations { get; protected set; } = new HashSet<UnitloadItemAllocation>();

    /// <summary>
    /// 从货载明细中分配指定数量的库存。
    /// 此方法维护仅 <see cref="OutboundLine.Allocations"/> 和 <see cref="UnitloadItem.Allocations"/> 集合，
    /// 不会维护 <see cref="Unitload.CurrentUat"/> 属性。
    /// </summary>
    /// <param name="unitloadItem"></param>
    /// <param name="quantityAllocated"></param>
    /// <returns></returns>
    public virtual UnitloadItemAllocation Allocate(UnitloadItem unitloadItem, decimal quantityAllocated)
    {
        UnitloadItemAllocation allocation = new UnitloadItemAllocation
        {
            UnitloadItem = unitloadItem,
            QuantityAllocated = quantityAllocated,
            OutboundDemand = this,
            OutboundDemandRootType = OutboundDemandRootType,
        };

        this.Allocations.Add(allocation);
        unitloadItem.Allocations.Add(allocation);

        return allocation;
    }

    public virtual void Deallocate(UnitloadItemAllocation allocation)
    {
        if (this.Allocations.Contains(allocation) == false)
        {
            throw new InvalidOperationException();
        }

        this.Allocations.Remove(allocation);
        allocation.UnitloadItem?.Allocations.Remove(allocation);
    }

    public override string? ToString()
    {
        return $"{this.OutboundOrder?.OutboundOrderCode}#{this.OutboundLineId}";
    }
}
