using NHibernateUtils;
using System.ComponentModel.DataAnnotations;

namespace ArcWms;


/// <summary>
/// 表示入库单。入库单既是指引入库操作的指令，也是入库操作之后的凭据。
/// </summary>
public class InboundOrder
{
    private ISet<InboundLine> _lines;

    /// <summary>
    /// 初始化此类的新实例。
    /// </summary>
    public InboundOrder()
    {
        this.CreationTime = DateTime.Now;
        this.ModificationTime = DateTime.Now;
        this._lines = new HashSet<InboundLine>();
    }


    /// <summary>
    /// Id
    /// </summary>
    public virtual int InboundOrderId { get; protected set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int v { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    [CreationTime]
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// 创建人。
    /// </summary>
    [CreationUser]
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? CreationUser { get; set; } = default!;

    /// <summary>
    /// 更新时间。
    /// </summary>
    [ModificationTime]
    public virtual DateTime ModificationTime { get; set; }

    /// <summary>
    /// 更改人
    /// </summary>
    [ModificationUser]
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? ModificationUser { get; set; } = default!;

    /// <summary>
    /// Wms 内部单号，自然键。
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.ORDER_CODE)]
    public virtual string? InboundOrderCode { get; set; }

    /// <summary>
    /// 业务类型。
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.BIZ_TYPE)]
    public virtual string BizType { get; set; } = default!;


    /// <summary>
    /// 业务单号，例如采购单，退货单，生产计划编号。
    /// </summary>
    [MaxLength(DATA_LENGTH.ORDER_CODE)]
    public virtual string? BizOrder { get; set; }

    /// <summary>
    /// 是否已关闭。关闭的入库单不能再入库。
    /// </summary>
    public virtual bool Closed { get; set; }

    /// <summary>
    /// 关单人。可以为空。
    /// </summary>
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? ClosedBy { get; set; }

    /// <summary>
    /// 关闭时间。
    /// </summary>
    public virtual DateTime? ClosedAt { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Comment { get; set; }

    /// <summary>
    /// 此入库单中的行。
    /// </summary>
    public virtual IEnumerable<InboundLine> Lines
    {
        get
        {
            return _lines;
        }
        protected set
        {
            _lines = (ISet<InboundLine>)value;
        }
    }

    /// <summary>
    /// 向此入库单中添加行。
    /// </summary>
    /// <param name="line"></param>
    public virtual void AddLine(InboundLine line)
    {
        if (line.InboundOrder != null)
        {
            throw new InvalidOperationException("入库行已属于其他入库单。");
        }

        line.InboundOrder = this;
        this._lines.Add(line);
    }


    public virtual void RemoveLine(InboundLine line)
    {
        line.InboundOrder = null!;
        this._lines.Remove(line);
    }

    public override string? ToString()
    {
        return this.InboundOrderCode;
    }
}
