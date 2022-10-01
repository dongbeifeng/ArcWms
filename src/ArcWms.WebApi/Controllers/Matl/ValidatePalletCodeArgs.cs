namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 验证托盘号的操作参数。
/// </summary>
public class ValidatePalletCodeArgs
{
    /// <summary>
    /// 要验证的托盘号。
    /// </summary>
    public string? PalletCode { get; set; }
}

