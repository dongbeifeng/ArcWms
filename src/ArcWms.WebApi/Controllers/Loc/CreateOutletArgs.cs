using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建出口的操作参数
/// </summary>
public class CreateOutletArgs
{
    /// <summary>
    /// 出货口编码
    /// </summary>
    [Required]
    public string OutletCode { get; set; } = default!;

    /// <summary>
    /// 关键点一的编码
    /// </summary>
    [Required]
    public string KP1 { get; set; } = default!;

    /// <summary>
    /// 关键点二的编码，可以为 null
    /// </summary>
    public string? KP2 { get; set; }
}

