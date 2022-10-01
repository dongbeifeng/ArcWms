using NHibernateUtils;
using System.Linq.Expressions;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 历史任务列表的查询参数。
/// </summary>
public class ArchivedTaskListArgs
{
    /// <summary>
    /// 任务号
    /// </summary>
    [SearchArg]
    public string? TaskCode { get; set; }

    /// <summary>
    /// 托盘号
    /// </summary>
    [SearchArg("Unitload.PalletCode")]
    public string? PalletCode { get; set; }

    /// <summary>
    /// 任务类型
    /// </summary>
    [SearchArg]
    public string? TaskType { get; set; }

    /// <summary>
    /// 起点编号
    /// </summary>
    [SearchArg("Start.LocationCode")]
    public string? StartLocationCode { get; set; }

    /// <summary>
    /// 终点编号
    /// </summary>
    [SearchArg("End.LocationCode")]
    public string? EndLocationCode { get; set; }

    /// <summary>
    /// 起点或者终点编号
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public string? AnyLocationCode { get; set; }

    /// <summary>
    /// 任务是否取消
    /// </summary>
    [SearchArg]
    public bool? Canncelled { get; set; }

    /// <summary>
    /// AnyLocationCode 的查询条件
    /// </summary>
    internal Expression<Func<ArchivedTransportTask, bool>>? AnyLocationCodeExpr
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(this.AnyLocationCode))
            {
                return x => x.Start.LocationCode.Like(AnyLocationCode)
                    || x.End.LocationCode.Like(this.AnyLocationCode);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 物料类型
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public string? MaterialType { get; set; }

    /// <summary>
    /// MaterialType 的查询条件
    /// </summary>
    internal Expression<Func<ArchivedTransportTask, bool>>? MaterialTypeExpr
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(MaterialType))
            {
                return (x => x.Unitload.Items
                    .Select(c => c.Material)
                    .Any(m => m.MaterialType!.Like(this.MaterialType))
                    );
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 物料编号
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// MaterialCode 的查询条件
    /// </summary>
    internal Expression<Func<ArchivedTransportTask, bool>>? MaterialCodeExpr
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(MaterialCode))
            {
                return (x => x.Unitload.Items
                    .Select(c => c.Material)
                    .Any(m => m.MaterialCode.Like(this.MaterialCode))
                    );
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 批号
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public string? Batch { get; set; }

    /// <summary>
    /// Batch 查询条件
    /// </summary>
    internal Expression<Func<ArchivedTransportTask, bool>>? BatchExpr
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Batch))
            {
                return (x => x.Unitload.Items
                    .Any(m => m.Batch.Like(this.Batch))
                    );
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 巷道Id列表
    /// </summary>
    [SearchArg(SearchMode.Expression)]
    public int[]? StreetletIdList { get; set; }

    /// <summary>
    /// StreetletIdList 的查询条件
    /// </summary>
    internal Expression<Func<ArchivedTransportTask, bool>>? StreetletIdListExpr
    {
        get
        {
            if (StreetletIdList != null && StreetletIdList.Length > 0)
            {
                return (x =>
                    this.StreetletIdList.Contains(x.Start.Streetlet.StreetletId)
                    || this.StreetletIdList.Contains(x.End.Streetlet.StreetletId)
                    );
            }
            else
            {
                return null;
            }
        }
    }


    /// <summary>
    /// 下发时间范围。
    /// </summary>
    public DateTime?[]? SendTime { get; set; }


    /// <summary>
    /// 最小下发时间。
    /// </summary>
    [SearchArg("SendTime", SearchMode.GreaterThanOrEqual)]    
    public DateTime? SendTimeFrom
    {
        get
        {
            if (SendTime is not null && SendTime.Length == 2)
            {
                return SendTime[0];
            }
            return null;
        }
    }

    /// <summary>
    /// 最大下发时间。
    /// </summary>
    [SearchArg("SendTime", SearchMode.LessThan)]
    public DateTime? SendTimeTo
    {
        get
        {
            if (SendTime is not null && SendTime.Length == 2 && SendTime[1] is DateTime t)
            {
                return t.Date.AddDays(1);
            }
            return null;
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

