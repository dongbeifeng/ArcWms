namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 启用或禁用巷道的操作参数。
/// </summary>
public class EnableStreetletArgs
{
    /// <summary>
    /// 要启用或禁用的巷道 Id。
    /// </summary>
    public int StreetletId { get; set; }

    /// <summary>
    /// 启用或禁用巷道的备注。
    /// </summary>
    public string? Comment { get; set; }
}
