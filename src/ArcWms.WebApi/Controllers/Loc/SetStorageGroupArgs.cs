using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 设置存储分组的操作参数
/// </summary>
public class SetStorageGroupArgs
{
    /// <summary>
    /// 要设置分组的位置 Id 列表
    /// </summary>
    public int[] LocationIds { get; set; } = new int[0];

    /// <summary>
    /// 禁用或启用位置的操作备注。
    /// </summary>
    [Required]
    public string StorageGroup { get; set; } = default!;
}

