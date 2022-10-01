namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 库龄报表的数据项。
/// </summary>
public class AgeReportItemInfo
{
    /// <summary>
    /// 物料编码。
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料描述。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 物料规格。
    /// </summary>
    public string? Specification { get; set; }

    /// <summary>
    /// 批号。
    /// </summary>
    public string? Batch { get; set; }

    /// <summary>
    /// 库存状态。
    /// </summary>
    public string? InventoryStatus { get; set; }

    /// <summary>
    /// 计量单位。
    /// </summary>
    public string? Uom { get; set; }

    /// <summary>
    /// 7天以内的库存数量
    /// </summary>
    public decimal ZeroToSevenDays { get; set; }

    /// <summary>
    /// 7到30天的库存数量
    /// </summary>
    public decimal SevenToThirtyDays { get; set; }

    /// <summary>
    /// 30天到90天的库存数量
    /// </summary>
    public decimal ThirtyToNinetyDays { get; set; }

    /// <summary>
    /// 90天以上的库存数量
    /// </summary>
    public decimal MoreThanNinetyDays { get; set; }
}

