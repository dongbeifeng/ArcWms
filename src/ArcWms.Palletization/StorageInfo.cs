using System.ComponentModel.DataAnnotations;

namespace ArcWms;


/// <summary>
/// 存储信息。这些信息决定了入库分配货位的结果。
/// </summary>
[Serializable]
public record StorageInfo
{
    /// <summary>
    /// 获取此货载的存储分组。存储分组影响上架时的货位分配。<see cref="Location.StorageGroup"/>。
    /// </summary>
    /// <remarks>
    /// 属性值由 <see cref="IUnitloadStorageInfoProvider.GetStorageGroup(Unitload)"/> 方法提供。
    /// </remarks>
    [Required]
    [MaxLength(10)]
    public string? StorageGroup { get; init; }

    /// <summary>
    /// 获取或设置此货载的容器规格。此属性与 <see cref="Location.Specification"/> 配合使用。
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string? PalletSpecification { get; init; }

    /// <summary>
    /// 获取或设置此货载的出库标记。
    /// 具有相同出库标记的货载在出库时应可互换。
    /// 对于双深位巷道，框架在分配货位时，会尽量使远端和近端存放出库标记相同的货载，
    /// 以减少出库时让路的可能性。
    /// 属性值由 <see cref="IUnitloadStorageInfoProvider.GetOutFlag(Unitload)"/> 方法提供。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string? OutFlag { get; init; }

    /// <summary>
    /// 获取货载的重量，单位千克。<see cref="Location.WeightLimit" />。
    /// </summary>
    public float Weight { get; init; }

    /// <summary>
    /// 获取货载的高度，单位米。<see cref="Location.HeightLimit" />。
    /// </summary>
    public float Height { get; init; }
}
