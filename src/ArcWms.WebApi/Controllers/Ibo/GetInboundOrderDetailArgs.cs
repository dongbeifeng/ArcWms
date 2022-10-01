using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 获取入库单详细信息
/// </summary>
public class GetInboundOrderDetailArgs
{
    /// <summary>
    /// 入库单Id。
    /// </summary>
    [Required]
    public int? InboundOrderId { get; set; }
}
