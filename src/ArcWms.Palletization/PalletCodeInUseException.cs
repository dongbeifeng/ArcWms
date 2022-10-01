namespace ArcWms;

[Serializable]
public class PalletCodeInUseException : Exception
{
    public PalletCodeInUseException(string palletCode) : base($"托盘号已占用：{palletCode}。")
    {
    }
}
