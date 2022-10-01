using NHibernateUtils;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 流水列表的查询参数
/// </summary>
public class FlowListArgs
{
    /// <summary>
    /// 开始时间
    /// </summary>
    [SearchArg(nameof(Flow.CreationTime), SearchMode.GreaterThanOrEqual)]
    public DateTime? TimeFrom { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SearchArg(nameof(Flow.CreationTime), SearchMode.LessThan)]
    public DateTime? TimeTo { get; set; }

    /// <summary>
    /// 物料类型
    /// </summary>
    [SearchArg("Material.MaterialType", SearchMode.Equal)]
    public string? MaterialType { get; set; }

    /// <summary>
    /// 物料代码。
    /// </summary>
    [SearchArg("Material.MaterialCode")]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 托盘号，支持模糊查询。
    /// </summary>
    [SearchArg]
    public string? PalletCode { get; set; }

    /// <summary>
    /// 批号，支持模糊查询。
    /// </summary>
    [SearchArg]
    public string? Batch { get; set; }

    /// <summary>
    /// 库存状态
    /// </summary>
    [SearchArg(SearchMode.In)]
    public string[]? InventoryStatus { get; set; }

    /// <summary>
    /// 单号，支持模糊查询
    /// </summary>
    [SearchArg]
    public string? OrderCode { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [SearchArg(nameof(Flow.BizType), SearchMode.In)]
    public string[]? BizTypes { get; set; }

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

