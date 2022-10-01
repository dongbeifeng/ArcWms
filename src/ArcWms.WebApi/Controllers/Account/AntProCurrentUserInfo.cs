namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 为 Ant Pro 的 currentUser 返回数据
/// </summary>
public class AntProCurrentUserInfo
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 用户 id
    /// </summary>
    public string? Userid { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 签名
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 群组
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public KeyLabelPair[]? Tags { get; set; }

    /// <summary>
    /// 消息数
    /// </summary>
    public int NotifyCount { get; set; }

    /// <summary>
    /// 未读消息数
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 所在省市
    /// </summary>
    public Geographic? Geographic { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 电话
    /// </summary>
    public string? Phone { get; set; }
}

/// <summary>
/// 
/// </summary>
public class KeyLabelPair
{
    /// <summary>
    /// 标签文本
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public string? Key { get; set; }
}

/// <summary>
/// 地理位置
/// </summary>
public class Geographic
{
    /// <summary>
    /// 省
    /// </summary>
    public KeyLabelPair? Province { get; set; }

    /// <summary>
    /// 市
    /// </summary>
    public KeyLabelPair? City { get; set; }
}
