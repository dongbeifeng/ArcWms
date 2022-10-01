namespace ArcWms.WebApi.MetaData;

class SupportedInventoryStatuses
{
    public static readonly InventoryStatus 待检 = new InventoryStatus("待检");
    public static readonly InventoryStatus 合格 = new InventoryStatus("合格");
    public static readonly InventoryStatus 不合格 = new InventoryStatus("不合格");
}
