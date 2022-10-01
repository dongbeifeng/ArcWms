using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 设置权限的操作参数
/// </summary>
public class SetPermissionArgs
{
    /// <summary>
    /// 允许的操作类型
    /// </summary>
    [Required]
    public string[]? AllowedOperationTypes { get; set; }
    public string RoleId { get; set; }
}

