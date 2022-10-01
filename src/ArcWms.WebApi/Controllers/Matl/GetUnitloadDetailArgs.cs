namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 查询托盘信息的操作参数。
/// </summary>
public class GetUnitloadDetailArgs
{
    /// <summary>
    /// 托盘号。
    /// </summary>
    public string? PalletCode { get; set; }

}

