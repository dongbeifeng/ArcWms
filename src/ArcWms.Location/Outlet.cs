using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示出口。
/// </summary>
public class Outlet
{
    public Outlet(string outletCode)
    {
        this.Streetlets = new HashSet<Streetlet>();
        this.OutletCode = outletCode;
    }

    protected Outlet()
    {
        this.Streetlets = new HashSet<Streetlet>();
        this.OutletCode = default!;
    }

    public virtual int OutletId { get; internal protected set; }

    [Required]
    [MaxLength(20)]
    public virtual string OutletCode { get; protected set; }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int v { get; set; }

    [Required]
    public virtual Location? KP1 { get; set; }

    public virtual Location? KP2 { get; set; }

    public virtual string? Comment { get; set; }

    /// <summary>
    /// 可到达此出口的巷道。
    /// </summary>
    public virtual ISet<Streetlet> Streetlets { get; protected set; }


    // TODO 考虑使用单独的类代替这个这个属性
    // 原因：由于程序集间的依赖关系次序，不能使用 IUnitloadAllocationTable 作为属性的类型
    /// <summary>
    /// 当前在此出口上的单据。
    /// </summary>
    public virtual object? CurrentUat { get; protected set; }

    /// <summary>
    /// 上次检查时间。
    /// </summary>
    public virtual DateTime LastCheckTime { get; set; } = default;

    /// <summary>
    /// 上次检查消息。
    /// </summary>
    public virtual string? LastCheckMessage { get; set; }


    // TODO 考虑引入接口，并重命名，
    public virtual void SetCurrentUat(object uat)
    {
        if (uat == null)
        {
            throw new ArgumentNullException(nameof(uat));
        }

        if (this.CurrentUat == uat)
        {
            return;
        }

        if (this.CurrentUat != null)
        {
            throw new InvalidOperationException($"出口已被占用。【{this.CurrentUat}】");
        }

        this.CurrentUat = uat;
    }

    // TODO 重命名
    public virtual void ResetCurrentUat()
    {
        this.CurrentUat = null;
    }

    public override string? ToString()
    {
        return this.OutletCode;
    }
}
