using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 入库单单据组盘的操作参数。
/// </summary>
public class IboPalletizeArgs
{
    /// <summary>
    /// 托盘号
    /// </summary>
    [Required]
    public string PalletCode { get; set; } = default!;

    /// <summary>
    /// 入库单号
    /// </summary>
    public string InboundOrderCode { get; set; } = default!;

    /// <summary>
    /// 物料编码
    /// </summary>
    [Required]
    public string MaterialCode { get; set; } = default!;

    /// <summary>
    /// 批号
    /// </summary>
    [Required]
    public string Batch { get; set; } = default!;

    /// <summary>
    /// 库存状态
    /// </summary>
    [Required]
    public string InventoryStatus { get; set; } = default!;

    /// <summary>
    /// 数量
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [Required]
    public string Uom { get; set; } = default!;
}


