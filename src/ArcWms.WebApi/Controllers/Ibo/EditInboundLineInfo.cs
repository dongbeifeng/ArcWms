using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建入库单或编辑入库单操作中的入库行信息。
/// </summary>
public class EditInboundLineInfo
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
    /// 入库单明细Id，用户在界面上新增的明细Id为0。
    /// </summary>
    public int InboundLineId { get; set; }


    /// <summary>
    /// 物料代码
    /// </summary>
    [Required]
    public string MaterialCode { get; set; } = default!;


    /// <summary>
    /// 要入库的批号，可以为空
    /// </summary>
    public string? Batch { get; set; } = default!;


    /// <summary>
    /// 要入库的库存状态。
    /// </summary>
    [Required]
    public string InventoryStatus { get; set; } = default!;

    /// <summary>
    /// 计量单位。
    /// </summary>
    [Required]
    public string Uom { get; set; } = default!;

    /// <summary>
    /// 应入数量
    /// </summary>
    [Range(0, int.MaxValue)]
    public decimal QuantityExpected { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }
}

