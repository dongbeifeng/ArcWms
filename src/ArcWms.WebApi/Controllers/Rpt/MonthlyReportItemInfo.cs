using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 库存月报的数据项
/// </summary>
public class MonthlyReportItemInfo
{
    /// <summary>
    /// 月份
    /// </summary>
    public DateTime Month { get; set; }

    /// <summary>
    /// 物料代码
    /// </summary>
    [Required]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料描述
    /// </summary>
    [Required]
    public string? Description { get; set; }


    /// <summary>
    /// 批号
    /// </summary>
    [Required]
    public string Batch { get; set; } = default!;

    /// <summary>
    /// 库存状态
    /// </summary>
    [Required]
    public string StockStatus { get; set; } = default!;

    /// <summary>
    /// 计量单位
    /// </summary>
    [Required]
    public string Uom { get; set; } = default!;

    /// <summary>
    /// 期初数量。期初数量 = 上期期末数量。
    /// </summary>
    public decimal Beginning { get; set; }

    /// <summary>
    /// 流入数量。
    /// </summary>
    public decimal Incoming { get; set; }

    /// <summary>
    /// 流出数量。
    /// </summary>
    public decimal Outgoing { get; set; }

    /// <summary>
    /// 期末数量。期末数量 = 期初数量 + 流入数量 - 流出数量。
    /// </summary>
    public decimal Ending { get; set; }



}


