namespace ArcWms;

/// <summary>
/// 在库存余额为负数时引发的异常。
/// </summary>
[Serializable]
public class NegativeBalanceException : Exception
{
    InventoryKey _inventoryKey;
    public NegativeBalanceException(InventoryKey inventoryKey)
    {
        _inventoryKey = inventoryKey;
    }

    public override string Message => $"库存余额为负数：{_inventoryKey}。";
}