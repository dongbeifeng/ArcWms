namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 物料选择列表的查询参数
/// </summary>
public class MaterialOptionsArgs
{
    /// <summary>
    /// 关键字
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 物料类型
    /// </summary>
    public string? MaterialType { get; set; }

    /// <summary>
    /// 是否只取有库存的物料
    /// </summary>
    public bool InStockOnly { get; set; }

    /// <summary>
    /// 取多少条记录，默认为 10
    /// </summary>
    public int? Limit { get; set; } = 10;

}


