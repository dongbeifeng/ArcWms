namespace ArcWms.WebApi.Models;

public record InventoryKey2(Material Material, string Batch, string InventoryStatus, string Uom) : InventoryKey(Material, Uom);
