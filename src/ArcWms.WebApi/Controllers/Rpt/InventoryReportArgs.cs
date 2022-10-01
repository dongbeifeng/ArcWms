using NHibernateUtils;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 实时库存报表的查询参数
/// </summary>
public class InventoryReportArgs
{
    /// <summary>
    /// 物料编码
    /// </summary>
    [SearchArg("Material.MaterialCode")]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 批号
    /// </summary>
    [SearchArg]
    public string? Batch { get; set; }


    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// 基于 1 的当前页面。
    /// </summary>
    public int Current { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;

}


