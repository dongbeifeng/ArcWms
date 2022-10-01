using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 获取出库单详细信息的查询参数。
/// </summary>
public class GetOutboundOrderDetailArgs
{
    /// <summary>
    /// 出库单 Id。
    /// </summary>
    [Required]
    public int? OutboundOrderId { get; set; }
}

