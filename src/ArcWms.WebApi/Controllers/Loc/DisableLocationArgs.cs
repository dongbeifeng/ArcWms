using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 禁用或启用位置的操作参数
/// </summary>
public class DisableLocationArgs
{
    /// <summary>
    /// 要禁用或启用的位置 Id 列表
    /// </summary>
    public int[] LocationIds { get; set; } = new int[0];

    /// <summary>
    /// 禁用或启用位置的操作备注。
    /// </summary>
    [Required]
    public string Comment { get; set; } = default!;
}

