using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using NHibernate;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using NHibernate.Linq;

namespace ArcWms;


/// <summary>
/// 此规则适用于单叉双深巷道。
/// </summary>
public class SSRule : IRule
{
    readonly ISession _session;
    readonly ILogger<SSRule> _logger;

    public SSRule(ISession session, ILogger<SSRule> logger)
    {
        _session = session;
        _logger = logger;
    }



    public bool CanApplyTo(Streetlet streetlet)
    {
        return !streetlet.IsDoubleDeep;
    }



    public string Name => "SSRule";

    public int Order => 100;


    /// <summary>
    /// 在指定的巷道和分组中分配一个货位以供入库。
    /// </summary>
    /// <param name="streetlet">要在其中分配货位的巷道。</param>
    /// <param name="excludedIdList">要排除的货位。</param>
    /// <param name="excludedColumnList">要排除的列。</param>
    /// <param name="excludedLevelList">要排除的层。</param>
    /// <param name="storageInfo">入库货物信息。</param>
    /// <param name="orderBy">排序依据。这是 LocationUnit 类的属性名。</param>
    /// <returns></returns>
    public async Task<Location?> SelectAsync(Streetlet streetlet, StorageInfo storageInfo, int[]? excludedIdList, int[]? excludedColumnList, int[]? excludedLevelList, string orderBy)
    {
        ArgumentNullException.ThrowIfNull(streetlet);
        Guard.IsNotNullOrWhiteSpace(orderBy, nameof(orderBy));

        if (streetlet.IsDoubleDeep)
        {
            string msg = $"此规则适用于单深巷道，但传入的巷道是双深。【{streetlet.StreetletCode}】。";
            throw new InvalidOperationException(msg);
        }

        if (streetlet.IsInboundDisabled)
        {
            throw new InvalidOperationException($"巷道 {streetlet.StreetletCode} 已禁入。");
        }

        IQueryable<Location> q = _session.Query<Location>();

        var predicate = BuildPredicate(streetlet, storageInfo, excludedIdList, excludedColumnList, excludedLevelList);
        return await q
            .Where(predicate)
            .OrderBy(loc => loc.WeightLimit)
            .ThenBy(loc => loc.HeightLimit)
            .ThenBy($"Cell.{orderBy}")
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }


    public static List<(Expression<Func<Location, bool>> Expression, string Description)> GetCondition(Streetlet streetlet, StorageInfo storageInfo, int[]? excludedIdList, int[]? excludedColumnList, int[]? excludedLevelList)
    {
        return new()
        {
            (loc => loc.Cell != null, "应属于单元格"),
            (loc => loc.Streetlet != null && loc.Streetlet == streetlet, "应在指定的巷道中"),
            (loc => loc.Exists, "应存在"),
            (loc => loc.UnitloadCount == 0, "应无货"),
            (loc => loc.OutboundCount == 0, "应无出站"),
            (loc => loc.InboundCount == 0, "应无入站"),
            (loc => loc.IsInboundDisabled == false, "不应禁止入站"),
            (loc => loc.Streetlet != null && loc.Streetlet.IsInboundDisabled == false, "巷道不应禁止入站"),
            (loc => loc.WeightLimit >= storageInfo.Weight, "限重不应小于货物重量"),
            (loc => loc.HeightLimit >= storageInfo.Height, "限高不应小于货物高度"),
            (loc => loc.StorageGroup == storageInfo.StorageGroup, "分组应与货物匹配"),
            (loc => loc.Specification == storageInfo.PalletSpecification, "规格应与货物匹配"),
            (excludedIdList == null
                ? loc => true
                : loc => excludedIdList.Contains(loc.LocationId) == false,
                "Id不应在排除列表中"),
            (excludedColumnList == null
                ? loc => true
                : loc => excludedColumnList.Contains(loc.Bay) == false,
                "所在列不应在排除列表中"),
            (excludedLevelList == null
                ? loc => true
                : loc => excludedLevelList.Contains(loc.Level) == false,
                "所在层不应在排除列表中"),
        };
    }

    public bool Check(Location location, Streetlet streetlet, StorageInfo storageInfo, int[]? excludedIdList, int[]? excludedColumnList, int[]? excludedLevelList, out List<string> messages)
    {
        messages = new List<string>();
        bool succ = true;
        foreach (var predicate in GetCondition(streetlet, storageInfo, excludedIdList, excludedColumnList, excludedLevelList))
        {
            var func = predicate.Expression.Compile();
            if (func(location))
            {
                messages.Add($"（√）{predicate.Description}");
            }
            else
            {
                succ = false;
                messages.Add($"（×）{predicate.Description}");
            }
        }

        return succ;

    }

    public static Expression<Func<Location, bool>> BuildPredicate(Streetlet streetlet, StorageInfo storageInfo, int[]? excludedIdList, int[]? excludedColumnList, int[]? excludedLevelList)
    {
        var predicate = GetCondition(streetlet, storageInfo, excludedIdList, excludedColumnList, excludedLevelList)
            .Select(x => x.Expression)
            .Aggregate((left, right) =>
                // 参考：http://www.albahari.com/nutshell/predicatebuilder.aspx
                Expression.Lambda<Func<Location, bool>>
                (
                    Expression.AndAlso(left.Body, Expression.Invoke(right, left.Parameters)),
                    left.Parameters
                )
            );

        return predicate;
    }

}


