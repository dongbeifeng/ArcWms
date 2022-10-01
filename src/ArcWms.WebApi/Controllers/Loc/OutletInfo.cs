using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 出口信息。
/// </summary>
public class OutletInfo
{
    /// <summary>
    /// 出货口Id
    /// </summary>
    public int OutletId { get; init; }

    /// <summary>
    /// 出货口编码
    /// </summary>
    [Required]
    public string? OutletCode { get; init; }


    /// <summary>
    /// 出货口的关键点1，不为 null
    /// </summary>
    [Required]
    public string? KP1 { get; init; }

    /// <summary>
    /// 出货口的关键点2，可能为 null
    /// </summary>
    public string? KP2 { get; init; }

    /// <summary>
    /// 可到达此出货口的巷道
    /// </summary>
    public string[]? Streetlets { get; init; }

    // TODO 重命名
    /// <summary>
    /// 当前下架的单据，例如“出库单”
    /// </summary>
    public string? CurrentUat { get; init; }


    // TODO 重命名
    /// <summary>
    /// 最近一次为此出货口调度下架的时间
    /// </summary>
    public DateTime? LastCheckTime { get; init; }

    /// <summary>
    /// 最近一次为此出货口调度下架的消息
    /// </summary>
    public string? LastCheckMessage { get; init; }


}
