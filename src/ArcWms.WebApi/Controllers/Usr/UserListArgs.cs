using ArcWms.WebApi.Models;
using System.Linq.Dynamic.Core;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 用户列表的查询参数
/// </summary>
public class UserListArgs
{
    /// <summary>
    /// 用户名，支持模糊查询
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 在查询对象上应用筛选条件
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public IQueryable<ApplicationUser> Filter(IQueryable<ApplicationUser> q)
    {
        UserName = trim(UserName);

        if (UserName != null)
        {
            q = q.Where(x => x.UserName.Contains(UserName));
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

}

