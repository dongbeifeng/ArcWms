using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 编辑系统参数的操作参数
/// </summary>
public class UpdateAppSettingArgs
{
    /// <summary>
    /// 参数名称。
    /// </summary>
    [Required]
    public string? SettingName { get; set; }

    /// <summary>
    /// 参数值。
    /// </summary>
    [Required]
    public string SettingValue { get; set; } = default!;
}

