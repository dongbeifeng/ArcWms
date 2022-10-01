using NHibernateUtils;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 出库单列表的查询参数
/// </summary>
public class OutboundOrderListArgs
{
    /// <summary>
    /// 出库单编码。
    /// </summary>
    [SearchArg]
    public string? OutboundOrderCode { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [SearchArg]
    public string? BizType { get; set; }

    /// <summary>
    /// 是否显示已关闭的出库单
    /// </summary>
    [SearchArg]
    public bool? Closed { get; set; }

    /// <summary>
    /// 排序字段，例如 F1 DESC, F2 ASC, F3
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


