using Microsoft.Extensions.Logging;
using NHibernate;
using Xunit.Abstractions;

namespace ArcWms.Tests;

public class DefaultOutboundOrderAllocatorTest
{

    public DefaultOutboundOrderAllocatorTest(ITestOutputHelper output)
    {
    }


    [Fact]
    public async Task AllocateItemAsync_尾托分配数量在需求数量以内()
    {
        var session = For<ISession>();
        ILogger<DefaultOutboundOrderAllocator> logger = For<ILogger<DefaultOutboundOrderAllocator>>();
        DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

        Material material = new Material();
        OutboundOrder outboundOrder = new OutboundOrder();
        outboundOrder.OutboundOrderCode = "OBO-1";
        outboundOrder.BizType = "出库";
        outboundOrder.AddLine(new OutboundLine
        {
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 100,
            QuantityFulfilled = 0,
        });
        UnitloadHelper unitloadHelper = new UnitloadHelper(session, () => new UnitloadSnapshot(), () => new UnitloadItemSnapshot(), For<ILogger<UnitloadHelper>>());

        Unitload p1 = new Unitload();
        p1.AddItem(new UnitloadItem
        {
            Material = material,
            Batch = "B",
            InventoryStatus = "合格",
            Uom = "PCS",
            Quantity = 60,
        });

        Unitload p2 = new Unitload();
        p2.AddItem(new UnitloadItem
        {
            Material = material,
            Batch = "B",
            InventoryStatus = "合格",
            Uom = "PCS",
            Quantity = 70,
        });

        await allocator.AllocateItemAsync(outboundOrder.Lines.First(), p1.Items.Single(), null);
        await allocator.AllocateItemAsync(outboundOrder.Lines.First(), p2.Items.Single(), null);

        Assert.Single(p1.Items.Single().Allocations);
        Assert.Same(outboundOrder, p1.CurrentUat);
        Assert.Equal(60, p1.Items.Single().Allocations.Single().QuantityAllocated);

        Assert.Single(p2.Items.Single().Allocations);
        Assert.Same(outboundOrder, p2.CurrentUat);
        Assert.Equal(40, p2.Items.Single().Allocations.Single().QuantityAllocated);

        Assert.Equal(2, outboundOrder.Lines.First().Allocations.Count);
    }


    [Fact]
    public async Task AllocateItemAsync_物料不匹配时不分配()
    {
        var session = For<ISession>();
        ILogger<DefaultOutboundOrderAllocator> logger = For<ILogger<DefaultOutboundOrderAllocator>>();
        DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

        OutboundOrder outboundOrder = new OutboundOrder();
        outboundOrder.AddLine(new OutboundLine
        {
            Material = new Material(),
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 100,
            QuantityFulfilled = 0,
        });

        Unitload p1 = new Unitload();
        p1.AddItem(new UnitloadItem
        {
            Material = new Material(),
            Batch = "B",
            InventoryStatus = "合格",
            Uom = "PCS",
            Quantity = 60,
        });

        await allocator.AllocateItemAsync(outboundOrder.Lines.First(), p1.Items.First(), null);

        Assert.Empty(p1.Items.First().Allocations);
        Assert.Null(p1.CurrentUat);
        Assert.Empty(outboundOrder.Lines.First().Allocations);

    }


    [Fact]
    public async Task AllocateItemAsync_库存状态不匹配时不分配()
    {
        var session = For<ISession>();
        ILogger<DefaultOutboundOrderAllocator> logger = For<ILogger<DefaultOutboundOrderAllocator>>();
        DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

        Material material = new Material();
        OutboundOrder outboundOrder = new OutboundOrder();
        OutboundLine line1 = new OutboundLine
        {
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 100,
            QuantityFulfilled = 0,
        };
        outboundOrder.AddLine(line1);

        Unitload p1 = new Unitload();
        p1.PalletCode = "P1";
        p1.ResetCurrentUat();
        UnitloadItem i1 = new UnitloadItem();
        i1.Material = material;
        i1.Batch = "B";
        i1.InventoryStatus = "不合格";
        i1.Uom = "PCS";
        i1.Quantity = 60;
        p1.AddItem(i1);
        await allocator.AllocateItemAsync(line1, i1, null);

        Assert.Empty(i1.Allocations);
        Assert.Null(p1.CurrentUat);
        Assert.Empty(line1.Allocations);
    }

