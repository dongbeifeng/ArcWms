using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示巷道。
/// </summary>
public class Streetlet
{
    public Streetlet()
    {
        this.Locations = new HashSet<Location>();
        this.Outlets = new HashSet<Outlet>();
        this.Usage = new Dictionary<StreetletUsageKey, StreetletUsageData>();
    }

    public virtual int StreetletId { get; internal protected set; }

    [Required]
    [MaxLength(4)]
    public virtual string StreetletCode { get; set; } = default!;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int v { get; set; }


    /// <summary>
    /// 获取或设置此巷道所在的区域。
    /// </summary>
    [Required]
    [MaxLength(16)]
    public virtual string Area { get; set; } = default!;

    public virtual string? Comment { get; set; }

    /// <summary>
    /// 获取属于此巷道的货位。
    /// </summary>
    public virtual ISet<Location> Locations { get; protected set; }


    /// <summary>
    /// 获取或设置此巷道是否禁止入站。
    /// </summary>
    /// <remarks>
    /// 若为 true，则不允许生成新的以此巷道为终点的任务。
    /// 已存在的任务不受影响，可以正常完成。
    /// </remarks>
    public virtual bool IsInboundDisabled { get; set; }

    /// <summary>
    /// 获取或设置此巷道禁止入站的备注。
    /// </summary>
    public virtual string? InboundDisabledComment { get; set; }


    /// <summary>
    /// 获取或设置此巷道是否禁止出站。
    /// </summary>
    /// <remarks>
    /// 若为 true，则不允许生成新的以此巷道为起点的任务。
    /// 已存在的任务不受影响，可以正常完成。
    /// </remarks>
    public virtual bool IsOutboundDisabled { get; set; }

    /// <summary>
    /// 获取或设置此位置禁止出站的备注。
    /// </summary>
    public virtual string? OutboundDisabledComment { get; set; }

    /// <summary>
    /// 指示此巷道是否为双深。
    /// </summary>
    public virtual bool IsDoubleDeep { get; set; }

    /// <summary>
    /// 获取或设置此巷道的保留货位数。
    /// </summary>
    public virtual int ReservedLocationCount { get; set; }

    /// <summary>
    /// 获取此巷道能够到达的出货口。
    /// </summary>
    public virtual ISet<Outlet> Outlets { get; protected set; }


    public override string? ToString()
    {
        return StreetletCode;
    }

    public virtual IDictionary<StreetletUsageKey, StreetletUsageData> Usage { get; protected set; }

    public virtual int GetTotalLocationCount()
    {
        if (this.Usage.Any())
        {
            return this.Usage.Sum(x => x.Value.Total);
        }

        return 0;
    }

    public virtual int GetAvailableLocationCount()
    {
        if (this.Usage.Any())
        {
            return this.Usage.Sum(x => x.Value.Available);
        }

        return 0;
    }
}
