using Microsoft.Extensions.Logging;
using NHibernate;
using Xunit.Abstractions;

namespace ArcWms.Tests;

public class UnitloadHelperTest
{
    public UnitloadHelperTest(ITestOutputHelper output)
    {
    }


    [Theory]
    [InlineData("K")]
    [InlineData("S")]
    public async Task EnterAsync_非N位置(string locationType)
    {
        var session = For<ISession>();
        Location loc = new Location
        {
            LocationType = locationType,
        };
        Unitload u1 = new Unitload { UnitloadId = 1, PalletCode = "P1" };
        Unitload u2 = new Unitload { UnitloadId = 2, PalletCode = "P2" };
        UnitloadHelper sut = new UnitloadHelper(session, () => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger<UnitloadHelper>>());

        await sut.EnterAsync(u1, loc);
        await sut.EnterAsync(u2, loc);

        await session.Received().SaveAsync(Arg.Is<Keeping>(x => x.Unitload == u1));
        await session.Received().SaveAsync(Arg.Is<Keeping>(x => x.Unitload == u2));

        Assert.Same(loc, u1.CurrentLocation);
        Assert.Same(loc, u2.CurrentLocation);
        Assert.Equal(2, loc.UnitloadCount);
    }


    [Fact]
    public async Task EnterAsync_N位置()
    {
        var session = For<ISession>();
        Location loc = new Location
        {
            LocationType = LocationTypes.N,
        };
        Unitload u1 = new Unitload { UnitloadId = 1, PalletCode = "P1" };
        Unitload u2 = new Unitload { UnitloadId = 2, PalletCode = "P2" };
        UnitloadHelper sut = new UnitloadHelper(session, () => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger<UnitloadHelper>>());

        await sut.EnterAsync(u1, loc);
        await sut.EnterAsync(u2, loc);

        await session.DidNotReceive().SaveAsync(Arg.Is<Keeping>(x => x.Unitload == u1));
        await session.DidNotReceive().SaveAsync(Arg.Is<Keeping>(x => x.Unitload == u2));

        Assert.Same(loc, u1.CurrentLocation);
        Assert.Same(loc, u2.CurrentLocation);
        Assert.Equal(0, loc.UnitloadCount);

    }


    [Fact]
    public async Task EnterAsync_调用前CurrentLocation应为null()
    {
        var session = For<ISession>();
        Location loc1 = new Location();
        Location loc2 = new Location();
        Unitload u1 = new Unitload();
        UnitloadHelper sut = new UnitloadHelper(session, () => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger<UnitloadHelper>>());
        await sut.EnterAsync(u1, loc1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.EnterAsync(u1, loc2));
    }


    [Theory]
    [InlineData("K")]
    [InlineData("S")]
    public async Task LeaveCurrentLocationAsyncTest_非N位置(string locationType)
    {
        var session = For<ISession>();
        Location loc = new Location
        {
            LocationType = locationType,
        };
        Unitload u1 = new Unitload { UnitloadId = 1, PalletCode = "P1" };
        Unitload u2 = new Unitload { UnitloadId = 2, PalletCode = "P2" };
        session.GetAsync<Keeping>(1).Returns(new Keeping { Unitload = u1, Location = loc });
        session.GetAsync<Keeping>(2).Returns(new Keeping { Unitload = u2, Location = loc });
        UnitloadHelper sut = new UnitloadHelper(session, () => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger<UnitloadHelper>>());

        await sut.EnterAsync(u1, loc);
        await sut.EnterAsync(u2, loc);

        await sut.LeaveCurrentLocationAsync(u1);
        Assert.Null(u1.CurrentLocation);
        await session.Received().DeleteAsync(Arg.Is<Keeping>(x => x.Unitload == u1));
        Assert.Equal(1, loc.UnitloadCount);

        await sut.LeaveCurrentLocationAsync(u2);
        Assert.Null(u2.CurrentLocation);
        await session.Received().DeleteAsync(Arg.Is<Keeping>(x => x.Unitload == u2));
        Assert.Equal(0, loc.UnitloadCount);
    }



