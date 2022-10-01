using Quartz;
using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// Quartz 的触发器标识，原生的 <see cref="TriggerKey"/> 没有无参构造函数，不能从 http 请求绑定。
/// </summary>
public class QuartzTriggerKeyInfo
{

    /// <summary>
    /// 名称
    /// </summary>
    [Required]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 组
    /// </summary>
    [Required]
    public string Group { get; set; } = default!;

    /// <summary>
    /// 获取 <see cref="TriggerKey"/>
    /// </summary>
    /// <returns></returns>
    public static implicit operator TriggerKey(QuartzTriggerKeyInfo info)
    {
        return new TriggerKey(info.Name, info.Group);
    }
}


