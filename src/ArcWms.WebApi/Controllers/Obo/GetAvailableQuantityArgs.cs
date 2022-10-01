namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 批号选择列表的查询参数
/// </summary>
public class GetAvailableQuantityArgs
{
    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 库存状态
    /// </summary>
    public string? InventoryStatus { get; set; }

    /// <summary>
    /// 批号
    /// </summary>
    public string? Batch { get; set; }

    /// <summary>
    /// 出库单号
    /// </summary>
    public string? OutboundOrderCode { get; set; }


}

