using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 角色信息
/// </summary>
public class RoleInfo
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [Required]
    public string? RoleId { get; set; }

    /// <summary>
    /// 角色名
    /// </summary>
    [Required]
    public string? RoleName { get; set; }

    /// <summary>
    /// 是否内置角色，内置角色不能删除
    /// </summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>
    /// 允许的操作
    /// </summary>
    public IEnumerable<string>? AllowedOpTypes { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

}

