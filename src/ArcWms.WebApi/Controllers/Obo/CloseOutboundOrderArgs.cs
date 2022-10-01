using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 关闭出库单的操作参数。
/// </summary>
public class CloseOutboundOrderArgs
{
    /// <summary>
    /// 要关闭的出库单 Id。
    /// </summary>
    [Required]
    public int? OutboundOrderId { get; set; }
}

