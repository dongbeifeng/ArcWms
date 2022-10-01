namespace ArcWms;

public class PalletizationItemInfo
{
    public InventoryKey? InventoryKey { get; set; }

    public decimal Quantity { get; set; }

    public string? Fifo { get; set; }

    public DateTime AgeBaseline { get; set; }
}
