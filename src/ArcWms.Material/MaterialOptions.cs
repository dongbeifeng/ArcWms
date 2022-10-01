namespace ArcWms;

public record MaterialOptions
{
    public MaterialType[] MaterialTypes { get; set; } = new MaterialType[0];

    public BizType[] BizTypes { get; set; } = new BizType[0];

    public InventoryStatus[] InventoryStatus { get; set; } = new InventoryStatus[0];

}
