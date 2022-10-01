namespace ArcWms.Tests;

public class InventoryKeyTypeTest
{
    [Fact]
    public void CreateTest()
    {
        InventoryKeyType keyType = InventoryKeyType.Create<InventoryKey>();
        Assert.Same(typeof(InventoryKey), (Type)keyType);
    }

    [Fact]
    public void CreateInstanceTest()
    {

        InventoryKeyType keyType = InventoryKeyType.Create<InventoryKey>();
        Material material = new Material();
        var key = keyType.CreateInstance(material, "PCS");

        Assert.Same(material, key.Material);
        Assert.Equal("PCS", key.Uom);
    }

}

