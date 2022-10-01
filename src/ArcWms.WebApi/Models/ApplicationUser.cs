using Microsoft.AspNetCore.Identity;

namespace ArcWms.WebApi.Models;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// 是否内置用户。
    /// </summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>
    /// 刷新令牌。
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 刷新令牌的生成时间。
    /// </summary>
    public DateTime? RefreshTokenTime { get; set; }

    /// <summary>
    /// 刷新令牌的过期时间。
    /// </summary>
    public DateTime? RefreshTokenExpireTime { get; set; }

}
