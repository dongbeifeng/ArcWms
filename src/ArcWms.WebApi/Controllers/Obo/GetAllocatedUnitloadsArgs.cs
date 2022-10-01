using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 获取分配给出库单的货载的查询参数。
/// </summary>
public class GetAllocatedUnitloadsArgs
{
    /// <summary>
    /// 出库单 Id。
    /// </summary>
    [Required]
    public int? OutboundOrderId { get; set; }
}

