using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建出库单或编辑出库单操作中的出库行信息。
/// </summary>
public class EditOutboundLineInfo
{
    /// <summary>
    /// 本行的操作：
    /// add 表示新增，
    /// edit 表示编辑，
    /// delete 表示删除。
    /// </summary>
    [Required]
    public string Op { get; set; } = default!;


    /// <summary>
    /// 出库单明细Id，用户在界面上新增的明细Id为0。
    /// </summary>
    public int OutboundLineId { get; set; }


    /// <summary>
    /// 物料代码
    /// </summary>
    [Required]
    public string MaterialCode { get; set; } = default!;


    /// <summary>
    /// 要出库的批号，可以为空
    /// </summary>
    public string? Batch { get; set; }


    /// <summary>
    /// 要出库的库存状态。
    /// </summary>
    [Required]
    public string InventoryStatus { get; set; }

    /// <summary>
    /// 计量单位。
    /// </summary>
    [Required]
    public string Uom { get; set; } = default!;

    /// <summary>
    /// 需求数量。
    /// </summary>
    [Range(0, int.MaxValue)]
    public decimal QuantityDemanded { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }
}


