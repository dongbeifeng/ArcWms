using NHibernateUtils;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 系统参数列表的查询参数
/// </summary>
public class AppSettingListArgs
{
    /// <summary>
    /// 系统参数名称。
    /// </summary>
    [SearchArg]
    public string? SettingName { get; set; }

}

