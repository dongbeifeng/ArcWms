using NHibernateUtils;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 操作记录列表的查询参数。
/// </summary>
public class OpListArgs
{
    /// <summary>
    /// 开始时间
    /// </summary>
    [SearchArg("CreationTime", SearchMode.GreaterThanOrEqual)]
    public DateTime? TimeFrom { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SearchArg("CreationTime", SearchMode.LessThan)]
    public DateTime? TimeTo { get; set; }


    /// <summary>
    /// 操作类型
    /// </summary>
    [SearchArg]
    public string? OperationType { get; set; }


    /// <summary>
    /// 操作人
    /// </summary>
    [SearchArg]
    public string? CreationUser { get; set; }


    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// 基于 1 的当前页面。
    /// </summary>
    public int? Current { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int? PageSize { get; set; }

}

