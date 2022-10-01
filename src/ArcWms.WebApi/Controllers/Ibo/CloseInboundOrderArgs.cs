using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 关闭入库单的操作参数。
/// </summary>
public class CloseInboundOrderArgs
{
    /// <summary>
    /// 入库单Id。
    /// </summary>
    [Required]
    public int? InboundOrderId { get; set; }
}

