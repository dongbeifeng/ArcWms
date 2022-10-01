namespace ArcWms;

/// <summary>
/// 表示库存的流动方向。
/// </summary>
public enum FlowDirection
{
    /// <summary>
    /// 未设置流水方向，这是非法值。
    /// </summary>
    NotSet = 0,

    /// <summary>
    /// 表示库存流出。
    /// </summary>
    Outbound = -1,

    /// <summary>
    /// 表示库存流入。
    /// </summary>
    Inbound = 1,
}
