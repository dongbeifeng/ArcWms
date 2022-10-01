using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建用户的操作参数
/// </summary>
public class CreateUserArgs
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string UserName { get; set; } = default!;

    /// <summary>
    /// 所属角色
    /// </summary>
    public string[]? Roles { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string Password { get; set; } = default!;
}

