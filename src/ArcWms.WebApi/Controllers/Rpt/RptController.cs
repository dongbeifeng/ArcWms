using ArcWms.WebApi.MetaData;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using NHibernate.Transform;
using NHibernateAspNetCoreFilters;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;
using System.Collections;
using static ArcWms.WebApi.Controllers.DashboardData;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 提供报表 api
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RptController : ControllerBase
{
    readonly ISession _session;
    readonly ILogger<RptController> _logger;

    /// <summary>
    /// 初始化新实例。
    /// </summary>
    /// <param name="session"></param>
    /// <param name="logger"></param>
    public RptController(ISession session, ILogger<RptController> logger)
    {
        _session = session;
        _logger = logger;
    }

    /// <summary>
    /// 库存汇总
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [HttpPost("get-inventory-report")]
    [Transaction]
    [OperationType(OperationTypes.库存汇总)]
    public async Task<ListData<InventoryReprotItemInfo>> GetInventoryReport(InventoryReportArgs args)
    {
        var list = await _session.Query<UnitloadItem>()
            .Filter(args)
            .GroupBy(x => new
            {
                x.Material.MaterialCode,
                x.Material.Description,
                x.Batch,
                x.InventoryStatus,
                x.Uom,
            })
            .Select(gp => new InventoryReprotItemInfo
            {
                MaterialCode = gp.Key.MaterialCode,
                Description = gp.Key.Description,
                Batch = gp.Key.Batch,
                InventoryStatus = gp.Key.InventoryStatus,
                Uom = gp.Key.Uom,
                Quantity = gp.Sum(x => x.Quantity)
            })
            .ToListAsync()
            .ConfigureAwait(false);
        return this.ListData(new PagedList<InventoryReprotItemInfo>(list, 1, list.Count, list.Count));
    }


    /// <summary>
    /// 库龄报表
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-age-report")]
    [Transaction]
    [OperationType(OperationTypes.库龄报表)]
    public async Task<ListData<AgeReportItemInfo>> GetAgeReport()
    {
        List<AgeReportItemInfo> list = await _session.Query<UnitloadItem>()
            .Select(x => new AgeReportItemInfo
            {
                MaterialCode = x.Material.MaterialCode,
                Description = x.Material.Description,
                Specification = x.Material.Specification,
                Batch = x.Batch,
                InventoryStatus = x.InventoryStatus,
                Uom = x.Uom,
            })
            .Distinct()
            .ToListAsync();

        var q1 = _session.Query<UnitloadItem>().Where(x => x.AgeBaseline >= DateTime.Now.AddDays(-7));
        await Fill(list, q1, nameof(AgeReportItemInfo.ZeroToSevenDays));

        var q2 = _session.Query<UnitloadItem>().Where(x => x.AgeBaseline >= DateTime.Now.AddDays(-30) && x.AgeBaseline < DateTime.Now.AddDays(-7));
        await Fill(list, q2, nameof(AgeReportItemInfo.SevenToThirtyDays));

        var q3 = _session.Query<UnitloadItem>().Where(x => x.AgeBaseline >= DateTime.Now.AddDays(-90) && x.AgeBaseline < DateTime.Now.AddDays(-30));
        await Fill(list, q3, nameof(AgeReportItemInfo.ThirtyToNinetyDays));

        var q4 = _session.Query<UnitloadItem>().Where(x => x.AgeBaseline < DateTime.Now.AddDays(-90));
        await Fill(list, q4, nameof(AgeReportItemInfo.MoreThanNinetyDays));

        return new ListData<AgeReportItemInfo>
        {
            Success = true,
            Data = list,
            Total = list.Count,
        };

        static async Task Fill(List<AgeReportItemInfo> list, IQueryable<UnitloadItem> q, string time)
        {
            var arr = await q.GroupBy(x => new
            {
                x.Material.MaterialCode,
                x.Material.Description,
                x.Material.Specification,
                x.Batch,
                x.InventoryStatus,
                x.Uom,
            })
            .Select(gp => new
            {
                gp.Key.MaterialCode,
                gp.Key.Description,
                gp.Key.Specification,
                gp.Key.Batch,
                gp.Key.InventoryStatus,
                gp.Key.Uom,
                Quantity = gp.Sum(x => x.Quantity),
            })
            .ToListAsync()
            .ConfigureAwait(false);

            foreach (var item in arr)
            {
                var age = list.Single(x =>
                    x.MaterialCode == item.MaterialCode
                    && x.Description == item.Description
                    && x.Specification == item.Specification
                    && x.Batch == item.Batch
                    && x.InventoryStatus == item.InventoryStatus
                    && x.Uom == item.Uom
                    );
                switch (time)
                {
                    case nameof(AgeReportItemInfo.ZeroToSevenDays):
                        age.ZeroToSevenDays = item.Quantity;
                        break;
                    case nameof(AgeReportItemInfo.SevenToThirtyDays):
                        age.SevenToThirtyDays = item.Quantity;
                        break;
                    case nameof(AgeReportItemInfo.ThirtyToNinetyDays):
                        age.ThirtyToNinetyDays = item.Quantity;
                        break;
                    case nameof(AgeReportItemInfo.MoreThanNinetyDays):
                        age.MoreThanNinetyDays = item.Quantity;
                        break;
                }
            }
        }


        const string ageCase = @"
CASE 
    WHEN DATEDIFF(MINUTE, s.AgeBaseline, GETDATE()) / 1440.0 < 7 THEN '7'
    WHEN DATEDIFF(MINUTE, s.AgeBaseline, GETDATE()) / 1440.0 < 30 THEN '30'
    WHEN DATEDIFF(MINUTE, s.AgeBaseline, GETDATE()) / 1440.0 < 90 THEN '90'
    ELSE '90+'
END";
        string hql = @$"
SELECT 
    m.MaterialCode AS MaterialCode, 
    m.Description AS Description, 
    m.Specification AS Specification, 
    s.Batch AS Batch, 
    s.StockStatus AS StockStatus, 
    s.Uom AS Uom, 
    SUM(s.Quantity) AS Quantity, 
    {ageCase} AS Age
FROM Stock s
JOIN s.Material m
GROUP BY m.MaterialCode, m.Description, m.Specification, s.Batch, s.StockStatus, s.Uom, {ageCase}    
";

        var rows = await _session.CreateQuery(hql).SetResultTransformer(Transformers.AliasToEntityMap).ListAsync<Hashtable>();
        List<AgeReportItemInfo> ages = new List<AgeReportItemInfo>();
        foreach (var row in rows)
        {
            var item = new
            {
                MaterialCode = Convert.ToString(row["MaterialCode"]),
                Description = Convert.ToString(row["Description"]),
                Specification = Convert.ToString(row["Specification"]),
                Batch = Convert.ToString(row["Batch"]),
                StockStatus = Convert.ToString(row["StockStatus"]),
                Uom = Convert.ToString(row["Uom"]),
                Quantity = Convert.ToDecimal(row["Quantity"]),
                Age = Convert.ToString(row["Age"]),
            };
            var age = ages.SingleOrDefault(x =>
                x.MaterialCode == item.MaterialCode
                && x.Batch == item.Batch
                && x.InventoryStatus == item.StockStatus
                && x.Uom == item.Uom
            );

            if (age == null)
            {
                age = new AgeReportItemInfo
                {
                    MaterialCode = item.MaterialCode!,
                    Description = item.Description!,
                    Specification = item.Specification!,
                    Batch = item.Batch!,
                    InventoryStatus = item.StockStatus!,
                    Uom = item.Uom!,
                };
                ages.Add(age);
            }

            switch (item.Age)
            {
                case "7":
                    age.ZeroToSevenDays = item.Quantity;
                    break;
                case "30":
                    age.SevenToThirtyDays = item.Quantity;
                    break;
                case "90":
                    age.ThirtyToNinetyDays = item.Quantity;
                    break;
                case "90+":
                    age.MoreThanNinetyDays = item.Quantity;
                    break;
                default:
                    break;
            }
        }

        return new ListData<AgeReportItemInfo>
        {
            Success = true,
            Data = ages,
            Total = ages.Count,
        };
    }

    // TODO 使用真实数据替换假数据
    // TODO 使用缓存提升性能
    /// <summary>
    /// 获取仪表盘数据
    /// </summary>
    /// <returns></returns>
    [Transaction]
    [HttpPost("get-dashboard-data")]
    public async Task<ApiData<DashboardData>> GetDashboardData()
    {
        Random random = new Random();
        var streetlets = await _session.Query<Streetlet>().ToListAsync().ConfigureAwait(false);

        DashboardData dashboardData = new DashboardData
        {
            Location = new LocationData
            {
                StreetletCount = streetlets.Count,
                AvailableLocationCount = streetlets.Sum(x => x.GetAvailableLocationCount()),
                DisabledLocationCount = streetlets.SelectMany(x => x.Usage.Values).Sum(x => x.InboundDisabled),
                LocationCount = streetlets.Sum(x => x.GetTotalLocationCount()),
                LocationUsageRate = streetlets.SelectMany(x => x.Usage.Values).Sum(x => x.Loaded) / streetlets.Sum(x => x.GetTotalLocationCount()),
                LocationUsageRate7 = Enumerable.Range(-7, 7)
                    .Select(x => new DateValuePair<double>()
                    {
                        Date = DateTime.Now.Date.AddDays(x),
                        Value = random.NextDouble()
                    })
                    .ToList(),
            },
            InboundOrder = new InboundOrderData
            {
                InboundOrderCount7 = Enumerable.Range(-7, 7)
                    .Select(x => new DateValuePair<int>
                    {
                        Date = DateTime.Now.Date.AddDays(x),
                        Value = random.Next(0, 100)
                    })
                    .ToList(),
                OpenInboundOrderCount = await _session.Query<InboundOrder>().Where(x => x.Closed == false).CountAsync().ConfigureAwait(false),
            },
            OutboundOrder = new OutboundOrderData
            {
                OpenOutboundOrderCount = await _session.Query<OutboundOrder>().Where(x => x.Closed == false).CountAsync().ConfigureAwait(false),
                OutboundOrderCount7 = Enumerable.Range(-7, 7)
                    .Select(x => new DateValuePair<int>
                    {
                        Date = DateTime.Now.Date.AddDays(x),
                        Value = random.Next(0, 100)
                    })
                    .ToList(),
            },
            Stock = new StockData
            {
                MaterialCount = 1,
                UnitloadCount = await _session.Query<Unitload>().CountAsync().ConfigureAwait(false),
                EmptyPalletCount = await _session.Query<Unitload>().Where(x => x.Items.Any() == false).CountAsync().ConfigureAwait(false),
                FlowInCount7 = Enumerable.Range(-7, 7)
                    .Select(x => new DateValuePair<int>
                    {
                        Date = DateTime.Now.Date.AddDays(x),
                        Value = random.Next(0, 3000)
                    })
                    .ToList(),
                FlowOutCount7 = Enumerable.Range(-7, 7)
                    .Select(x => new DateValuePair<int>
                    {
                        Date = DateTime.Now.Date.AddDays(x),
                        Value = random.Next(0, 3000)
                    })
                    .ToList(),
            },
            Task = new TaskData
            {
                TaskCount = await _session.Query<TransportTask>().CountAsync().ConfigureAwait(false),
                TaskCount7 = Enumerable.Range(-7, 7)
                    .Select(x => new DateValuePair<int>
                    {
                        Date = DateTime.Now.Date.AddDays(x),
                        Value = random.Next(1000, 4000)
                    }).ToList(),
            },
        };

        return this.Success(dashboardData);


        /*

        DashboardData dashboardData = new DashboardData();
        dashboardData.Location.StreetletCount = await _session.Query<Streetlet>().CountAsync();
        dashboardData.Location.LocationCount = await _session.Query<Location>()
            .Where(x => x.LocationType == LocationTypes.S
                && x.Exists)
            .CountAsync();
        dashboardData.Location.AvailableLocationCount = await _session.Query<Location>()
            .Where(x => x.LocationType == LocationTypes.S
                && x.Exists
                && x.IsInboundDisabled == false)
            .CountAsync();
        dashboardData.Location.DisabledLocationCount = await _session.Query<Location>()
            .Where(x => x.LocationType == LocationTypes.S
                && x.Exists
                && (x.IsInboundDisabled || x.IsOutboundDisabled))
            .CountAsync();



         */
    }

}

