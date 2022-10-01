namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 任务信息
/// </summary>
public class TaskInfo
{
    /// <summary>
    /// 任务Id
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// 任务号
    /// </summary>
    public string? TaskCode { get; set; }

    /// <summary>
    /// 任务类型
    /// </summary>
    public string? TaskType { get; set; }

    /// <summary>
    /// 托盘号
    /// </summary>
    public string? PalletCode { get; set; }

    /// <summary>
    /// 起点
    /// </summary>
    public string? StartLocationCode { get; set; }

    /// <summary>
    /// 终点
    /// </summary>
    public string? EndLocationCode { get; set; }

    /// <summary>
    /// 任务下发时间
    /// </summary>
    public DateTime? SendTime { get; set; }

    /// <summary>
    /// 货载明细
    /// </summary>
    public List<UnitloadItemInfo> Items { get; set; } = default!;

    /// <summary>
    /// 单号
    /// </summary>
    public string? OrderCode { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }


}


