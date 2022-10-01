using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 清除出口单据的操作参数。
/// </summary>
public class ClearOutletArgs
{
    /// <summary>
    /// 出口 Id。
    /// </summary>
    [Required]
    public int? OutletId { get; set; }

}

