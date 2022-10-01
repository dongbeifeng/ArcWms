namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 为变更库存状态展示的货载明细信息
/// </summary>
public class ChangeInventoryStatusUnitloadItemInfo
{
    /// <summary>
    /// 货载项Id
    /// </summary>
    public int UnitloadItemId { get; set; }

    /// <summary>
    /// 托盘号
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// 所在货位编码
    /// </summary>
    public string? LocationCode { get; set; }


    /// <summary>
    /// 所在巷道编码
    /// </summary>
    public string? StreetletCode { get; set; }

    /// <summary>
    /// 托盘是否已分配
    /// </summary>
    public bool Allocated { get; set; }

    /// <summary>
    /// 托盘是否有任务
    /// </summary>
    public bool HasTask { get; set; }

    /// <summary>
    /// 托盘是否有盘点错误
    /// </summary>
    public bool HasCountingError { get; set; }

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
    /// 计量单位
    /// </summary>
    public string? Uom { get; set; } = default!;

    /// <summary>
    /// 是否可变更库存状态
    /// </summary>
    public bool CanChangeInventoryStatus { get; set; }

    /// <summary>
    /// 不可变更库存状态的原因
    /// </summary>
    public string ReasonWhyInventoryStatusCannotBeChanged { get; set; } = string.Empty;

}


