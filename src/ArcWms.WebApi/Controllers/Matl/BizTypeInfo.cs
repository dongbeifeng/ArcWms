namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 业务类型信息。
/// </summary>
public class BizTypeInfo
{
    /// <summary>
    /// 业务类型。
    /// </summary>
    public string? BizType { get; set; }

    /// <summary>
    /// 展示名称。
    /// </summary>
    public string? DisplayName { get; set; }


    /// <summary>
    /// 更改库存状态时的发出状态。
    /// </summary>
    public string? IssuingStatus { get; set; }

    /// <summary>
    /// 更改库存状态时的接收状态。
    /// </summary>
    public string? ReceivingStatus { get; set; }

}

