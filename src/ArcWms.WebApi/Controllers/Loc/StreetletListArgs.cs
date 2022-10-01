using NHibernateUtils;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 巷道列表的查询参数
/// </summary>
public class StreetletListArgs
{
    /// <summary>
    /// 巷道编码。
    /// </summary>
    [SearchArg]
    public string? StreetletCode { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sort { get; set; } = "StreetletId";

    /// <summary>
    /// 基于 1 的当前页面，默认值为 1。
    /// </summary>
    public int? Current { get; set; }

    /// <summary>
    /// 每页大小，默认值为 10。
    /// </summary>
    public int? PageSize { get; set; }

}

