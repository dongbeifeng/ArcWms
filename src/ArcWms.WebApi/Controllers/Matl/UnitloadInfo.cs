using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 货载列表的数据项。
/// </summary>
public class UnitloadInfo
{
    /// <summary>
    /// 货载Id
    /// </summary>
    public int UnitloadId { get; set; }

    /// <summary>
    /// 托盘号
    /// </summary>
    [Required]
    public string? PalletCode { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime ModificationTime { get; set; }

    /// <summary>
    /// 所在货位编码
    /// </summary>
    public string? LocationCode { get; set; }

    /// <summary>
    /// 所在货位类型
    /// </summary>
    public string? LocationType { get; set; }

    /// <summary>
    /// 所在巷道编码
    /// </summary>
    public string? StreetletCode { get; set; }

    /// <summary>
    /// 托盘是否已分配
    /// </summary>
    public bool Allocated { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// 是否有任务
    /// </summary>
    public bool HasTask { get; set; }

    /// <summary>
    /// 货载明细列表
    /// </summary>
    public List<UnitloadItemInfo> Items { get; set; } = new List<UnitloadItemInfo>();
}


