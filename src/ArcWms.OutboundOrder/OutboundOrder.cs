using NHibernateUtils;
using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示出库单。
/// </summary>
public class OutboundOrder : IUnitloadAllocationTable
{

    public OutboundOrder()
    {
        this.CreationTime = DateTime.Now;
        this.ModificationTime = DateTime.Now;
        this.Lines = new HashSet<OutboundLine>();
    }

    /// <summary>
    /// 出库单Id
    /// </summary>
    public virtual int OutboundOrderId { get; protected set; }

    /// <summary>
    /// 出库单编号。
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.ORDER_CODE)]
    public virtual string? OutboundOrderCode { get; set; }

    public virtual int v { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [CreationTime]
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [CreationUser]
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? CreationUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    [ModificationTime]
    public virtual DateTime ModificationTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    [ModificationUser]
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? ModificationUser { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Required]
    [MaxLength(DATA_LENGTH.BIZ_TYPE)]
    public virtual string BizType { get; set; }

    /// <summary>
    /// 业务单据号
    /// </summary>
    [MaxLength(DATA_LENGTH.ORDER_CODE)]
    public virtual string? BizOrder { get; set; }

    /// <summary>
    /// 是否已关闭
    /// </summary>
    public virtual bool Closed { get; set; }

    /// <summary>
    /// 关闭时间
    /// </summary>
    public virtual DateTime? ClosedAt { get; set; }

    /// <summary>
    /// 由谁关闭
    /// </summary>
    [MaxLength(DATA_LENGTH.USERNAME)]
    public virtual string? ClosedBy { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Comment { get; set; }

    /// <summary>
    /// 出库单明细集合。
    /// </summary>
    public virtual ISet<OutboundLine> Lines { get; protected set; }

    /// <summary>
    /// 向此出库单添加明细。
    /// </summary>
    /// <param name="line"></param>
    public virtual void AddLine(OutboundLine line)
    {
        if (line.OutboundOrder != null)
        {
            throw new InvalidOperationException("出库单明细已属于其他出库单。");
        }

        line.OutboundOrder = this;
        this.Lines.Add(line);
    }

    /// <summary>
    /// 从此出库单移除明细
    /// </summary>
    /// <param name="line"></param>
    public virtual void RemoveLine(OutboundLine line)
    {
        line.OutboundOrder = null;
        this.Lines.Remove(line);
    }

    /// <summary>
    /// 计算货载项在此出库单中的分配数量。
    /// </summary>
    /// <param name="unitloadItem"></param>
    /// <returns></returns>
    public virtual decimal ComputeAllocated(UnitloadItem unitloadItem)
    {
        return Lines.SelectMany(x => x.Allocations)
            .Where(x => x.UnitloadItem == unitloadItem)
            .Sum(x => x.QuantityAllocated);
    }


    public override string? ToString()
    {
        return this.OutboundOrderCode;
    }

    public virtual string GetOrderType()
    {
        // TODO 处理硬编码
        return "出库单";
    }
}
