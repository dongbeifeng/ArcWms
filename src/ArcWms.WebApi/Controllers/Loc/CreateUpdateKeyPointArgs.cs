using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建和编辑关键点的操作参数
/// </summary>
public class CreateUpdateKeyPointArgs
{
    /// <summary>
    /// 货位编号
    /// </summary>
    [Required]
    public string LocationCode { get; set; } = default!;

    /// <summary>
    /// 请求类型
    /// </summary>
    public string? RequestType { get; set; }

    /// <summary>
    /// 标记
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 入站数限制
    /// </summary>
    [Range(1, 999)]
    public int InboundLimit { get; set; }

    /// <summary>
    /// 出站数限制
    /// </summary>
    [Range(1, 999)]
    public int OutboundLimit { get; set; }
}

