namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 储位详情。
/// </summary>
public class StorageLocationDetail : StorageLocationInfo
{
    /// <summary>
    /// 在此货位上的货载，包含出站任务未完成的货载
    /// </summary>
    public UnitloadDetail[]? Unitloads { get; set; }


    /// <summary>
    /// 货位是否存在
    /// </summary>
    public bool Exists { get; set; }

}

