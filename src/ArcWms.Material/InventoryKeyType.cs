namespace ArcWms;

/// <summary>
/// 表示 <see cref="InventoryKey"/> 的类型，可隐式转换为 <see cref="Type"/>。
/// </summary>
public sealed record InventoryKeyType
{
    readonly Type _inventoryKeyType;

    private InventoryKeyType(Type inventoryKeyType)
    {
        _inventoryKeyType = inventoryKeyType;
    }
    
    public static InventoryKeyType Create<T>() where T : InventoryKey
    {
        return new InventoryKeyType(typeof(T));
    }

    public InventoryKey CreateInstance(params object?[]? args)
    {
        var inventoryKey = (InventoryKey?)Activator.CreateInstance(_inventoryKeyType, args);
        if (inventoryKey == null)
        {
            throw new InvalidOperationException($"未能创建库存键实例：${_inventoryKeyType}。");
        }

        return inventoryKey;
    }

    public static implicit operator Type(InventoryKeyType inventoryKeyType)
    {
        return inventoryKeyType._inventoryKeyType;
    }
}
