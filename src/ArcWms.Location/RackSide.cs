namespace ArcWms;

/// <summary>
/// 指示货位在左侧货架还是右侧货架。左右没有绝对意义，仅用于奖两侧的货位区分开。
/// </summary>
public enum RackSide
{
    /// <summary>
    /// 左侧货架。
    /// </summary>
    Left = -1,

    /// <summary>
    /// 未指定货架是左侧还是右侧。
    /// </summary>
    NotSet = 0,

    /// <summary>
    /// 右侧货架。
    /// </summary>
    Right = 1
}
