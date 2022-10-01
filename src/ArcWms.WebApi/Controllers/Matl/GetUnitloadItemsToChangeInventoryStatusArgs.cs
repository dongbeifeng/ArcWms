using NHibernateUtils;
using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 货载明细列表的查询参数
/// </summary>
public class GetUnitloadItemsToChangeInventoryStatusArgs
{
    /// <summary>
    /// 业务类型
    /// </summary>
    [Required]
    public string? BizType { get; set; }



    /// <summary>
    /// 托盘号
    /// </summary>
    [SearchArg]
    public string? PalletCode { get; set; }


    /// <summary>
    /// 物料编码
    /// </summary>
    [SearchArg("Material.MaterialCode")]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 批号
    /// </summary>
    [SearchArg]
    public string? Batch { get; set; }

    // TODO 重复的代码
    /// <summary>
    /// 库存状态
    /// </summary>
    [SearchArg]
    public string? InventoryStatus => BizType switch
    {
        "待检转合格" => "待检",
        "待检转不合格" => "待检",
        "不合格转合格" => "不合格",
        "合格转不合格" => "合格",
        _ => throw new(),
    };


    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sort { get; set; }

}

