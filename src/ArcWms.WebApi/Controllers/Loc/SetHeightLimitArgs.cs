using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 设置限高的操作参数
/// </summary>
public class SetHeightLimitArgs
{
    /// <summary>
    /// 要设置限高的位置 Id 列表
    /// </summary>
    public int[] LocationIds { get; set; } = new int[0];

    /// <summary>
    /// 限高。
    /// </summary>
    [Required]
    public float HeightLimit { get; set; }
}