    [Fact]
    public async Task LeaveCurrentLocationAsync_N位置()
    {
        var session = For<ISession>();
        Location loc = new Location
        {
            LocationType = LocationTypes.N,
        };
        Unitload u1 = new Unitload { UnitloadId = 1, PalletCode = "P1" };
        Unitload u2 = new Unitload { UnitloadId = 2, PalletCode = "P2" };
        session.GetAsync<Keeping>(1).Returns(new Keeping { Unitload = u1, Location = loc });
        session.GetAsync<Keeping>(2).Returns(new Keeping { Unitload = u2, Location = loc });
        UnitloadHelper sut = new UnitloadHelper(session, () => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger<UnitloadHelper>>());

        await sut.EnterAsync(u1, loc);
        await sut.EnterAsync(u2, loc);

        await sut.LeaveCurrentLocationAsync(u1);
        Assert.Null(u1.CurrentLocation);
        Assert.Equal(0, loc.UnitloadCount);

        await sut.LeaveCurrentLocationAsync(u2);
        Assert.Null(u2.CurrentLocation);
        Assert.Equal(0, loc.UnitloadCount);
    }


    [Fact]
    public void GetSnapshotTest()
    {
        Unitload unitload = new()
        {
            UnitloadId = 1,
            PalletCode = "PalletCode",
            Comment = "Comment",
            HasCountingError = true
        };
        unitload.AddItem(new UnitloadItem
        {
            UnitloadItemId = 1,
            Batch = "B1",
            Material = new Material(),
            Fifo = "111",
            Quantity = 100,
            InventoryStatus = "合格",
            Uom = "PCS",
        });
        unitload.AddItem(new UnitloadItem
        {
            UnitloadItemId = 2,
            Batch = "B2",
            Material = new Material(),
            Fifo = "222",
            Quantity = 200,
            InventoryStatus = "不合格",
            Uom = "PCS",
        });


        UnitloadHelper unitloadHelper = new UnitloadHelper(For<ISession>(), () => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger<UnitloadHelper>>());

        UnitloadSnapshot snapshot = unitloadHelper.GetSnapshot(unitload);
        UnitloadItem item1 = unitload.Items.Single(x => x.UnitloadItemId == 1);
        UnitloadItem item2 = unitload.Items.Single(x => x.UnitloadItemId == 2);
        UnitloadItemSnapshot itemSnapshot1 = snapshot.Items.Single(x => x.Batch == item1.Batch);
        UnitloadItemSnapshot itemSnapshot2 = snapshot.Items.Single(x => x.Batch == item2.Batch);
        Assert.Equal(0, snapshot.UnitloadSnapshotId);
        Assert.Equal(unitload.PalletCode, snapshot.PalletCode);
        Assert.Equal(unitload.StorageInfo, snapshot.StorageInfo);
        Assert.Equal(unitload.Comment, snapshot.Comment);
        Assert.Equal(unitload.HasCountingError, snapshot.HasCountingError);
        Assert.Equal(unitload.HasCountingError, snapshot.HasCountingError);

        Assert.Equal(0, itemSnapshot1.UnitloadItemSnapshotId);
        Assert.Same(snapshot, itemSnapshot1.Unitload);
        Assert.Equal(item1.UnitloadItemId, item1.UnitloadItemId);
        Assert.Equal(item1.Batch, item1.Batch);
        Assert.Same(item1.Material, item1.Material);
        Assert.Equal(item1.Fifo, item1.Fifo);
        Assert.Equal(item1.Quantity, item1.Quantity);
        Assert.Equal(item1.InventoryStatus, item1.InventoryStatus);
        Assert.Equal(item1.Uom, item1.Uom);

        Assert.Equal(0, itemSnapshot2.UnitloadItemSnapshotId);
        Assert.Same(snapshot, itemSnapshot2.Unitload);
        Assert.Equal(item2.UnitloadItemId, item2.UnitloadItemId);
        Assert.Equal(item2.Batch, item2.Batch);
        Assert.Same(item2.Material, item2.Material);
        Assert.Equal(item2.Fifo, item2.Fifo);
        Assert.Equal(item2.Quantity, item2.Quantity);
        Assert.Equal(item2.InventoryStatus, item2.InventoryStatus);
        Assert.Equal(item2.Uom, item2.Uom);
    }


    class Src
    {
        public int A { get; set; }
        public string? B { get; set; }
        public string? C { get; set; }
        public string? D { get; set; }
        public string? E { get; set; }
    }


    class Dest
    {
        public int a { get; set; }
        public string? b { get; set; }
        public object? c { get; set; }
        public string? d { get; set; }

        internal string? e { get; set; }
    }

    [Fact]
    public void CopyPropertiesTest()
    {
        var src = new Src
        {
            A = 1,
            B = "2",
            C = "3",
            D = "4",
            E = "5",
        };
        var dest = new Dest();

        UnitloadHelper.CopyProperties(src, dest, new string[] { "D" });
        Assert.Equal(1, dest.a);
        Assert.Equal("2", dest.b);
        Assert.Equal("3", dest.c);
        Assert.Null(dest.d);
        Assert.Null(dest.e);
    }

}
