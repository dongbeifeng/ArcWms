using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace ArcWms;

/// <summary>
/// 单深位巷道分配货位帮助类。
/// </summary>
public sealed class SAllocationHelper
{
    readonly IEnumerable<IRule> _rules;
    readonly ILogger<SAllocationHelper> _logger;

    public SAllocationHelper(IEnumerable<IRule> rules, ILogger<SAllocationHelper> logger)
    {
        _rules = rules;
        _logger = logger;
    }


    /// <summary>
    /// 在指定的巷道和分组中分配一个货位以供入库。
    /// </summary>
    /// <param name="streetlet">要在其中分配货位的巷道。</param>
    /// <param name="excludedIdList">要排除的货位。</param>
    /// <param name="excludedColumnList">要排除的列。</param>
    /// <param name="excludedLevelList">要排除的层。</param>
    /// <param name="storageInfo">入库的货物信息。</param>
    /// <param name="orderBy">排序依据。这是 LocationUnit 类的属性名。</param>
    /// <returns>
    /// 从不返回 null。
    /// </returns>
    public async Task<Location?> AllocateAsync(
        Streetlet streetlet,
        StorageInfo storageInfo,
        int[]? excludedIdList = null,
        int[]? excludedColumnList = null,
        int[]? excludedLevelList = null,
        string orderBy = "i1")
    {
        ArgumentNullException.ThrowIfNull(streetlet);
        Guard.IsNotNullOrWhiteSpace(orderBy, nameof(orderBy));

        _logger.LogDebug("巷道 {streetletCode}", streetlet.StreetletCode);

        var rules = _rules.Where(x => x.CanApplyTo(streetlet)).OrderBy(x => x.Order);
        foreach (var rule in rules)
        {
            _logger.LogDebug("正在测试 {ruleName}", rule.Name);
            var loc = await rule.SelectAsync(streetlet, storageInfo, excludedIdList, excludedColumnList, excludedLevelList, orderBy).ConfigureAwait(false);
            if (loc != null)
            {
                _logger.LogDebug("{ruleName} 成功分配到货位 {locationCode}", rule.Name, loc.LocationCode);
                return loc;
            }
            else
            {
                _logger.LogDebug("{ruleName} 失败", rule.Name);
            }
        }

        return null;
    }
}


