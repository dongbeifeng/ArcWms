using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 更改货载位置的操作参数
/// </summary>
public class ChangeLocationArgs
{
    /// <summary>
    /// 要更改位置的托盘号。
    /// </summary>
    [Required]
    public string PalletCode { get; set; } = default!;

    /// <summary>
    /// 目标货位编码.
    /// </summary>
    [Required]
    public string DestinationLocationCode { get; set; } = default!;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }
}


