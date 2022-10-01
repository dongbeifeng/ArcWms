using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;


/// <summary>
/// 编辑角色的操作参数
/// </summary>
public class UpdateRoleArgs
{
    [Required]
    public string RoleId { get; set; } = default!;


    /// <summary>
    /// 角色名称
    /// </summary>
    [Required]
    public string RoleName { get; set; } = default!;

}

