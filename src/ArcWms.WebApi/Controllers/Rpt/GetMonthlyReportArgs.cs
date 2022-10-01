namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 月报查询参数。
/// </summary>
public class GetMonthlyReportArgs
{
    /// <summary>
    /// 表示月份的时间，例如 2021-01
    /// </summary>
    public string Month { get; set; }
}

