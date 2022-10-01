using NHibernateUtils;
using System.ComponentModel.DataAnnotations;

namespace ArcWms;


/// <summary>
/// 表示物料主数据。
/// </summary>
public class Material
{
    public Material()
    {
    }

    public virtual int MaterialId { get; set; }

    [Required]
    [MaxLength(30)]
    public virtual string MaterialCode { get; set; } = default!;


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int v { get; set; }

    [CreationTime]
    public virtual DateTime CreationTime { get; set; }

    [CreationUser]
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? CreationUser { get; set; } = default!;

    [ModificationTime]
    public virtual DateTime ModificationTime { get; set; }

    [ModificationUser]
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? ModificationUser { get; set; }


    /// <summary>
    /// 获取物料类型。
    /// </summary>
    [MaxLength(8)]
    public virtual string? MaterialType { get; set; }

    [Required]
    [MaxLength(255)]
    public virtual string? Description { get; set; }

    [MaxLength(30)]
    public virtual string? SpareCode { get; set; }

    [MaxLength(64)]
    public virtual string? Specification { get; set; }

    [MaxLength(20)]
    public virtual string? MnemonicCode { get; set; }

    /// <summary>
    /// 是否启用批次管理。
    /// </summary>
    public virtual bool BatchEnabled { get; set; }

    [MaxLength(50)]
    public virtual string? MaterialGroup { get; set; }

    public virtual decimal ValidDays { get; set; }

    /// <summary>
    /// 此物料的静置时间（以小时为单位）。
    /// </summary>
    public virtual decimal StandingTime { get; set; }

    [MaxLength(1)]
    public virtual string? AbcClass { get; set; }

    [Required]
    [MaxLength(DATA_LENGTH.UOM)]
    public virtual string? Uom { get; set; }



    public virtual decimal LowerBound { get; set; } = -1;

    public virtual decimal UpperBound { get; set; } = 99999999;


    public virtual decimal DefaultQuantity { get; set; }

    [Required]
    [MaxLength(8)]
    public virtual string? DefaultStorageGroup { get; set; }

    public virtual string? Comment { get; set; }

}
