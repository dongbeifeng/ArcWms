using Microsoft.AspNetCore.Identity;

namespace ArcWms.WebApi.Models;

public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// 是否内置角色。
    /// </summary>
    public bool IsBuiltIn { get; set; }

}
