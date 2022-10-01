using NHibernateUtils;
using System.Data;
using System.Linq.Expressions;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 货载列表的查询参数。
/// </summary>
public class UnitloadListArgs
{
    /// <summary>
    /// 托盘号，
    /// </summary>
    [SearchArg]
    public string? PalletCode { get; set; }

    /// <summary>
    /// 物料类型
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public string? MaterialType { get; set; }

    internal Expression<Func<Unitload, bool>>? MaterialTypeExpr
    {
        get
        {
            return this.MaterialType switch
            {
                null => null,
                _ => x => x.Items.Any(i => i.Material.MaterialType == this.MaterialType)
            };
        }
    }

    /// <summary>
    /// 是否已分配
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public bool? Allocated { get; set; }

    internal Expression<Func<Unitload, bool>>? AllocatedExpr
    {
        get
        {
            return this.Allocated switch
            {
                null => null,
                true => x => x.CurrentUat != null,
                false => x => x.CurrentUat == null,
            };
        }
    }

    /// <summary>
    /// 物料编码。
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public string? MaterialCode { get; set; }

    internal Expression<Func<Unitload, bool>>? MaterialCodeExpr
    {
        get
        {
            return this.MaterialCode switch
            {
                null => null,
                _ => x => x.Items.Select(i => i.Material).Any(m => SqlMethods.Like(m.MaterialCode, this.MaterialCode))
            };
        }
    }

    /// <summary>
    /// 批号。
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public string? Batch { get; set; }

    internal Expression<Func<Unitload, bool>>? BatchExpr
    {
        get
        {
            return this.Batch switch
            {
                null => null,
                _ => x => x.Items.Any(i => SqlMethods.Like(i.Batch, Batch))
            };
        }
    }

    /// <summary>
    /// 库存状态
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public string? InventoryStatus { get; set; }

    internal Expression<Func<Unitload, bool>>? InventoryStatusExpr
    {
        get
        {
            return this.InventoryStatus switch
            {
                null => null,
                _ => x => x.Items.Any(i => i.InventoryStatus == InventoryStatus)
            };
        }
    }

    /// <summary>
    /// 托盘所在巷道
    /// </summary>
    [SearchArg]
    public int? StreetletId { get; set; }

    /// <summary>
    /// 托盘所在货位号。
    /// </summary>
    [SearchArg("CurrentLocation.LocationCode")]
    public string? LocationCode { get; set; }

    /// <summary>
    /// 托盘是否有任务
    /// </summary>
    [SearchArg]
    public bool? BeingMoved { get; set; }

    /// <summary>
    /// 托盘的操作提示
    /// </summary>
    [SearchArg]
    public string? OpHintType { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// 基于 1 的当前页面，默认值为 1。
    /// </summary>
    public int? Current { get; set; } = 1;

    /// <summary>
    /// 每页大小，默认值为 10。
    /// </summary>
    public int? PageSize { get; set; }

}


