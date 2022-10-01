using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建出库单操作的参数
/// </summary>
public class CreateOutboundOrderArgs
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
    /// 出库行信息
    /// </summary>
    public List<EditOutboundLineInfo> Lines { get; set; } = default!;
}


