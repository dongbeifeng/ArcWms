namespace ArcWms;

/// <summary>
/// 表示库存的键。键用于确定库存“是什么”。
/// 两条库存记录只有在库存键相等的情况下才可以进行比较大小和加减操作。
/// 使用 <see cref="IHasInventoryKey"/> 接口标记具有库存键的类。
/// 最基础的库存键由 <see cref="Material"/> 和 <see cref="Uom"/> 两个属性构成。
/// </summary>
public record InventoryKey(Material Material, string Uom);
