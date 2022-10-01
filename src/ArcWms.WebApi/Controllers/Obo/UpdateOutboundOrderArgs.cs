namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 编辑出库单操作的参数。
/// </summary>
public class UpdateOutboundOrderArgs
{
    /// <summary>
    /// 要更新的出库单Id。
    /// </summary>
    public int? OutboundOrderId { get; set; }

    /// <summary>
    /// 业务单号
    /// </summary>
    public string? BizOrder { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 出库明细
    /// </summary>
    public List<EditOutboundLineInfo>? Lines { get; set; }
}

