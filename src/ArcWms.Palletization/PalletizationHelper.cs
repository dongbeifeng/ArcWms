using Autofac;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;

namespace ArcWms;

/// <summary>
/// 组盘帮助程序。
/// </summary>
/// <remarks>
/// 组盘是复杂操作，涉及项目上的具体业务逻辑，无法在框架的位置上对需求进行预测并实现，这里仅提供最基本的功能，
/// 如果货载和货载项上新增的字段，则需要重新实现组盘逻辑，因为这些字段无法在这里赋值。
/// </remarks>
public sealed class PalletizationHelper
{
    // TODO 依赖太多
    readonly ILogger<PalletizationHelper> _logger;
    readonly Func<Unitload> _createUnitload;
    readonly Func<UnitloadItem> _createUnitloadItem;
    readonly IUnitloadStorageInfoProvider _storageInfoProvider;
    readonly IPalletCodeValidator _palletCodeValidator;
    readonly ISession _session;
    readonly UnitloadHelper _unitloadHelper;

    public PalletizationHelper(ISession session,
        Func<Unitload> unitloadFactory,
        Func<UnitloadItem> unitloadItemFactory,
        IUnitloadStorageInfoProvider storageInfoProvider,
        IPalletCodeValidator palletCodeValidator,
        UnitloadHelper unitloadHelper,
        ILogger<PalletizationHelper> logger)
    {
        _logger = logger;
        _createUnitload = unitloadFactory;
        _createUnitloadItem = unitloadItemFactory;
        _storageInfoProvider = storageInfoProvider;
        _palletCodeValidator = palletCodeValidator;
        _session = session;
        _unitloadHelper = unitloadHelper;
    }

    public Task<bool> IsPalletCodeInUse(string palletCode)
    {
        ArgumentNullException.ThrowIfNull(palletCode);

        return _session.Query<Unitload>().AnyAsync(x => x.PalletCode == palletCode);
    }


    /// <summary>
    /// 创建新货载。
    /// </summary>
    /// <param name="palletCode"></param>
    /// <param name="items"></param>
    /// <param name="opType"></param>
    /// <param name="bizType"></param>
    /// <param name="orderCode"></param>
    /// <param name="bizOrder"></param>
    /// <returns></returns>
    public async Task<Unitload> PalletizeAsync(string palletCode, IEnumerable<PalletizationItemInfo> items)
    {
        ArgumentNullException.ThrowIfNull(palletCode);
        ArgumentNullException.ThrowIfNull(items);

        // 验证托盘编码是否合法
        if (_palletCodeValidator.IsValid(palletCode, out string err) == false)
        {
            throw new InvalidPalletCodeException(palletCode, err);
        }

        // 验证托盘编码是否已占用
        if (await IsPalletCodeInUse(palletCode))
        {
            throw new PalletCodeInUseException(palletCode);
        }

        _logger.LogDebug($"正在生成货载：{palletCode}", palletCode);

        // 生成货载
        Unitload unitload = _createUnitload.Invoke();

        unitload.PalletCode = palletCode;
        foreach (var item in items)
        {
            if (item.InventoryKey is null)
            {
                ThrowHelper.ThrowArgumentException(nameof(items), "InventoryKey 未赋值。");
            }

            UnitloadItem unitloadItem = _createUnitloadItem.Invoke();
            unitloadItem.SetInventoryKey(item.InventoryKey);
            unitloadItem.Quantity = item.Quantity;
            unitloadItem.Fifo = item.Fifo;
            unitloadItem.AgeBaseline = item.AgeBaseline;
            unitload.AddItem(unitloadItem);
        }

        var loc = await _session.Query<Location>()
            .Where(x => x.LocationCode == NLocationCode.Value)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
        await _unitloadHelper.EnterAsync(unitload, loc).ConfigureAwait(false);

        unitload.StorageInfo = new StorageInfo
        {
            StorageGroup = _storageInfoProvider.GetStorageGroup(unitload),
            OutFlag = _storageInfoProvider.GetOutFlag(unitload),
            PalletSpecification = _storageInfoProvider.GetPalletSpecification(unitload)
        };

        // 将货载保存到数据库
        await _session.SaveAsync(unitload).ConfigureAwait(false);

        _logger.LogInformation($"已生成货载：{palletCode}", palletCode);

        return unitload;
    }

}
