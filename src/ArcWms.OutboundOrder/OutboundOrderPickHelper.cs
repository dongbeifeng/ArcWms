using Microsoft.Extensions.Logging;
using NHibernate;

namespace ArcWms;

public sealed class OutboundOrderPickHelper
{
    ILogger<OutboundOrderPickHelper> _logger;
    ISession _session;
    InventoryKeyType _inventoryKeyType;

    public OutboundOrderPickHelper(InventoryKeyType inventoryKeyType, ISession session, ILogger<OutboundOrderPickHelper> logger)
    {
        _session = session;
        _inventoryKeyType = inventoryKeyType;
        _logger = logger;
    }

    /// <summary>
    /// 从货载中拣货。
    /// </summary>
    /// <param name="unitload"></param>
    /// <param name="pickInfo">拣货信息。</param>
    /// <returns>被拣选操作扣数的库存键，null 表示未发生扣数操作。</returns>
    public async Task<InventoryKey?> PickAsync(Unitload unitload, OutboundOrder outboundOrder, OutboundOrderPickInfo pickInfo, bool allowExcess = false)
    {
        if (unitload == null)
        {
            throw new ArgumentNullException(nameof(unitload));
        }

        if (pickInfo == null)
        {
            throw new ArgumentNullException(nameof(pickInfo));
        }

        _logger.LogDebug("托盘：{palletCode}", unitload);
        _logger.LogDebug("拣选信息：{pickInfo}", pickInfo);
        _logger.LogDebug("出库单：{outboundOrder}", outboundOrder);

        if (pickInfo.QuantityPicked < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pickInfo), "拣货数量不能小于 0。");
        }

        if (unitload.HasTask)
        {
            string msg = string.Format("拣货失败。托盘有任务，不允许拣货。");
            throw new InvalidOperationException(msg);
        }

        if (unitload.CurrentLocation?.LocationType == LocationTypes.S)
        {
            string msg = string.Format("拣货失败。当货载在储位上时，不允许拣货。");
            throw new InvalidOperationException(msg);
        }

        if ((unitload.CurrentUat as OutboundOrder) == null)
        {
            string msg = string.Format("拣货失败。货载未分配给出库单。");
            throw new InvalidOperationException(msg);
        }

        if (outboundOrder.Closed)
        {
            string msg = string.Format("拣货失败。出库单已关闭。");
            throw new InvalidOperationException(msg);
        }

        // 检查拣货信息合法性：
        // 1，范围，每个拣货信息都在当前货载中有对应的分配信息，不能拣货涉及两个货载；
        // 2，数量，每个拣货信息的实际拣货数量都不能超过分配数量；

        var allocInfo = unitload.Items
            .SelectMany(x => x.Allocations)
            .SingleOrDefault(x => x.UnitloadItemAllocationId == pickInfo.UnitloadItemAllocationId);

        if (allocInfo == null)
        {
            string msg = string.Format("未在货载中找到分配信息#{0}。", pickInfo.UnitloadItemAllocationId);
            throw new InvalidOperationException(msg);
        }

        _logger.LogDebug("分配信息：{allocInfo}", allocInfo);

        // 检查分配的一致性
        OutboundLine outboundLine = (OutboundLine)allocInfo.OutboundDemand!;
        if (outboundOrder.Lines.Contains(outboundLine) == false)
        {
            string msg = string.Format("库存项分配到的出库明细不在货载分配到的出库单中。分配信息#{0}。", pickInfo.UnitloadItemAllocationId);
            throw new InvalidOperationException(msg);
        }

        // 实际拣货数量不允许超出分配数量
        if (allocInfo.QuantityAllocated < pickInfo.QuantityPicked)
        {
            if (allowExcess)
            {
                _logger.LogWarning("实际拣货数量超出分配数量");
            }
            else
            {
                string msg = string.Format("实际拣货数量超出分配数量，分配信息#{0}。", pickInfo.UnitloadItemAllocationId);
                throw new InvalidOperationException(msg);
            }
        }

        // 到这里检查结束

        // 从货载中清除此分配信息
        UnitloadItem item = allocInfo.UnitloadItem ?? throw new Exception();
        outboundLine.Deallocate(allocInfo);
        if (unitload.Items.SelectMany(x => x.Allocations).Any() == false)
        {
            unitload.ResetCurrentUat();
        }

        // 若拣货为 0，则将货载清除分配信息后立即返回
        if (pickInfo.QuantityPicked == 0)
        {
            return null;
        }


        // 以下是拣货不为 0 的情况

        // 1，更新出库行的已出数量
        // 2，更新库存项的数量
        // 3，生成流水
        // 扣除拣货数量
        item.Quantity -= pickInfo.QuantityPicked;
        if (item.Quantity < 0)
        {
            throw new Exception("程序错误。");
        }
        if (item.Quantity == 0)
        {
            _logger.LogInformation("拣货后 {item} 数量为 0，从 {unitload} 中移除", item, unitload);
            unitload.RemoveItem(item);

            if (unitload.Items.Count() == 0)
            {
                _logger.LogInformation("{unitload} 不再有库存", unitload);
            }
        }

        // 增加出库行的已出数量。
        if (outboundLine.QuantityFulfilled + pickInfo.QuantityPicked > outboundLine.QuantityDemanded)
        {
            _logger.LogWarning("超过出库行的需求数量");
            // TODO 根据实际情况检查是否超出分配数量
            bool allowOverDelivery = false;
            if (!allowOverDelivery)
            {
                throw new ApplicationException("不允许超发。");
            }
        }

        outboundLine.QuantityFulfilled += pickInfo.QuantityPicked;
        _logger.LogDebug("{outboundLine} 已出数量增加到 {quantityFulfilled}。", outboundLine.OutboundLineId, outboundLine.QuantityFulfilled);

        // 并发处理
        await _session.LockAsync(outboundOrder, LockMode.Upgrade).ConfigureAwait(false);
        await _session.LockAsync(unitload, LockMode.Upgrade).ConfigureAwait(false);

        return item.GetInventoryKey(_inventoryKeyType);
    }



}
