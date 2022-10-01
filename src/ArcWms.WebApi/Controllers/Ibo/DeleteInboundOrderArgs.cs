using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 删除入库单的操作参数。
/// </summary>
public class DeleteInboundOrderArgs
{
    /// <summary>
    /// 入库单Id。
    /// </summary>
    [Required]
    public int? InboundOrderId { get; set; }
}
