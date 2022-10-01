using NHibernateUtils;
using System.Linq.Expressions;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 储位列表的查询参数
/// </summary>
public class StorageLocationListArgs
{
    /// <summary>
    /// 货位类型，始终是 <see cref="LocationTypes.S"/>。
    /// </summary>
    [SearchArg(SearchMode.Equal)]
    public string LocationType { get; } = LocationTypes.S;

    /// <summary>
    /// 货位是否存在，始终是 true。
    /// </summary>
    [SearchArg]
    public bool Exists { get; } = true;


    /// <summary>
    /// 货位编码。
    /// </summary>
    [SearchArg]
    public string? LocationCode { get; set; }

    /// <summary>
    /// 货位所在巷道。
    /// </summary>
    [SearchArg("Streetlet.StreetletId", SearchMode.In)]
    public int[]? StreetletIdList { get; set; }

    /// <summary>
    /// 货位是否有货。
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public bool? Loaded { get; set; }

    internal Expression<Func<Location, bool>>? LoadedExpr
    {
        get
        {
            return Loaded switch
            {
                true => x => x.UnitloadCount > 0,
                false => x => x.UnitloadCount == 0,
                null => null,
            };
        }
    }


    /// <summary>
    /// 存储分组。
    /// </summary>
    [SearchArg]
    public string? StorageGroup { get; set; }

    /// <summary>
    /// 是否禁止入站
    /// </summary>
    [SearchArg]
    public bool? IsInboundDisabled { get; set; }

    /// <summary>
    /// 是否禁止出站
    /// </summary>
    [SearchArg]
    public bool? IsOutboundDisabled { get; set; }

    /// <summary>
    /// 是否有入站任务
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public bool? HasInboundMoves { get; set; }

    internal Expression<Func<Location, bool>>? HasInboundMovesExpr
    {
        get
        {
            return HasInboundMoves switch
            {
                true => x => x.InboundCount > 0,
                false => x => x.InboundCount == 0,
                null => null,
            };
        }
    }

    /// <summary>
    /// 是否有出站任务
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public bool? HasOutboundMoves { get; set; }

    internal Expression<Func<Location, bool>>? HasOutboundMovesExpr
    {
        get
        {
            return HasOutboundMoves switch
            {
                true => x => x.OutboundCount > 0,
                false => x => x.OutboundCount == 0,
                null => null,
            };
        }
    }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// 基于 1 的当前页面，默认值为 1。
    /// </summary>
    public int? Current { get; set; } = 1;

    /// <summary>
    /// 每页大小，默认值为 10。
    /// </summary>
    public int? PageSize { get; set; }

}

