using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 创建角色的操作参数
/// </summary>
public class CreateRoleArgs
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Required]
    public string RoleName { get; set; } = default!;

}

