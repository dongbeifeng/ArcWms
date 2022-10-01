using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 刷新令牌操作的结果
/// </summary>
public class RefreshTokenInfo
{
    /// <summary>
    /// 新的访问令牌
    /// </summary>
    [Required]
    public string? Token { get; set; }

    /// <summary>
    /// 访问令牌的有效时间（分钟）
    /// </summary>
    public int TokenExpiry { get; set; }

    /// <summary>
    /// 新的刷新令牌
    /// </summary>
    [Required]
    public string? RefreshToken { get; set; }
}
