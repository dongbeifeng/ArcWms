using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示单元货物的快照。
/// </summary>
public class UnitloadSnapshot
{
    public UnitloadSnapshot()
    {
        Items = new HashSet<UnitloadItemSnapshot>();
    }


    public virtual int UnitloadSnapshotId { get; internal protected set; }

    /// <summary>
    /// 快照的创建时间。
    /// </summary>
    public virtual DateTime SnapshotTime { get; set; }


    [Required]
    [MaxLength(20)]
    public virtual string? PalletCode { get; set; }

    /// <summary>
    /// 源货载的创建时间，不是快照的创建时间。
    /// </summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// 源货载的修改时间。
    /// </summary>
    public virtual DateTime ModificationTime { get; set; }

    /// <summary>
    /// 源货载的创建人。
    /// </summary>
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? CreationUser { get; set; }


    public virtual StorageInfo StorageInfo { get; set; }

    public virtual bool HasCountingError { get; set; }

    public virtual string? Comment { get; set; }

    public virtual ISet<UnitloadItemSnapshot> Items { get; protected set; }

    #region 维护 Items 集合

    public virtual void AddItem(UnitloadItemSnapshot item)
    {
        item.Unitload = this;
        this.Items.Add(item);
    }

    #endregion
}