    [Fact]
    public async Task AllocateItemAsync_计量单位不匹配时不分配()
    {
        var session = For<ISession>();
        ILogger<DefaultOutboundOrderAllocator> logger = For<ILogger<DefaultOutboundOrderAllocator>>();
        DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

        Material material = new Material();
        OutboundOrder outboundOrder = new OutboundOrder();
        OutboundLine line1 = new OutboundLine
        {
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 100,
            QuantityFulfilled = 0,
        };
        outboundOrder.AddLine(line1);

        Unitload p1 = new Unitload();
        p1.PalletCode = "P1";
        p1.ResetCurrentUat();
        UnitloadItem i1 = new UnitloadItem();
        i1.Material = material;
        i1.Batch = "B";
        i1.InventoryStatus = "合格";
        i1.Uom = "米";
        i1.Quantity = 60;
        p1.AddItem(i1);

        await allocator.AllocateItemAsync(line1, i1, null);

        Assert.Empty(i1.Allocations);
        Assert.Null(p1.CurrentUat);
        Assert.Empty(line1.Allocations);
    }


    [Fact]
    public async Task AllocateItemAsync_批号不匹配时不分配()
    {
        var session = For<ISession>();
        ILogger<DefaultOutboundOrderAllocator> logger = For<ILogger<DefaultOutboundOrderAllocator>>();
        DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

        Material material = new Material();
        OutboundOrder outboundOrder = new OutboundOrder();
        OutboundLine line1 = new OutboundLine
        {
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            Batch = "L",
            QuantityDemanded = 100,
            QuantityFulfilled = 0,
        };
        outboundOrder.AddLine(line1);

        Unitload p1 = new Unitload();
        p1.PalletCode = "P1";
        p1.ResetCurrentUat();
        UnitloadItem i1 = new UnitloadItem();
        i1.Material = material;
        i1.Batch = "B";
        i1.InventoryStatus = "合格";
        i1.Uom = "PCS";
        i1.Quantity = 60;
        p1.AddItem(i1);

        await allocator.AllocateItemAsync(line1, i1, null);

        Assert.Empty(i1.Allocations);
        Assert.Null(p1.CurrentUat);
        Assert.Empty(line1.Allocations);
    }

    [Fact]
    public async Task AllocateItemAsync_能够分配到一个出库单下的两个明细()
    {
        var session = For<ISession>();
        ILogger<DefaultOutboundOrderAllocator> logger = For<ILogger<DefaultOutboundOrderAllocator>>();
        DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

        Material material = new Material();
        OutboundOrder outboundOrder = new OutboundOrder();
        OutboundLine line1 = new OutboundLine
        {
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 45,
            QuantityFulfilled = 0,
        };
        outboundOrder.AddLine(line1);

        OutboundLine line2 = new OutboundLine
        {
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 55,
            QuantityFulfilled = 0,
        };
        outboundOrder.AddLine(line2);


        Unitload p1 = new Unitload();
        p1.PalletCode = "P1";
        p1.ResetCurrentUat();
        UnitloadItem i1 = new UnitloadItem();
        i1.Material = material;
        i1.Batch = "B";
        i1.InventoryStatus = "合格";
        i1.Uom = "PCS";
        i1.Quantity = 60;
        p1.AddItem(i1);

        await allocator.AllocateItemAsync(line1, i1, null);
        await allocator.AllocateItemAsync(line2, i1, null);

        Assert.Equal(2, i1.Allocations.Count);
        Assert.Same(outboundOrder, p1.CurrentUat);
        Assert.Equal(45, i1.Allocations.First().QuantityAllocated);
        Assert.Equal(15, i1.Allocations.Last().QuantityAllocated);
        Assert.Equal(1, line1.Allocations.Count);
        Assert.Equal(1, line2.Allocations.Count);

    }

    [Fact]
    public async Task AllocateItemAsync_不可以分配到多个出库单()
    {
        var session = For<ISession>();
        ILogger<DefaultOutboundOrderAllocator> logger = For<ILogger<DefaultOutboundOrderAllocator>>();
        DefaultOutboundOrderAllocator allocator = new DefaultOutboundOrderAllocator(session, logger);

        Material material = new Material();
        OutboundOrder outboundOrder = new OutboundOrder();
        OutboundLine line1 = new OutboundLine
        {
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 45,
            QuantityFulfilled = 0,
        };
        outboundOrder.AddLine(line1);

        OutboundOrder outboundOrder2 = new OutboundOrder();
        OutboundLine line2 = new OutboundLine
        {
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 55,
            QuantityFulfilled = 0,
        };
        outboundOrder2.AddLine(line2);

        Unitload p1 = new Unitload();
        p1.PalletCode = "P1";
        p1.ResetCurrentUat();
        UnitloadItem i1 = new UnitloadItem();
        i1.Material = material;
        i1.Batch = "B";
        i1.InventoryStatus = "合格";
        i1.Uom = "PCS";
        i1.Quantity = 60;
        p1.AddItem(i1);

        await allocator.AllocateItemAsync(line1, i1, null);
        await allocator.AllocateItemAsync(line2, i1, null);

        Assert.Single(i1.Allocations);
        Assert.Same(outboundOrder, p1.CurrentUat);
        Assert.Equal(45, i1.Allocations.Single().QuantityAllocated);
        Assert.Equal(1, line1.Allocations.Count);
        Assert.Empty(line2.Allocations);

    }

}


