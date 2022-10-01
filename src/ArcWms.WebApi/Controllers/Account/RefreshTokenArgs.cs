using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 刷新令牌的操作参数
/// </summary>
public class RefreshTokenArgs
{
    /// <summary>
    /// 当前的刷新令牌，用于换取新的访问令牌和刷新令牌
    /// </summary>
    [Required]
    public string RefreshToken { get; set; } = default!;
}
