using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示入库单中的明细行。
/// </summary>
public class InboundLine : IHasInventoryKey
{
    /// <summary>
    /// 初始化此类的新实例。
    /// </summary>
    public InboundLine()
    {
    }

    /// <summary>
    /// 主键
    /// </summary>
    public virtual int InboundLineId { get; protected set; }


    /// <summary>
    /// 获取或设置此行所属的入库单。
    /// </summary>
    [Required]
    public virtual InboundOrder? InboundOrder { get; internal protected set; }

    /// <summary>
    /// 收货物料。
    /// </summary>
    [Required]
    public virtual Material? Material { get; set; }

    /// <summary>
    /// 收货批号。
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.BATCH)]
    public virtual string? Batch { get; set; }

    [Required]
    [MaxLength(DATA_LENGTH.INVENTORY_STATUS)]
    public virtual string? InventoryStatus { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.UOM)]
    public virtual string? Uom { get; set; } = default!;

    /// <summary>
    /// 应收数量。
    /// </summary>
    public virtual decimal QuantityExpected { get; set; }

    /// <summary>
    /// 实收数量。
    /// </summary>
    public virtual decimal QuantityReceived { get; set; }


    // TODO 在收货时维护此属性
    /// <summary>
    /// 指示此明细行是否发生过入库操作。发生过入库操作的入库明细不能被删除。
    /// 在第一次拣货时，此属性变为 true，此后不会变回 false。
    /// </summary>
    public virtual bool Dirty { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Comment { get; set; }

}
