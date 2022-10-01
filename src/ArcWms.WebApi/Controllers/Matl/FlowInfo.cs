using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 流水信息
/// </summary>
public class FlowInfo
{
    /// <summary>
    /// 流水Id
    /// </summary>
    public int FlowId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    [Required]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料类型
    /// </summary>
    public string? MaterialType { get; set; }


    /// <summary>
    /// 物料描述
    /// </summary>
    [Required]
    public string? Description { get; set; }

    /// <summary>
    /// 批号
    /// </summary>
    [Required]
    public string? Batch { get; set; }

    /// <summary>
    /// 库存状态
    /// </summary>
    [Required]
    public string? InventoryStatus { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Required]
    public string? BizType { get; set; }

    /// <summary>
    /// 流动方向
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FlowDirection Direction { get; set; }

    /// <summary>
    /// 托盘号
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// WMS 单号
    /// </summary>
    public string? OrderCode { get; set; }

    /// <summary>
    /// 业务单号
    /// </summary>
    public string? BizOrder { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string? OperationType { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 计量单位
    /// </summary>
    [Required]
    public string? Uom { get; set; }

    /// <summary>
    /// 操作人
    /// </summary>
    public string? CreationUser { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

}


