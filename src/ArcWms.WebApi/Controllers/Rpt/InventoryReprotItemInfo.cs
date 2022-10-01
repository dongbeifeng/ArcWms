using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 库存信息
/// </summary>
public class InventoryReprotItemInfo
{
    /// <summary>
    /// 库存最后一次变动时间。
    /// </summary>
    public DateTime ModificationTime { get; set; }

    /// <summary>
    /// 物料代码。
    /// </summary>
    [Required]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料描述。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 批号。
    /// </summary>
    public string? Batch { get; set; }

    /// <summary>
    /// 库存状态。
    /// </summary>
    [MaxLength(10)]
    [Required]
    public string? InventoryStatus { get; set; }

    /// <summary>
    /// 数量。
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 计量单位。
    /// </summary>
    [Required]
    public string? Uom { get; set; }


}

