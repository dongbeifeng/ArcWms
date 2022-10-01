using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 设置限重的操作参数
/// </summary>
public class SetWeightLimitArgs
{
    /// <summary>
    /// 要设置限重的位置 Id 列表
    /// </summary>
    public int[] LocationIds { get; set; } = new int[0];

    /// <summary>
    /// 限重。
    /// </summary>
    [Required]
    public float WeightLimit { get; set; }
}
