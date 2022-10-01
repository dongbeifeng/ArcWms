namespace ArcWms;

[Serializable]
public class InvalidPalletCodeException : Exception
{
    public InvalidPalletCodeException(string palletCode, string error) : base($"无效的托盘号：{palletCode}，错误：{error}。")
    {
    }
}
