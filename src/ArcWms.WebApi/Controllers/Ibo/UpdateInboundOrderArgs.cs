using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 编辑入库单操作的参数
/// </summary>
public class UpdateInboundOrderArgs
{
    /// <summary>
    /// 要更新的入库单 Id。
    /// </summary>
    [Required]
    public int? InboundOrderId { get; set; }

    /// <summary>
    /// 业务单号
    /// </summary>
    public string? BizOrder { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 入库明细
    /// </summary>
    public List<EditInboundLineInfo>? Lines { get; set; }
}

