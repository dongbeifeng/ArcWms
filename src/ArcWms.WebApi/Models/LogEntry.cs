using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Models;

// TODO 使用ELK处理日志，不使用 SQL Server
/// <summary>
/// 表示一条日志记录。
/// </summary>
public class LogEntry
{
    /// <summary>
    /// 日志记录Id
    /// </summary>
    [Key]
    public long LogId { get; set; }

    /// <summary>
    /// 日志消息
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// 消息模板
    /// </summary>
    public string? MessageTemplate { get; set; }

    /// <summary>
    /// 日志级别
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// 日志时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 异常
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// 附加属性
    /// </summary>
    public string? Properties { get; set; }

    /// <summary>
    /// aspnet请求Id
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 源上下文
    /// </summary>
    public string? SourceContext { get; set; }

    // TODO 改名
    /// <summary>
    /// 指示此日志是否由后台轮询作业驱动产生
    /// </summary>
    public bool? Polling { get; set; }
}
