using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 设置巷道可到达的出货口的操作参数。
/// </summary>
public class SetOutletsArgs
{
    /// <summary>
    /// 要设置出口的巷道 Id。
    /// </summary>
    [Required]
    public int? StreetletId { get; set; }

    /// <summary>
    /// 出口Id列表。
    /// </summary>
    [Required]
    public int[] OutletIdList { get; set; } = default!;
}

