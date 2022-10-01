namespace ArcWms.WebApi.Controllers;

// TODO 多处引用，写转换函数
/// <summary>
/// 货载明细信息
/// </summary>
public class UnitloadItemInfo
{
    /// <summary>
    /// 货载项Id
    /// </summary>
    public int UnitloadItemId { get; set; }

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
    /// 批号
    /// </summary>
    public string? Batch { get; set; }

    /// <summary>
    /// 库存状态
    /// </summary>
    public string? InventoryStatus { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 已分配给出库单的数量
    /// </summary>
    public decimal QuantityAllocatedToOutboundOrder
    {
        get
        {
            return this.AllocationsToOutboundOrder == null || this.AllocationsToOutboundOrder.Length == 0
                ? 0m
                : this.AllocationsToOutboundOrder.Sum(x => x.QuantityAllocated);
        }
    }

    /// <summary>
    /// 分配给出库单行的数量明细，字典的键表示出库单明细的Id，字典的值表示分配的数量
    /// </summary>
    public AllocationInfoToOutboundOrder[]? AllocationsToOutboundOrder { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    public string? Uom { get; set; }


    /// <summary>
    /// 分配信息
    /// </summary>
    public class AllocationInfoToOutboundOrder
    {
        /// <summary>
        /// 分配信息Id
        /// </summary>
        public int UnitloadItemAllocationId { get; set; }

        /// <summary>
        /// 出库单明细Id
        /// </summary>
        public int OutboundLineId { get; set; }

        /// <summary>
        /// 分配数量
        /// </summary>
        public decimal QuantityAllocated { get; set; }
    }
}


