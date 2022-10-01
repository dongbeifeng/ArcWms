using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建参数的操作参数
/// </summary>
public class CreateAppSettingArgs
{
    /// <summary>
    /// 参数名称
    /// </summary>
    [Required]
    public string SettingName { get; set; } = default!;

    /// <summary>
    /// 参数类型
    /// </summary>
    [Required]
    public string SettingType { get; set; } = default!;

    /// <summary>
    /// 参数值
    /// </summary>
    [Required]
    public string SettingValue { get; set; } = default!;
}

