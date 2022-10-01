namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 批号选择列表的查询参数
/// </summary>
public class BatchOptionsArgs
{
    /// <summary>
    /// 关键字
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 库存状态
    /// </summary>
    public string? InventoryStatus { get; set; }

    /// <summary>
    /// 取多少条记录，默认为 10
    /// </summary>
    public int? Limit { get; set; } = 10;

}

