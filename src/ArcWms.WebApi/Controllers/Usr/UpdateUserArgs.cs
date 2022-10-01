namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 编辑用户的操作参数。
/// </summary>
public class UpdateUserArgs
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 所属角色
    /// </summary>
    public string[]? Roles { get; set; }


    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }
    public string UserId { get; set; }
}

