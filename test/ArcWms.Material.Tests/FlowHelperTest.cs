using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernateUtils;

namespace ArcWms.Tests;

public class FlowHelperTest
{
    [Fact]
    public async Task GenerateFlowAsyncTest_DirectionNotSet()
    {
        var inventoryKey = new InventoryKey(new Material(), "PCS");
        Flow[] list = new Flow[]
        {
            new Flow { FlowId = 1, Material = inventoryKey.Material, Uom = inventoryKey.Uom, Balance = 10 },
            new Flow { FlowId = 2, Material = inventoryKey.Material, Uom = inventoryKey.Uom, Balance = 20 },
        };

        ISession session = For<ISession>();
        session.Query<Flow>().Returns(new TestingQueryable<Flow>(list));

        FlowHelper sut = new FlowHelper(session, () => new Flow(), For<ILogger<FlowHelper>>());
        await Assert.ThrowsAsync<ArgumentException>(() => sut.GenerateFlowAsync(inventoryKey, FlowDirection.NotSet, 5, new InboundBizType("采购入库"), "组盘", "PALLETCODE", "ORDERCODE", "BIZORDER", false));
    }    

    [Fact]
    public async Task GenerateFlowAsyncTest_Inbound()
    {
        var inventoryKey = new InventoryKey(new Material(), "PCS");
        Flow[] list = new Flow[]
        {
            new Flow { FlowId = 1, Material = inventoryKey.Material, Uom = inventoryKey.Uom, Balance = 10 },
            new Flow { FlowId = 2, Material = inventoryKey.Material, Uom = inventoryKey.Uom, Balance = 20 },
        };

        ISession session = For<ISession>();
        session.Query<Flow>().Returns(new TestingQueryable<Flow>(list));

        FlowHelper sut = new FlowHelper(session, () => new Flow(), For<ILogger<FlowHelper>>());
        var flow = await sut.GenerateFlowAsync(inventoryKey, FlowDirection.Inbound, 5, new InboundBizType("采购入库"), "组盘", "PALLETCODE", "ORDERCODE", "BIZORDER", false);

        Assert.Same(inventoryKey.Material, flow.Material);
        Assert.Equal(inventoryKey.Uom, flow.Uom);
        Assert.Equal(FlowDirection.Inbound, flow.Direction);
        Assert.Equal(5, flow.Quantity);
        Assert.Equal(25, flow.Balance);
        Assert.Equal("采购入库", flow.BizType);
        Assert.Equal("组盘", flow.OperationType);
        Assert.Equal("PALLETCODE", flow.PalletCode);
        Assert.Equal("ORDERCODE", flow.OrderCode);
        Assert.Equal("BIZORDER", flow.BizOrder);
    }

    [Fact]
    public async Task GenerateFlowAsyncTest_Outbound()
    {
        var inventoryKey = new InventoryKey(new Material(), "PCS");
        Flow[] list = new Flow[]
        {
            new Flow { FlowId = 1, Material = inventoryKey.Material, Uom = inventoryKey.Uom, Balance = 10 },
            new Flow { FlowId = 2, Material = inventoryKey.Material, Uom = inventoryKey.Uom, Balance = 20 },
        };

        ISession session = For<ISession>();
        session.Query<Flow>().Returns(new TestingQueryable<Flow>(list));

        FlowHelper sut = new FlowHelper(session, () => new Flow(), For<ILogger<FlowHelper>>());
        var flow = await sut.GenerateFlowAsync(inventoryKey, FlowDirection.Outbound, 5, new InboundBizType("销售出库"), "拣选", "PALLETCODE", "ORDERCODE", "BIZORDER", false);

        Assert.Same(inventoryKey.Material, flow.Material);
        Assert.Equal(inventoryKey.Uom, flow.Uom);
        Assert.Equal(FlowDirection.Outbound, flow.Direction);
        Assert.Equal(5, flow.Quantity);
        Assert.Equal(15, flow.Balance);
        Assert.Equal("销售出库", flow.BizType);
        Assert.Equal("拣选", flow.OperationType);
        Assert.Equal("PALLETCODE", flow.PalletCode);
        Assert.Equal("ORDERCODE", flow.OrderCode);
        Assert.Equal("BIZORDER", flow.BizOrder);
    }


    [Fact]
    public async Task GenerateFlowAsyncTest_NegativeBalance()
    {
        var inventoryKey = new InventoryKey(new Material(), "PCS");
        Flow[] list = new Flow[]
        {
            new Flow { FlowId = 1, Material = inventoryKey.Material, Uom = inventoryKey.Uom, Balance = 10 },
            new Flow { FlowId = 2, Material = inventoryKey.Material, Uom = inventoryKey.Uom, Balance = 20 },
        };

        ISession session = For<ISession>();
        session.Query<Flow>().Returns(new TestingQueryable<Flow>(list));

        FlowHelper sut = new FlowHelper(session, () => new Flow(), For<ILogger<FlowHelper>>());
        var task = sut.GenerateFlowAsync(inventoryKey, FlowDirection.Outbound, 25, new InboundBizType("销售出库"), "拣选", "PALLETCODE", "ORDERCODE", "BIZORDER", false);
        await Assert.ThrowsAsync<NegativeBalanceException>(() => task);
    }

}
