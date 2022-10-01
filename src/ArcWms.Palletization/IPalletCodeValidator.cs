namespace ArcWms;

// TODO 扩展点

public interface IPalletCodeValidator
{
    /// <summary>
    /// 判断托盘号是否符合编码规则，但不检查托盘号是否占用。
    /// </summary>
    /// <param name="palletCode">要检查的托盘号</param>
    /// <param name="msg">描述性文本</param>
    /// <returns>true 表示托盘号有效，否则为无效</returns>
    bool IsValid(string palletCode, out string msg);
}
