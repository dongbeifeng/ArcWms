namespace ArcWms.Tests;

public class DefaultInventoryKeyTest
{
    [Fact]
    public void TestEquals()
    {
        InventoryKey k1 = new InventoryKey(new Material(), "PCS");
        InventoryKey k2 = new InventoryKey(k1.Material, "PCS");
        InventoryKey k3 = new InventoryKey(new Material(), "PCS");

        Assert.True(k1.Equals(k2));
        Assert.True(k1 == k2);
        Assert.False(k1.Equals(k3));
    }

    [Fact]
    public void TestGetHashCode()
    {
        InventoryKey k1 = new InventoryKey(new Material(), "PCS");
        InventoryKey k2 = new InventoryKey(k1.Material, "PCS");
        InventoryKey k3 = new InventoryKey(new Material(), "PCS");

        Assert.Equal(k1.GetHashCode(), k2.GetHashCode());
        Assert.NotEqual(k1.GetHashCode(), k3.GetHashCode());
    }
}

