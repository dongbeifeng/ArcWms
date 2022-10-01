namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 获取储位详细信息的查询参数。
/// </summary>
public class GetStorageLocationDetailArgs
{
    /// <summary>
    /// 货位编码。
    /// </summary>
    public string? LocationCode { get; set; }
}
