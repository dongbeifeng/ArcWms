namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 出库单列表页的数据项
/// </summary>
public class OutboundOrderInfo
{
    /// <summary>
    /// 出库单Id
    /// </summary>
    public int OutboundOrderId { get; set; }

    /// <summary>
    /// 出库单编号。
    /// </summary>
    public string OutboundOrderCode { get; set; }


    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreationUser { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ModificationTime { get; set; }

    /// <summary>
    /// 修改人
    /// </summary>
    public string? ModificationUser { get; set; }


    /// <summary>
    /// 业务类型
    /// </summary>
    public string? BizType { get; set; }

    /// <summary>
    /// 业务单据号
    /// </summary>
    public string? BizOrder { get; set; }

    /// <summary>
    /// 是否已关闭
    /// </summary>
    public bool Closed { get; set; }

    /// <summary>
    /// 关闭时间
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// 由谁关闭
    /// </summary>
    public string? ClosedBy { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 出库单明细集合。
    /// </summary>
    public List<OutboundLineInfo>? Lines { get; set; }

    /// <summary>
    /// 已分配的货载数
    /// </summary>
    public int UnitloadCount { get; set; }


}


