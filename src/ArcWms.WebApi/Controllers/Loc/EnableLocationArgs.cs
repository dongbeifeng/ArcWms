namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 启用或禁用位置的操作参数。
/// </summary>
public class EnableLocationArgs
{
    /// <summary>
    /// 要启用或禁用的位置 Id 列表。
    /// </summary>
    public int[] LocationIds { get; set; } = new int[0];

    /// <summary>
    /// 启用或禁用位置的备注。
    /// </summary>
    public string? Comment { get; set; }
}
