using NHibernateUtils;
using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示一个位置，位置的本质是货物停留点和移动的起止点。
/// </summary>
public class Location
{
    public Location()
    {
    }

    /// <summary>
    /// 主键
    /// </summary>
    public virtual int LocationId { get; internal protected set; }

    /// <summary>
    /// 获取或设置此位置的编码。这是自然键。
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.LOCATION_CODE)]
    public virtual string LocationCode { get; set; } = default!;

    /// <summary>
    /// 版本号
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int v { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [CreationTime]
    public virtual DateTime CreationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间。
    /// </summary>
    [ModificationTime]
    public virtual DateTime ModificationTime { get; set; } = DateTime.Now;


    /// <summary>
    /// 获取此位置的类型。
    /// </summary>
    [MaxLength(4)]
    [Required]
    public virtual string? LocationType { get; set; }


    /// <summary>
    /// 获取或设置此货位的入站数，已生成未下发的任务也计算在内。
    /// </summary>
    public virtual int InboundCount { get; set; }


    /// <summary>
    /// 获取或设置此货位的入站数限制。当入站数达到此值时，不允许生成新的入站。
    /// 此属性用来限制任务的生成，而不用来限制任务的下发。
    /// </summary>
    [Range(1, 999)]
    public virtual int InboundLimit { get; set; }

    /// <summary>
    /// 获取或设置此位置是否禁止入站。
    /// </summary>
    /// <remarks>
    /// 若为 true，则不允许生成新的以此位置为终点的任务。
    /// 已存在的任务不受影响，可以正常完成。
    /// </remarks>
    public virtual bool IsInboundDisabled { get; set; }

    /// <summary>
    /// 获取或设置此位置禁止入站的备注。
    /// </summary>
    public virtual string? InboundDisabledComment { get; set; }

    /// <summary>
    /// 获取或设置此货位的出站数，已生成未下发的任务也计算在内。
    /// </summary>
    public virtual int OutboundCount { get; set; }


    /// <summary>
    /// 获取或设置此货位的最大出站数。当出站数达到此值时，不允许生成新的出站任务。
    /// 此属性用来限制任务的生成，而不用来限制任务的下发。
    /// </summary>
    [Range(1, 999)]
    public virtual int OutboundLimit { get; set; }

    /// <summary>
    /// 获取或设置此位置是否禁止出站。
    /// </summary>
    /// <remarks>
    /// 若为 true，则不允许生成新的以此位置为起点的任务。
    /// 已存在的任务不受影响，可以正常完成。
    /// </remarks>
    public virtual bool IsOutboundDisabled { get; set; }

    /// <summary>
    /// 获取或设置此位置禁止出站的备注。
    /// </summary>
    public virtual string? OutboundDisabledComment { get; set; }

    /// <summary>
    /// 获取或设置此位置此货位是否存在。
    /// </summary>
    public virtual bool Exists { get; set; } = true;


    /// <summary>
    /// 此货位的限重，单位千克。
    /// </summary>
    public virtual float WeightLimit { get; set; }

    /// <summary>
    /// 此货位的限高，单位米。
    /// </summary>
    public virtual float HeightLimit { get; set; }

    /// <summary>
    /// 指示此位置的规格（不含高度），例如【九角1200x1100】
    /// </summary>
    [MaxLength(16)]
    public virtual string? Specification { get; set; }


    /// <summary>
    /// 指示货位属于哪个巷道。
    /// </summary>
    public virtual Streetlet? Streetlet { get; set; }

    /// <summary>
    /// 指示货位在巷道哪一侧。
    /// </summary>
    public virtual RackSide Side { get; set; }

    /// <summary>
    /// 指示货位属于第几深位。
    /// </summary>
    public virtual int Deep { get; set; }

    /// <summary>
    /// 此位置的列。
    /// </summary>
    public virtual int Bay { get; set; }

    /// <summary>
    /// 此位置的层。
    /// </summary>
    public virtual int Level { get; set; }

    /// <summary>
    /// 获取或设置此货位的存储分组。
    /// </summary>
    [MaxLength(10)]
    public virtual string? StorageGroup { get; set; }

    /// <summary>
    /// 指示此货位上的货载数，此属性不适用于 <see cref="LocationTypes.N"/> 类型的货位。
    /// 货载正在出站但任务未完成时，此计数保持不变，直到出站任务完成时才更新计数。
    /// </summary>
    public virtual int UnitloadCount { get; internal protected set; }


    /// <summary>
    /// 获取或设置此货位所在的单元格。
    /// </summary>
    public virtual Cell? Cell { get; set; }


    /// <summary>
    /// 获取或设置此位置的标记。
    /// </summary>
    [MaxLength(30)]
    public virtual string? Tag { get; set; }


    /// <summary>
    /// 获取或设置此位置上的请求类型，仅适用于关键点。
    /// </summary>
    [MaxLength(16)]
    public virtual string? RequestType { get; set; }

    /// <summary>
    /// 获取一个值指示此货位是否有货。
    /// </summary>
    /// <returns></returns>
    public virtual bool IsLoaded()
    {
        return this.UnitloadCount > 0;
    }

    /// <summary>
    /// 获取一个值指示此货位是否可用。货位可用是指货位无货且未禁止入站。
    /// 货位的入站数和出站数不影响此方法的返回值。
    /// </summary>
    /// <returns></returns>
    public virtual bool IsAvailable()
    {
        return IsLoaded() == false
            && IsInboundDisabled == false;
    }


    public virtual void IncreaseUnitloadCount()
    {
        if (LocationType == LocationTypes.N)
        {
            throw new InvalidOperationException("不能为 N 位置调用此方法。");
        }

        var prevLoaded = IsLoaded();
        var prevAvail = IsAvailable();
        UnitloadCount++;

        if (LocationType == LocationTypes.S)
        {
            // 更新巷道使用数据
            var key = new StreetletUsageKey
            {
                StorageGroup = StorageGroup!,
                Specification = Specification,
                WeightLimit = WeightLimit,
                HeightLimit = HeightLimit,
            };
            var usage = Streetlet?.Usage;
            if (usage != null && usage.ContainsKey(key))
            {
                var loaded = IsLoaded();
                var avail = IsAvailable();

                if (prevLoaded != loaded)
                {
                    usage[key] = usage[key] with { Loaded = usage[key].Loaded + 1 };
                }
                if (prevAvail != avail)
                {
                    usage[key] = usage[key] with { Available = usage[key].Available - 1 };
                }
            }
        }
    }



    public virtual void DecreaseUnitloadCount()
    {
        if (LocationType == LocationTypes.N)
        {
            throw new InvalidOperationException("不能为 N 位置调用此方法。");
        }

        if (UnitloadCount == 0)
        {
            throw new InvalidOperationException($"{nameof(UnitloadCount)} 不能小于 0。");
        }

        var prevLoaded = IsLoaded();
        var prevAvail = IsAvailable();
        UnitloadCount--;

        if (LocationType == LocationTypes.S)
        {
            var key = new StreetletUsageKey
            {
                StorageGroup = StorageGroup!,
                Specification = Specification,
                WeightLimit = WeightLimit,
                HeightLimit = HeightLimit,
            };

            var usage = Streetlet?.Usage;
            if (usage != null && usage.ContainsKey(key))
            {
                var loaded = IsLoaded();
                var avail = IsAvailable();

                if (prevLoaded != loaded)
                {
                    usage[key] = usage[key] with { Loaded = usage[key].Loaded - 1 };
                }
                if (prevAvail != avail)
                {
                    usage[key] = usage[key] with { Available = usage[key].Available + 1 };
                }
            }
        }
    }



    public override string? ToString()
    {
        return this.LocationCode;
    }
}
