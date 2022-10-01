namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 是否内置用户，内置用户不能删除
    /// </summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>
    /// 所属角色
    /// </summary>
    public IEnumerable<string>? Roles { get; set; }


}

