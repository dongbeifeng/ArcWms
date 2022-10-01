using NHibernateUtils;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 物料主数据列表的查询参数。
/// </summary>
public class MaterialListArgs
{
    /// <summary>
    /// 物料代码，
    /// </summary>
    [SearchArg]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料描述
    /// </summary>
    [SearchArg]
    public string? Description { get; set; }

    /// <summary>
    /// 物料类型
    /// </summary>
    [SearchArg(SearchMode.Equal)]
    public string? MaterialType { get; set; }


    /// <summary>
    /// 物料规格。
    /// </summary>
    [SearchArg]
    public string? Specification { get; set; }

    /// <summary>
    /// 物料的备注。
    /// </summary>
    [SearchArg]
    public string? Comment { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// 基于 1 的当前页面，默认值为 1。
    /// </summary>
    public int? Current { get; set; } = 1;

    /// <summary>
    /// 每页大小，默认值为 10。
    /// </summary>
    public int? PageSize { get; set; }

}


