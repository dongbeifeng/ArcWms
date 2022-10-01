namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 入库单明细
/// </summary>
public class InboundLineInfo
{
    /// <summary>
    /// 入库单明细Id。
    /// </summary>
    public int InboundLineId { get; set; }

    /// <summary>
    /// 物料Id
    /// </summary>
    public int MaterialId { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; } = default!;

    /// <summary>
    /// 物料类型
    /// </summary>
    public string? MaterialType { get; set; } = default!;

    /// <summary>
    /// 物料描述
    /// </summary>
    public string? Description { get; set; } = default!;

    /// <summary>
    /// 物料规格
    /// </summary>
    public string? Specification { get; set; } = default!;


    /// <summary>
    /// 要出库的批号，不为空
    /// </summary>
    public string? Batch { get; set; }


    /// <summary>
    /// 要出库的库存状态。
    /// </summary>
    public string? InventoryStatus { get; set; }

    /// <summary>
    /// 计量单位。
    /// </summary>
    public string? Uom { get; set; }

    /// <summary>
    /// 应入数量。
    /// </summary>
    public decimal QuantityExpected { get; set; }

    /// <summary>
    /// 已入数量
    /// </summary>
    public decimal QuantityReceived { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }
}

