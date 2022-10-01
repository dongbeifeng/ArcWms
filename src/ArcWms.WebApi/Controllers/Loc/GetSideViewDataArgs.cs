using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 获取侧视图数据的查询参数。
/// </summary>
public class GetSideViewDataArgs
{
    /// <summary>
    /// 巷道编码。
    /// </summary>
    [Required]
    public string? StreetletCode { get; set; }
}
