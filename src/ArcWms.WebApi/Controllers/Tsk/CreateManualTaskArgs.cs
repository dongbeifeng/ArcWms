using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建手工任务的操作参数。
/// </summary>
public class CreateManualTaskArgs
{
    /// <summary>
    /// 托盘号
    /// </summary>
    [Required]
    public string? PalletCode { get; set; }

    /// <summary>
    /// 任务类型
    /// </summary>
    [Required]
    public string? TaskType { get; set; }

    /// <summary>
    /// 起点
    /// </summary>
    [Required]
    public string? FromLocationCode { get; set; }

    /// <summary>
    /// 终点
    /// </summary>
    [Required]
    public string? ToLocationCode { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }
}


