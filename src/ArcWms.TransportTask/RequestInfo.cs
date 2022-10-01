namespace ArcWms;

/// <summary>
/// 请求信息。
/// </summary>
public record RequestInfo
{
    /// <summary>
    /// 请求类型。
    /// </summary>
    public string? RequestType { get; init; }

    /// <summary>
    /// 请求发出的位置。
    /// </summary>
    public string? LocationCode { get; init; }

    /// <summary>
    /// 容器编码。
    /// </summary>
    public string? PalletCode { get; init; }

    /// <summary>
    /// 重量。
    /// </summary>
    public float Weight { get; init; }

    /// <summary>
    /// 高度。
    /// </summary>
    public float Height { get; init; }

    /// <summary>
    /// 附加信息。
    /// </summary>
    public dynamic? AdditionalInfo { get; set; }

}
