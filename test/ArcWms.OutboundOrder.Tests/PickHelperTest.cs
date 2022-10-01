using ArcWms;
using Microsoft.Extensions.Logging;
using NHibernate;
using Xunit.Abstractions;

namespace ArcWms.Tests;


public class PickHelperTest
{

    public PickHelperTest(ITestOutputHelper output)
    {
    }


    /// <summary>
    /// 需求 100，i1 分配 60/60，i2 分配 40/70
    /// </summary>
    private async Task<(ISession session, OutboundOrder outboundOrder, Unitload p1, Unitload p2)> PrepareData()
    {
        var session = For<ISession>();

        var allocator = new DefaultOutboundOrderAllocator(session, For<ILogger<DefaultOutboundOrderAllocator>>());

        Material material = new Material();

        var outboundOrder = new OutboundOrder();
        outboundOrder.OutboundOrderCode = "OBO-1";
        outboundOrder.BizType = "出库";
        outboundOrder.AddLine(new OutboundLine
        {
            OutboundLineId = 1,
            Material = material,
            InventoryStatus = "合格",
            Uom = "PCS",
            QuantityDemanded = 100,
            QuantityFulfilled = 0,
        });

        var p1 = new Unitload { UnitloadId = 1 };
        p1.AddItem(new UnitloadItem
        {
            UnitloadItemId = 1,
            Material = material,
            Batch = "B",
            InventoryStatus = "合格",
            Uom = "PCS",
            Quantity = 60,
        });

        var p2 = new Unitload { UnitloadId = 2 };
        p2.ResetCurrentUat();
        p2.AddItem(new UnitloadItem
        {
            UnitloadItemId = 2,
            Material = material,
            Batch = "B",
            InventoryStatus = "合格",
            Uom = "PCS",
            Quantity = 70,
        });

        await allocator.AllocateItemAsync(outboundOrder.Lines.First(), p1.Items.Single(), null);
        await allocator.AllocateItemAsync(outboundOrder.Lines.First(), p2.Items.Single(), null);

        p1.Items.Single().Allocations.Single().UnitloadItemAllocationId = 1;
        p2.Items.Single().Allocations.Single().UnitloadItemAllocationId = 2;

        return (session, outboundOrder, p1, p2);
    }


    [Fact]
    public async Task PickAsync_会从货载项扣数并删除分配信息()
    {
        (ISession session, OutboundOrder outboundOrder, Unitload p1, Unitload p2) = await PrepareData();
        OutboundOrderPickHelper outboundOrderPickHelper = new OutboundOrderPickHelper(
            InventoryKeyType.Create<InventoryKey>(),
            session,
            For<ILogger<OutboundOrderPickHelper>>()
            );

        await outboundOrderPickHelper.PickAsync(
            p1,
            outboundOrder,
            new OutboundOrderPickInfo { QuantityPicked = 60, UnitloadItemAllocationId = 1 }
            );
        await outboundOrderPickHelper.PickAsync(
            p2,
            outboundOrder,
            new OutboundOrderPickInfo { QuantityPicked = 20, UnitloadItemAllocationId = 2 }
            );

        Assert.Empty(p1.Items);
        Assert.Empty(p2.Items.Single().Allocations);
        Assert.Equal(50, p2.Items.Single().Quantity);
    }

    [Fact]
    public async Task PickAsync_会更新出库明细的实出数量()
    {
        (ISession session, OutboundOrder outboundOrder, Unitload p1, Unitload p2) = await PrepareData();

        OutboundOrderPickHelper outboundOrderPickHelper = new OutboundOrderPickHelper(
            InventoryKeyType.Create<InventoryKey>(),
            session,
            For<ILogger<OutboundOrderPickHelper>>()
            );

        await outboundOrderPickHelper.PickAsync(
            p1,
            outboundOrder,
            new OutboundOrderPickInfo { QuantityPicked = 60, UnitloadItemAllocationId = 1 }
            );
        Assert.Equal(60, outboundOrder.Lines.Single().QuantityFulfilled);
        Assert.Single(outboundOrder.Lines.Single().Allocations);

        await outboundOrderPickHelper.PickAsync(
            p2,
            outboundOrder,
            new OutboundOrderPickInfo { QuantityPicked = 20, UnitloadItemAllocationId = 2 }
            );
        Assert.Equal(80, outboundOrder.Lines.Single().QuantityFulfilled);
        Assert.Empty(outboundOrder.Lines.Single().Allocations);

    }

    [Fact]
    public async Task PickAsync_不会删除空货载()
    {
        (ISession session, OutboundOrder outboundOrder, Unitload p1, Unitload p2) = await PrepareData();
        OutboundOrderPickHelper outboundOrderPickHelper = new OutboundOrderPickHelper(
            InventoryKeyType.Create<InventoryKey>(),
            session,
            For<ILogger<OutboundOrderPickHelper>>()
            );

        await outboundOrderPickHelper.PickAsync(
            p1,
            outboundOrder,
            new OutboundOrderPickInfo { QuantityPicked = 60, UnitloadItemAllocationId = 1 }
            );
        session.Flush();


        Assert.Empty(p1.Items);
        Assert.Null(p1.CurrentUat);
        await session.DidNotReceive().DeleteAsync(p1);
        session.DidNotReceive().Delete(p1);

    }

}


