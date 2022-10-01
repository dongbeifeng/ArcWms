using NHibernateUtils;
using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示单元货物，又称单元货载，简称货载。货载分为三种类型：
/// * 空货载：Items 集合元素个数为 0；
/// * 标准货载：Items 集合元素个数为 1；
/// * 混合货载：Items 集合元素个数大于 1；
/// </summary>
public class Unitload
{
    private ISet<UnitloadItem> _items;

    public Unitload()
    {
        this.CreationTime = DateTime.Now;
        this.ModificationTime = DateTime.Now;
        this.StorageInfo = new StorageInfo();
        _items = new HashSet<UnitloadItem>();
    }

    public virtual int UnitloadId { get; set; }

    [Required]
    [MaxLength(20)]
    public virtual string? PalletCode { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int v { get; set; }

    [CreationTime]
    public virtual DateTime CreationTime { get; set; }

    [ModificationTime]
    public virtual DateTime ModificationTime { get; set; }

    [CreationUser]
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? CreationUser { get; set; }

    public virtual StorageInfo StorageInfo { get; set; }

    public virtual bool HasCountingError { get; set; }

    public virtual IEnumerable<UnitloadItem> Items
    {
        get
        {
            return _items;
        }
        protected set
        {
            _items = (ISet<UnitloadItem>)value;
        }
    }

    public virtual bool HasTask { get; set; }


    public virtual Location? CurrentLocation { get; internal protected set; }


    public virtual DateTime CurrentLocationTime { get; internal protected set; }


    public virtual IUnitloadAllocationTable? CurrentUat { get; protected set; }


    /// <summary>
    /// 设置 <see cref="CurrentUat"/> 和 <see cref="CurrentUatRootType"/> 属性。
    /// </summary>
    /// <param name="uat"></param>
    public virtual void SetCurrentUat(IUnitloadAllocationTable uat)
    {
        if (uat == null)
        {
            throw new ArgumentNullException(nameof(uat));
        }

        if (this.CurrentUat != null && this.CurrentUat != uat)
        {
            throw new InvalidOperationException("已分配给其他单据");
        }

        this.CurrentUat = uat;
    }

    /// <summary>
    /// 清除 <see cref="CurrentUat"/> 和 <see cref="CurrentUatRootType"/> 属性。
    /// </summary>
    public virtual void ResetCurrentUat()
    {
        this.CurrentUat = null;
    }

    // TODO 重命名
    [MaxLength(20)]
    public virtual string? OpHintType { get; protected set; }

    [MaxLength(20)]
    public virtual string? OpHintInfo { get; protected set; }

    public virtual string? Comment { get; set; }

    public virtual void SetOpHint(string opHintType, string opHintInfo)
    {
        if (!string.IsNullOrWhiteSpace(this.OpHintType))
        {
            throw new InvalidOperationException("货载上有未清除的操作提示。");
        }

        this.OpHintType = opHintType;
        this.OpHintInfo = opHintInfo;
    }


    public virtual void ResetOpHint()
    {
        this.OpHintType = null;
        this.OpHintInfo = null;
    }

    public virtual void AddItem(UnitloadItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (item.Unitload != null)
        {
            throw new InvalidOperationException("向货载添加货载项失败，货载项已属于其他货载。");
        }
        _items.Add(item);
        item.Unitload = this;

    }


    public virtual void RemoveItem(UnitloadItem item)
    {
        if (this.Items.Contains(item) == false)
        {
            throw new InvalidOperationException("项不在这个货载里。");
        }

        item.Unitload = null!;
        _items.Remove(item);
    }

    /// <summary>
    /// 获取一个值，指示此货载是否在货架上。有下架任务的货载，在任务完成前，也算作在货架上。
    /// </summary>
    /// <returns></returns>
    public virtual bool IsInRack()
    {
        return this.CurrentLocation?.LocationType == LocationTypes.S;
    }

    public override string? ToString()
    {
        return this.PalletCode;
    }
}


