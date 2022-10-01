using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建入库单的操作参数
/// </summary>
public class CreateInboundOrderArgs
{
    /// <summary>
    /// 业务类型
    /// </summary>
    [Required]
    public string BizType { get; set; } = default!;

    /// <summary>
    /// 业务单据号
    /// </summary>
    public string? BizOrder { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 入库行信息
    /// </summary>
    public List<EditInboundLineInfo> Lines { get; set; } = default!;
}
