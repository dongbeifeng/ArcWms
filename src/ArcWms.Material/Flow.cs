using NHibernateUtils;
using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示库存流水。
/// </summary>
public class Flow : IHasInventoryKey
{
    /// <summary>
    /// 初始化 Flow 类的新实例。
    /// </summary>
    public Flow()
    {
        this.CreationTime = DateTime.Now;
    }

    public virtual int FlowId { get; internal protected set; }

    [CreationTime]
    public virtual DateTime CreationTime { get; set; }

    [MaxLength(DATA_LENGTH.USERNAME)]
    [CreationUser]
    public virtual string? CreationUser { get; set; }

    [Required]
    public virtual Material? Material { get; set; }

    [Required]
    [MaxLength(DATA_LENGTH.BATCH)]
    public virtual string? Batch { get; set; }

    [MaxLength(DATA_LENGTH.INVENTORY_STATUS)]
    [Required]
    public virtual string? InventoryStatus { get; set; }

    public virtual decimal Quantity { get; set; }

    [Required]
    [MaxLength(DATA_LENGTH.UOM)]
    public virtual string? Uom { get; set; }


    [Required]
    [MaxLength(DATA_LENGTH.BIZ_TYPE)]
    public virtual string? BizType { get; set; }

    public virtual FlowDirection Direction { get; set; }

    [Required]
    [MaxLength(DATA_LENGTH.OPERATION_TYPE)]
    public virtual string? OperationType { get; set; }


    [MaxLength(20)]
    public virtual string? OrderCode { get; set; }

    [MaxLength(20)]
    public virtual string? BizOrder { get; set; }

    [MaxLength(20)]
    public virtual string? PalletCode { get; set; }

    public virtual decimal Balance { get; set; }

    public virtual string? Comment { get; set; }

}
