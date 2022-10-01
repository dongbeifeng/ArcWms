namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 货载详情
/// </summary>
public class UnitloadDetail : UnitloadInfo
{
    /// <summary>
    /// 货载到达当前位置的时间
    /// </summary>
    public DateTime LocationTime { get; set; }


    /// <summary>
    /// 操作提示类型
    /// </summary>
    public string? OpHintType { get; set; }

    /// <summary>
    /// 操作提示信息
    /// </summary>
    public string? OpHintInfo { get; set; }


    /// <summary>
    /// 货载的重量，单位千克。
    /// </summary>
    public float Weight { get; set; }

    /// <summary>
    /// 货载的高度，单位米。
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// 存储分组。
    /// </summary>
    public string? StorageGroup { get; set; }


    /// <summary>
    /// 出库标记
    /// </summary>
    public string? OutFlag { get; set; }

    /// <summary>
    /// 容器规格。
    /// </summary>
    public string? PalletSpecification { get; set; }


    /// <summary>
    /// 当前分配到的单据
    /// </summary>
    public string? CurrentUat { get; set; }

    /// <summary>
    /// 当前任务号
    /// </summary>
    public string? CurrentTaskCode { get; set; }

    /// <summary>
    /// 当前任务类型
    /// </summary>
    public string? CurrentTaskType { get; set; }

    /// <summary>
    /// 当前任务起点位置编码
    /// </summary>
    public string? CurrentTaskStartLocationCode { get; set; }

    /// <summary>
    /// 当前任务终点位置编码
    /// </summary>
    public string? CurrentTaskEndLocationCode { get; set; }


}

