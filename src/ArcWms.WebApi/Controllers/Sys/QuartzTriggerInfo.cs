namespace ArcWms.WebApi.Controllers;

/// <summary>
/// Quartz 的触发器信息
/// </summary>
public class QuartzTriggerInfo
{
    /// <summary>
    /// 触发器的名称
    /// </summary>
    public string? TriggerName { get; set; }

    /// <summary>
    /// 触发器的组
    /// </summary>
    public string? TriggerGroup { get; set; }

    /// <summary>
    /// 触发器说明
    /// </summary>
    public string? TriggerDescription { get; set; }

    /// <summary>
    /// Cron 表达式
    /// </summary>
    public string? CronExpressionString { get; set; }


    /// <summary>
    /// 上次触发时间
    /// </summary>
    public DateTime? PreviousFireTime { get; set; }

    /// <summary>
    /// 下次触发时间
    /// </summary>
    public DateTime? NextFireTime { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string? TriggerState { get; set; }
}


