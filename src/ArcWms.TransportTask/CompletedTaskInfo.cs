namespace ArcWms;

/// <summary>
/// 表示已完成的任务信息。
/// </summary>
public record CompletedTaskInfo
{
    /// <summary>
    /// 任务编号。
    /// </summary>
    public string TaskCode { get; set; }

    /// <summary>
    /// 任务类型。
    /// </summary>
    public string TaskType { get; set; }


    /// <summary>
    /// 指示任务是否已被取消。
    /// </summary>
    public bool Cancelled { get; set; }


    /// <summary>
    /// 实际完成位置。
    /// </summary>
    public string? ActualEnd { get; set; }

    /// <summary>
    /// 附加信息。
    /// </summary>
    public dynamic? AdditionalInfo { get; set; }
}
