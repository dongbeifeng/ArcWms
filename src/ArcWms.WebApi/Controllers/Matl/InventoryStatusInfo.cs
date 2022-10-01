namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 库存状态信息。
/// </summary>
public class InventoryStatusInfo
{
    /// <summary>
    /// 库存状态。
    /// </summary>
    public string InventoryStatus { get; set; }

    /// <summary>
    /// 展示名称。
    /// </summary>
    public string DisplayName { get; set; }
}

