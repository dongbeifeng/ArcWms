using ArcWms.WebApi.Models;
using System.Linq.Dynamic.Core;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 角色列表的查询参数
/// </summary>
public class RoleListArgs
{
    /// <summary>
    /// 角色名，支持模糊查询
    /// </summary>
    public string? RoleName { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// 基于 1 的当前页面，默认值为 1。
    /// </summary>
    public int? Current { get; set; } = 1;

    /// <summary>
    /// 每页大小，默认值为 10。
    /// </summary>
    public int? PageSize { get; set; }


    /// <summary>
    /// 在查询对象上应用筛选条件
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public IQueryable<ApplicationRole> Filter(IQueryable<ApplicationRole> q)
    {
        RoleName = trim(RoleName);

        if (RoleName != null)
        {
            q = q.Where(x => x.Name.Contains(RoleName));
        }

        return q;

        static string? trim(string? str)
        {
            str = str?.Trim();
            if (string.IsNullOrEmpty(str))
            {
                str = null;
            }
            return str;
        }
    }


}

