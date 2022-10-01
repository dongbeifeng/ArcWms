using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 已登录用户更改自己密码的操作参数
/// </summary>
public class ChangePasswordArgs
{
    /// <summary>
    /// 旧密码
    /// </summary>
    [Required]
    public string? OriginalPassword { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    [Required]
    public string? Password { get; set; }

    /// <summary>
    /// 确认新密码
    /// </summary>
    [Required]
    public string? ConfirmPassword { get; set; }
}
