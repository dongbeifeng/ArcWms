using Microsoft.EntityFrameworkCore;

namespace ArcWms.WebApi.Models;

/// <summary>
/// 表示技术日志数据库。
/// </summary>
public class LogDbContext : DbContext
{
    /// <summary>
    /// 初始化新实例。
    /// </summary>
    /// <param name="options"></param>
    public LogDbContext(DbContextOptions<LogDbContext> options)
        : base(options)
    {
    }

    public DbSet<LogEntry> Logs { get; set; }
}
