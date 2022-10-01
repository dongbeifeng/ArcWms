using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using NHibernateUtils;

namespace ArcWms;

/// <summary>
/// 出库单库存分配程序。
/// </summary>
public class DefaultOutboundOrderAllocator : IOutboundOrderAllocator
{
    protected readonly ILogger<DefaultOutboundOrderAllocator> _logger;

    protected readonly ISession _session;

    /// <summary>
    /// 初始化类 <see cref="Wms.OutboundOrderAllocator"/> 的一个实例
    /// </summary>
    public DefaultOutboundOrderAllocator(ISession session, ILogger<DefaultOutboundOrderAllocator> logger)
    {
        _session = session;
        _logger = logger;
    }

    /// <summary>
    /// 为出库单分配库存。
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="outboundOrder">要分配库存的出库单</param>
    /// <param name="options">分配选项</param>
    public async Task AllocateAsync(OutboundOrder outboundOrder, AllocateStockOptions? options)
    {
        if (outboundOrder == null)
        {
            throw new ArgumentNullException(nameof(outboundOrder));
        }
        if (outboundOrder.Closed)
        {
            throw new InvalidOperationException("出库单已关闭");
        }
        if (options == null)
        {
            options = new AllocateStockOptions();
        }
        options.Normalize();

        _logger.LogInformation("正在为出库单 {outboundOrderCode} 分配库存", outboundOrder.OutboundOrderCode);
        _logger.LogInformation("区域：{areas}", options.Areas == null ? "" : string.Join(", ", options.Areas));
        //_logger.LogInformation("跳过脱机的巷道：{skipOfflineStreetlets}", options.SkipOfflineStreetlets);
        //_logger.LogInformation("排除的巷道：{excludeLanewasys}", string.Join(", ", options.ExcludeStreetlets.Select(x => x.StreetletCode)));
        _logger.LogInformation("包含的托盘：{includePallets}", string.Join(", ", options.IncludePallets ?? new string[0]));
        _logger.LogInformation("排除的托盘：{excludePallets}", string.Join(", ", options.ExcludePallets ?? new string[0]));
        //_logger.LogInformation("跳过禁止出站的货位：{skipLocationsOutboundDisabled}", options.SkipLocationsOutboundDisabled);

        foreach (var line in outboundOrder.Lines)
        {
            await ProcessLineAsync(line, options).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 处理单个出库行
    /// </summary>
    /// <param name="line">出库行</param>
    /// <param name="streetlets">用于分配的货载所在的巷道</param>
    /// <param name="includeUnitloads">要在分配中包含的货载，这些货载优先参与分配。</param>
    /// <param name="excludeUnitloads">要在分配中排除的货载，这些货载不会参与分配，即使出现在 includeUnitloads 中，也不参与分配。</param>
    async Task ProcessLineAsync(OutboundLine line, AllocateStockOptions options)
    {
        _logger.LogInformation("正在为出库单明细 {outboundLine} 分配库存", line);

        {
            var shortage = line.ComputeShortage();
            _logger.LogDebug("出库单明细 {outboundLine} 的分配欠数是 {shortage}", line, shortage);
            if (shortage <= 0)
            {
                _logger.LogDebug("不需要分配");
                return;
            }
        }

        // 显式包含的货载项
        List<UnitloadItem> included = new List<UnitloadItem>();
        if (options.IncludePallets != null && options.IncludePallets.Length > 0)
        {
            included = await _session.Query<UnitloadItem>()
                .Where(x => options.IncludePallets.Contains(x.Unitload.PalletCode)
                    && x.Material == line.Material)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        // 库内候选项
        
        // 未实现：按二深、一深的排序
        throw new NotImplementedException();
        var candidateItems = _session
            .Query<UnitloadItem>()
            .Where(x =>
                x.Unitload.CurrentLocation!.LocationType == LocationTypes.S
                && included.Contains(x) == false   // 显式包含的货载项已在上面处理过，这里需排除
                && x.Material == line.Material)
            .OrderBy(x => x.Fifo)
            //.ThenBy(x => x.Unitload.CurrentLocation.Cell.Deep2Loaded)
            //.ThenBy(x => x.Unitload.CurrentLocation.Cell.Deep1Loaded)
            .ThenBy(x => x.Unitload.CurrentLocation.Cell.o1)
            .ThenBy(x => x.Unitload.CurrentLocation.Deep)
            .LoadInChunksAsync(options.ChunkSize);  // 和 nhQuery
        await foreach (var item in Concat(included, candidateItems).ConfigureAwait(false))
        {
            if (item.Quantity == 0)
            {
                _logger.LogWarning("货载项 {unitloadItem} 的数量为 0", item);
                continue;
            }

            var taken = await AllocateItemAsync(line, item, options).ConfigureAwait(false);
            var shortage = line.ComputeShortage();
            _logger.LogDebug("出库单明细 {outboundLine} 的分配欠数是 {shortage}", line, shortage);
            if (shortage <= 0)
            {
                _logger.LogInformation("出库单明细 {outboundLine} 分配库存完成", line);
                return;
            }
        }

        _logger.LogInformation("出库单明细 {outboundLine} 分配库存完成", line);

        static async IAsyncEnumerable<UnitloadItem> Concat(List<UnitloadItem> include, IAsyncEnumerable<UnitloadItem> candidateItems)
        {
            foreach (var item in include)
            {
                yield return item;
            }

            await foreach (var item in candidateItems.ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// 为出库行从指定的货载项进行分配。此方法具有副作用，会更改货载的分配信息
    /// </summary>
    /// <param name="line">出库行</param>
    /// <param name="item">要从中分配的货载项</param>
    /// <returns>从货载项中分配的数量</returns>
    internal async Task<decimal> AllocateItemAsync(OutboundLine line, UnitloadItem item, AllocateStockOptions? options)
    {
        if (line.OutboundOrder == null)
        {
            throw new Exception("出库单明细不属于任何出库单");
        }
        if (item.Unitload == null)
        {
            throw new Exception("货载明细不属于任何货载");
        }

        if (!TestUnitloadItem(line, item, options))
        {
            return 0m;
        }

        if (line.ComputeShortage() <= 0m)
        {
            return 0m;
        }

        // 货载项中的可用数（未分配数）
        var allocated = line.OutboundOrder.ComputeAllocated(item);
        var available = item.Quantity - allocated;
        _logger.LogDebug("货载项 {unitloadItem} 的库存数量是 {quantity}", item, item.Quantity);
        _logger.LogDebug("货载项 {unitloadItem} 的已分配数量是 {allocated}", item, allocated);
        _logger.LogDebug("货载项 {unitloadItem} 的可用数量是 {available}", item, available);
        if (available < 0)
        {
            throw new Exception("程序错误");
        }

        if (available == 0)
        {
            return 0m;
        }
        var taken = Math.Min(available, line.ComputeShortage());

        line.Allocate(item, taken);
        item.Unitload.SetCurrentUat(line.OutboundOrder);
        await _session.UpdateAsync(item.Unitload).ConfigureAwait(false);

        _logger.LogInformation("为出库单明细 {outboundLine} 从货载项 {unitloadItem} 中分配了 {quantity} {uom}", line, item, taken, item.Uom);

        return taken;
    }

    /// <summary>
    /// 测试货载项是否满足出库单明细的需求
    /// </summary>
    /// <remarks>
    /// 会动态调用使用 <see cref="TestUnitloadItemAttribute"/> 标记的方法。
    /// </remarks>
    /// <param name="line"></param>
    /// <param name="unitloadItem"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public bool TestUnitloadItem(OutboundLine line, UnitloadItem item, AllocateStockOptions? options)
    {
        if (item.Unitload == null)
        {
            throw new Exception("货载明细不属于任何货载");
        }
        bool passed = true;
        _logger.LogDebug("检查出库单明细 {outboundLine} 与货载项 {unitloadItem} 是否匹配", line, item);

        if (options == null)
        {
            options = new();
        }
        if (options.ExcludePallets != null && options.ExcludePallets.Contains(item.Unitload.PalletCode))
        {
            _logger.LogDebug("（×）已显式排除");
            passed = false;
        }
        else
        {
            _logger.LogDebug("（√）未显式排除");
        }

        if (line.Material == item.Material)
        {
            _logger.LogDebug("（√）物料匹配");
        }
        else
        {
            _logger.LogDebug("（×）物料不匹配");
            passed = false;
        }

        if (string.IsNullOrWhiteSpace(line.Batch) || line.Batch == item.Batch)
        {
            _logger.LogDebug("（√）批号匹配");
        }
        else
        {
            _logger.LogDebug("（×）批号不匹配");
            passed = false;
        }

        if (line.InventoryStatus == item.InventoryStatus)
        {
            _logger.LogDebug("（√）库存状态匹配");
        }
        else
        {
            _logger.LogDebug("（×）库存状态不匹配");
            passed = false;
        }

        if (line.Uom == item.Uom)
        {
            _logger.LogDebug("（√）计量单位匹配");
        }
        else
        {
            _logger.LogDebug("（×）计量单位不匹配");
            passed = false;
        }

        if (item.Unitload.CurrentUat == null || item.Unitload.CurrentUat == line.OutboundOrder)
        {
            _logger.LogDebug("（√）未分配到其他对象");
        }
        else
        {
            _logger.LogDebug("（×）已分配到其他对象");
            passed = false;
        }

        if (item.Unitload.HasCountingError == false)
        {
            _logger.LogDebug("（√）无盘点错误");
        }
        else
        {
            _logger.LogDebug("（×）有盘点错误");
            passed = false;
        }

        // TODO 优化：让路任务和整理任务允许分配库存
        //string[] taskTypes = new[] { "让路", "整理" };
        if (item.Unitload.HasTask == false)
        {
            _logger.LogDebug("（√）无任务");
        }
        else
        {
            _logger.LogDebug("（×）有任务");
            passed = false;
        }

        if (string.IsNullOrWhiteSpace(item.Unitload.OpHintType) && string.IsNullOrWhiteSpace(item.Unitload.OpHintInfo))
        {
            _logger.LogDebug("（√）无操作提示");
        }
        else
        {
            _logger.LogDebug("（×）有操作提示");
            passed = false;
        }

        if (InAreas(item.Unitload.CurrentLocation?.Streetlet?.Area, options?.Areas)
            || options?.IncludePallets?.Contains(item.Unitload.PalletCode, StringComparer.OrdinalIgnoreCase) == true)
        {
            _logger.LogDebug("（√）在指定区域、或显式包含");
        }
        else
        {
            _logger.LogDebug("（×）不在指定区域、且未显式包含");
            passed = false;
        }

        if (item.Unitload.CurrentLocation?.Streetlet?.IsOutboundDisabled == false
            || options?.SkipOfflineStreetlets != true
            || options?.IncludePallets?.Contains(item.Unitload.PalletCode, StringComparer.OrdinalIgnoreCase) == true)
        {
            _logger.LogDebug("（√）巷道未禁出、或允许从禁出的巷道分配、或显式包含");
        }
        else
        {
            _logger.LogDebug("（×）巷道已禁出");
            passed = false;
        }

        if (passed)
        {
            _logger.LogDebug("出库单明细 {outboundLine} 与货载项 {unitloadItem} 匹配", line, item);
        }
        else
        {
            _logger.LogDebug("出库单明细 {outboundLine} 与货载项 {unitloadItem} 不匹配", line, item);
        }
        return passed;

        static bool InAreas(string? area, string[]? areas)
        {
            return areas == null
                || areas == null
                || areas.Length == 0
                || areas.Where(x => x != null).Any(x => string.Equals(x, area, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// 解除出库单在货架上的分配，货架外的分配使用 <see cref="DeallocateAsync(OutboundOrder, Unitload)"/> 方法单独处理。
    /// </summary>
    /// <param name="outboundOrder">出库单</param>
    public async Task DeallocateInRackAsync(OutboundOrder outboundOrder)
    {
        if (outboundOrder == null)
        {
            throw new ArgumentNullException(nameof(outboundOrder));
        }

        if (outboundOrder.Closed)
        {
            throw new InvalidOperationException("出库单已关闭");
        }

        _logger.LogDebug("正在取消分配出库单 {outboundOrder} 在货架上的托盘", outboundOrder);
        var unitloadsInRack = _session.Query<Unitload>()
            .Where(x => x.CurrentUat == outboundOrder)
            .ToArray()
            .Where(x => x.IsInRack() && x.HasTask == false)
            .ToArray();
        foreach (var u in unitloadsInRack)
        {
            await DeallocateAsync(outboundOrder, u).ConfigureAwait(false);
        }
        await _session.FlushAsync().ConfigureAwait(false);
        _logger.LogInformation("已取消分配出库单 {outboundOrder} 在货架上的 {palletCount} 个托盘", outboundOrder, unitloadsInRack.Length);
    }

    /// <summary>
    /// 解除指定出库单在指定货载上分配信息。
    /// </summary>
    /// <param name="outboundOrder">出库单</param>
    /// <param name="unitload">货载</param>
    public async Task DeallocateAsync(OutboundOrder outboundOrder, Unitload unitload)
    {
        if (outboundOrder == null)
        {
            throw new ArgumentNullException(nameof(outboundOrder));
        }

        if (outboundOrder.Closed)
        {
            throw new InvalidOperationException("出库单已关闭。");
        }

        if (unitload == null)
        {
            throw new ArgumentNullException(nameof(unitload));
        }

        _logger.LogDebug("正在从出库单 {outboundOrder} 解除分配托盘 {palletCode}", outboundOrder, unitload.PalletCode);

        if (unitload.CurrentUat != outboundOrder)
        {
            string msg = string.Format("货载未分配给出库单");
            throw new InvalidOperationException(msg);
        }

        if (unitload.HasTask)
        {
            _logger.LogWarning("托盘 {palletCode} 有任务，应避免取消分配", unitload.PalletCode);
        }

        foreach (var line in outboundOrder.Lines)
        {
            foreach (var alloc in line.Allocations.Where(x => x.UnitloadItem?.Unitload == unitload).ToArray())
            {
                line.Deallocate(alloc);
            }
        }

        unitload.ResetCurrentUat();

        await _session.UpdateAsync(outboundOrder).ConfigureAwait(false);
        await _session.UpdateAsync(unitload).ConfigureAwait(false);

        _logger.LogDebug("成功从出库单 {outboundOrder} 解除分配托盘 {palletCode}", outboundOrder, unitload.PalletCode);
    }
}

