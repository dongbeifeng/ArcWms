namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 出库单明细
/// </summary>
public class OutboundLineInfo
{
    /// <summary>
    /// 出库单明细Id。
    /// </summary>
    public int OutboundLineId { get; set; }

    /// <summary>
    /// 物料Id
    /// </summary>
    public int MaterialId { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料类型
    /// </summary>
    public string? MaterialType { get; set; }

    /// <summary>
    /// 物料描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string? Specification { get; set; }


    /// <summary>
    /// 要出库的批号，可以为空
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
    /// 需求数量。
    /// </summary>
    public decimal QuantityDemanded { get; set; }

    /// <summary>
    /// 已出数量
    /// </summary>
    public decimal QuantityFulfilled { get; set; }

    /// <summary>
    /// 未出数量，MAX(应出-已出, 0)
    /// </summary>
    public decimal QuantityUnfulfilled { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }
}


