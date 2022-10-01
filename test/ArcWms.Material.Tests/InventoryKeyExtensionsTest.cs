namespace ArcWms.Tests;

public class InventoryKeyExtensionsTest
{
    private class Foo : IHasInventoryKey
    {
        public Material? Material { get; set; }

        public string? Batch { get; set; }

        public string? InventoryStatus { get; set; }

        public string? Uom { get; set; }

        public string? ExtraKeyProp { get; set; }

        public string? NormalProp { get; set; }
    }

    private record FooInventoryKey(Material Material, string Batch, string InventoryStatus, string Uom, string ExtraKeyProp) : InventoryKey(Material, Uom);


    [Fact]
    public void GetInventoryKeyTest()
    {
        Foo foo = new()
        {
            Material = new Material(),
            Batch = "1513",
            InventoryStatus = "合格",
            Uom = "PCS",
            ExtraKeyProp = "X",
            NormalProp = "300"
        };

        FooInventoryKey inventoryKey = (FooInventoryKey)foo.GetInventoryKey(InventoryKeyType.Create<FooInventoryKey>());

        Assert.Same(foo.Material, inventoryKey.Material);
        Assert.Equal(foo.Batch, inventoryKey.Batch);
        Assert.Equal(foo.InventoryStatus, inventoryKey.InventoryStatus);
        Assert.Equal(foo.Uom, inventoryKey.Uom);
        Assert.Equal(foo.ExtraKeyProp, inventoryKey.ExtraKeyProp);
        Assert.Equal(new FooInventoryKey(foo.Material, foo.Batch, foo.InventoryStatus, foo.Uom, foo.ExtraKeyProp), inventoryKey);
    }


    [Fact]
    public void SetInventoryKeyTest()
    {
        FooInventoryKey inventoryKey = new(new Material(), "1513", "合格", "PCS", "X");

        Foo foo = new();
        foo.SetInventoryKey(inventoryKey);

        Assert.Same(inventoryKey.Material, foo.Material);
        Assert.Equal(inventoryKey.Batch, foo.Batch);
        Assert.Equal(inventoryKey.InventoryStatus, foo.InventoryStatus);
        Assert.Equal(inventoryKey.Uom, foo.Uom);
        Assert.Equal(inventoryKey.ExtraKeyProp, foo.ExtraKeyProp);

        Assert.Null(foo.NormalProp);
    }


    [Fact]
    public void OfInventoryKeyTest()
    {
        Foo foo1 = new() { Material = new Material(), Batch = "1513", InventoryStatus = "合格", Uom = "PCS", ExtraKeyProp = "X", NormalProp = "300" };
        Foo foo2 = new() { Material = new Material(), Batch = "1514", InventoryStatus = "合格", Uom = "PCS", ExtraKeyProp = "X", NormalProp = "300" };
        Foo foo3 = new() { Material = new Material(), Batch = "1515", InventoryStatus = "合格", Uom = "PCS", ExtraKeyProp = "X", NormalProp = "300" };

        IQueryable<Foo> q = new List<Foo> { foo1, foo2, foo3 }.AsQueryable();
        FooInventoryKey inventoryKey = (FooInventoryKey)foo2.GetInventoryKey(InventoryKeyType.Create<FooInventoryKey>());

        var q2 = q.OfInventoryKey(inventoryKey);

        Assert.Equal(3, q.Count());
        Assert.Equal(1, q2.Count());
        Assert.Same(foo2, q2.Single());
    }


    [Fact]
    public void BuildWhereClauseTest()
    {
        var arr = new (string name, object? value)[]
        {
                ("A", 100),
                ("B", 200),
                ("C", 300),
        };

        string where = InventoryKeyExtensions.BuildWhereClause(arr);

        const string expected = @"A = @0 AND B = @1 AND C = @2";
        Assert.Equal(expected, where);
    }


    [Fact]
    public void TestInventoryKeyComponent()
    {
        (string name, object value) a = ("A", 100);
        (string name, object value) b = ("A", 100);
        (string name, object value) c = ("A", 200);
        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }
}
