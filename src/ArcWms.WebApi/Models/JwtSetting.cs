namespace ArcWms.WebApi.Models;

public class JwtSetting
{
    /// <summary>
    /// 安全密钥
    /// </summary>
    public string? SecurityKey { get; set; }

    /// <summary>
    /// 颁发者
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// 接收者
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Token 的过期时间，单位为分钟
    /// </summary>
    public int TokenExpiry { get; set; }
}
