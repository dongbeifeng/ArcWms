using ArcWms;
using ArcWms.WebApi.MetaData;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using NHibernateAspNetCoreFilters;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 提供位置 api
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class LocController : ControllerBase
{
    readonly ISession _session;
    readonly LocationHelper _locHelper;
    readonly Func<Location> _locFactory;
    readonly ILogger<LocController> _logger;

    /// <summary>
    /// 初始化新实例。
    /// </summary>
    /// <param name="session"></param>
    /// <param name="locHelper"></param>
    /// <param name="locFactory"></param>
    /// <param name="logger"></param>
    public LocController(ISession session, LocationHelper locHelper, Func<Location> locFactory, ILogger<LocController> logger)
    {
        _session = session;
        _locHelper = locHelper;
        _locFactory = locFactory;
        _logger = logger;
    }

    /// <summary>
    /// 巷道列表
    /// </summary>
    /// <param name="args">查询参数。</param>
    /// <returns></returns>
    [HttpPost("get-streetlet-list")]
    [Transaction]
    [OperationType(OperationTypes.查看巷道)]
    public async Task<ListData<StreetletInfo>> GetStreetletList(StreetletListArgs args)
    {
        var pagedList = await _session.Query<Streetlet>().SearchAsync(args, args.Sort, args.Current, args.PageSize).ConfigureAwait(false);
        return this.ListData(pagedList, x => new StreetletInfo
        {
            StreetletId = x.StreetletId,
            StreetletCode = x.StreetletCode,
            IsDoubleDeep = x.IsDoubleDeep,
            IsInboundDisabled = x.IsInboundDisabled,
            InboundDisabledComment = x.InboundDisabledComment,
            IsOutboundDisabled = x.IsOutboundDisabled,
            OutboundDisabledComment = x.OutboundDisabledComment,

            TotalLocationCount = x.GetTotalLocationCount(),
            AvailableLocationCount = x.GetAvailableLocationCount(),
            ReservedLocationCount = x.ReservedLocationCount,
            UsageRate = (x.GetTotalLocationCount() - x.GetAvailableLocationCount()) / (float)x.GetTotalLocationCount(),
            UsageInfos = x.Usage.Select(x => new StreetletUsageInfo
            {
                StorageGroup = x.Key.StorageGroup,
                WeightLimit = x.Key.WeightLimit,
                HeightLimit = x.Key.HeightLimit,
                Specification = x.Key.Specification,
                Total = x.Value.Total,
                Loaded = x.Value.Loaded,
                InboundDisabled = x.Value.InboundDisabled,
                Available = x.Value.Available,
            }).ToArray(),
            Outlets = x.Outlets
                .Select(x => new OutletInfo
                {
                    OutletId = x.OutletId,
                    OutletCode = x.OutletCode,
                    CurrentUat = x.CurrentUat?.ToString()
                })
                .ToArray(),
        });
    }



    /// <summary>
    /// 禁止巷道入站。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("disable-streetlet-inbound")]
    [OperationType(OperationTypes.禁止巷道入站)]
    [Transaction]
    public async Task<ApiData> DisableStreetletInbound(EnableStreetletArgs args)
    {
        Streetlet streetlet = await _session
            .GetAsync<Streetlet>(args.StreetletId)
            .ConfigureAwait(false);

        if (streetlet.IsInboundDisabled == false)
        {
            streetlet.IsInboundDisabled = true;
            streetlet.InboundDisabledComment = args.Comment;
            await _session.UpdateAsync(streetlet).ConfigureAwait(false);
        }

        _logger.LogInformation("已将巷道 {streetletCode} 禁止入站", streetlet.StreetletCode);
        _ = await this.SaveOpAsync($"巷道：{streetlet.StreetletCode}。备注：{args.Comment}").ConfigureAwait(false);

        return this.Success();
    }

    /// <summary>
    /// 允许巷道入站。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("enable-streetlet-inbound")]
    [OperationType(OperationTypes.允许巷道入站)]
    [Transaction]
    public async Task<ApiData> EnableStreetletInbound(EnableStreetletArgs args)
    {
        Streetlet streetlet = await _session
            .GetAsync<Streetlet>(args.StreetletId)
            .ConfigureAwait(false);

        if (streetlet.IsInboundDisabled)
        {
            streetlet.IsInboundDisabled = false;
            streetlet.InboundDisabledComment = args.Comment;
            await _session.UpdateAsync(streetlet).ConfigureAwait(false);
        }

        _logger.LogInformation("已将巷道 {streetletCode} 允许入站", streetlet.StreetletCode);
        _ = await this.SaveOpAsync($"巷道：{streetlet.StreetletCode}。备注：{args.Comment}").ConfigureAwait(false);

        return this.Success();

    }

    /// <summary>
    /// 禁止巷道出站
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("disable-streetlet-outbound")]
    [OperationType(OperationTypes.禁止巷道出站)]
    [Transaction]
    public async Task<ApiData> DisableStreetletOutbound(EnableStreetletArgs args)
    {
        Streetlet streetlet = await _session
            .GetAsync<Streetlet>(args.StreetletId)
            .ConfigureAwait(false);

        if (streetlet.IsOutboundDisabled == false)
        {
            streetlet.IsOutboundDisabled = true;
            streetlet.OutboundDisabledComment = args.Comment;
            await _session.UpdateAsync(streetlet).ConfigureAwait(false);
        }

        _logger.LogInformation("已将巷道 {streetletCode} 禁止出站", streetlet.StreetletCode);
        _ = await this.SaveOpAsync($"巷道：{streetlet.StreetletCode}。备注：{args.Comment}").ConfigureAwait(false);

        return this.Success();

    }

    /// <summary>
    /// 允许巷道出站。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("enable-streetlet-outbound")]
    [OperationType(OperationTypes.允许巷道出站)]
    [Transaction]
    public async Task<ApiData> EnableStreetletOutbound(EnableStreetletArgs args)
    {
        Streetlet streetlet = await _session
            .GetAsync<Streetlet>(args.StreetletId)
            .ConfigureAwait(false);

        if (streetlet.IsOutboundDisabled)
        {
            streetlet.IsOutboundDisabled = false;
            streetlet.OutboundDisabledComment = args.Comment;
            await _session.UpdateAsync(streetlet).ConfigureAwait(false);
        }
        _logger.LogInformation("已将巷道 {streetletCode} 允许出站", streetlet.StreetletCode);
        _ = await this.SaveOpAsync($"巷道：{streetlet.StreetletCode}。备注：{args.Comment}").ConfigureAwait(false);

        return this.Success();

    }



    /// <summary>
    /// 获取巷道的选项列表
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-streetlet-options")]
    [Transaction]
    public async Task<OptionsData<StreetletInfo>> GetStreetletOptions()
    {
        var items = await _session.Query<Streetlet>()
            .Select(x => new StreetletInfo
            {
                StreetletId = x.StreetletId,
                StreetletCode = x.StreetletCode,
                IsInboundDisabled = x.IsInboundDisabled,
                IsOutboundDisabled = x.IsOutboundDisabled,
            })
            .ToListAsync().ConfigureAwait(false);
        return this.OptionsData(items);
    }


    /// <summary>
    /// 设置巷道可以到达的出口。
    /// </summary>
    /// <param name="id">巷道Id</param>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("set-outlets")]
    [OperationType(OperationTypes.设置出口)]
    [Transaction]
    public async Task<ApiData> SetOutlets(SetOutletsArgs args)
    {
        Streetlet streetlet = await _session.GetAsync<Streetlet>(args.StreetletId).ConfigureAwait(false);
        if (streetlet == null)
        {
            throw new InvalidOperationException($"巷道不存在。");
        }

        streetlet.Outlets.Clear();
        foreach (var outletId in args.OutletIdList)
        {
            Outlet outlet = await _session.GetAsync<Outlet>(outletId).ConfigureAwait(false);
            streetlet.Outlets.Add(outlet);
        }

        var op = await this.SaveOpAsync("巷道【{0}】，{1} 个出口", streetlet.StreetletCode, streetlet.Outlets.Count).ConfigureAwait(false);
        _logger.LogInformation("设置出口成功，{streetletCode} --> {outlets}", streetlet.StreetletCode, string.Join(",", streetlet.Outlets.Select(x => x.OutletCode)));

        return this.Success();
    }

    /// <summary>
    /// 清除出口的单据
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("clear-outlet")]
    [OperationType(OperationTypes.清除出口单据)]
    [Transaction]
    public async Task<ApiData> ClearOutlet(ClearOutletArgs args)
    {
        _logger.LogDebug("正在清除 {outletId} 上的单据", args.OutletId);
        Outlet? outlet = await _session.GetAsync<Outlet>(args.OutletId).ConfigureAwait(false);
        if (outlet != null)
        {
            if (outlet.CurrentUat != null)
            {
                object uat = outlet.CurrentUat;
                outlet.ResetCurrentUat();
                _logger.LogInformation("已清除 {outletCode} 上的 {uat}", outlet.OutletCode, uat);
                return this.Success();
            }
            else
            {
                _logger.LogWarning("{outletCode} 上没有单据", outlet.OutletCode);
                return this.Failure("出口上没有单据");
            }
        }
        else
        {
            _logger.LogWarning("{outletId} 不存在", args.OutletId);
            return this.Failure("出口不存在");
        }
    }

    /// <summary>
    /// 获取巷道侧视图数据。
    /// </summary>
    /// <param name="args">查询参数。</param>
    /// <returns></returns>
    [HttpPost("get-side-view")]
    [OperationType(OperationTypes.侧视图)]
    [Transaction]
    public async Task<ApiData<SideViewData>> GetSideViewData(GetSideViewDataArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.StreetletCode);

        Streetlet? streetlet = await _session.Query<Streetlet>().SingleOrDefaultAsync(x => x.StreetletCode == args.StreetletCode).ConfigureAwait(false);
        if (streetlet == null)
        {
            return this.Failure<SideViewData>($"巷道 {args.StreetletCode} 不存在。");
        }

        var sideViewData = new SideViewData
        {
            StreetletCode = streetlet.StreetletCode,
            IsInboundDisabled = streetlet.IsInboundDisabled,
            InboundDisabledComment = streetlet.InboundDisabledComment,
            IsOutboundDisabled = streetlet.IsOutboundDisabled,
            OutboundDisabledComment = streetlet.OutboundDisabledComment,
            AvailableCount = streetlet.Locations
                    .Where(x =>
                        x.Exists
                        && x.UnitloadCount == 0
                        && x.InboundCount == 0
                        && x.IsInboundDisabled == false)
                    .Count(),
            LocationCount = streetlet.Locations
                    .Where(x => x.Exists)
                    .Count(),
            Locations = streetlet.Locations.Select(loc => new SideViewLocation
            {
                LocationId = loc.LocationId,
                LocationCode = loc.LocationCode,
                IsLoaded = loc.UnitloadCount > 0,
                Side = loc.Side,
                Deep = loc.Deep,
                Level = loc.Level,
                Bay = loc.Bay,
                IsInboundDisabled = loc.IsInboundDisabled,
                InboundDisabledComment = loc.InboundDisabledComment,
                InboundCount = loc.InboundCount,
                InboundLimit = loc.InboundLimit,
                IsOutboundDisabled = loc.IsOutboundDisabled,
                OutboundDisabledComment = loc.OutboundDisabledComment,
                OutboundLimit = loc.OutboundLimit,
                OutboundCount = loc.OutboundCount,
                Specification = loc.Specification,
                StorageGroup = loc.StorageGroup,
                WeightLimit = loc.WeightLimit,
                HeightLimit = loc.HeightLimit,
                Exists = loc.Exists,
                i1 = loc.Cell?.i1 ?? default,
                o1 = loc.Cell?.o1 ?? default,
                i2 = loc.Cell?.i2 ?? default,
                o2 = loc.Cell?.o2 ?? default,
                i3 = loc.Cell?.i3 ?? default,
                o3 = loc.Cell?.o3 ?? default,
            }).ToList()
        };

        return this.Success(sideViewData);
    }

    /// <summary>
    /// 重建所有巷道的统计信息，这个操作消耗资源较多
    /// </summary>
    /// <returns></returns>
    [HttpPost("rebuild-stats")]
    [OperationType(OperationTypes.重建巷道统计信息)]
    [Transaction]
    public async Task<ApiData> RebuildStreetletsStat()
    {
        var streetlets = await _session.Query<Streetlet>().ToListAsync().ConfigureAwait(false);
        foreach (var streetlet in streetlets)
        {
            await _locHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);
        }
        return this.Success();
    }

    /// <summary>
    /// 出口列表
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [HttpPost("get-outlet-list")]
    [Transaction]
    [OperationType(OperationTypes.查看出口)]
    public async Task<ListData<OutletInfo>> GetOutletList(OutletListArgs args)
    {
        var pagedList = await _session.Query<Outlet>().SearchAsync(args, args.Sort, args.Current, args.PageSize).ConfigureAwait(false);
        return this.ListData(pagedList, x => new OutletInfo
        {
            OutletId = x.OutletId,
            OutletCode = x.OutletCode,
            CurrentUat = x.CurrentUat?.ToString(),
            KP1 = x.KP1?.LocationCode,
            KP2 = x.KP2?.LocationCode,
            Streetlets = x.Streetlets.Select(x => x.StreetletCode).ToArray(),
            LastCheckTime = x.LastCheckTime,
            LastCheckMessage = x.LastCheckMessage,
        });
    }

    /// <summary>
    /// 获取出口的选项列表
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-outlet-options")]
    [Transaction]
    public async Task<OptionsData<OutletInfo>> GetOutletOptions()
    {
        var list = await _session.Query<Outlet>().ToListAsync().ConfigureAwait(false);
        var items = list
            .Select(x => new OutletInfo
            {
                OutletId = x.OutletId,
                OutletCode = x.OutletCode,
                CurrentUat = x.CurrentUat?.ToString(),
                KP1 = x.KP1?.LocationCode,
                KP2 = x.KP2?.LocationCode,
                Streetlets = x.Streetlets.Select(x => x.StreetletCode).ToArray(),
                LastCheckTime = x.LastCheckTime,
                LastCheckMessage = x.LastCheckMessage,
            })
            .ToList();
        return this.OptionsData(items);
    }

    /// <summary>
    /// 获取储位列表。
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [HttpPost("get-storage-location-list")]
    [Transaction]
    [OperationType(OperationTypes.查看位置)]
    public async Task<ListData<StorageLocationInfo>> GetStorageLocationList(StorageLocationListArgs args)
    {
        var pagedList = await _session.Query<Location>().SearchAsync(args, args.Sort, args.Current, args.PageSize).ConfigureAwait(false);
        return this.ListData(pagedList, x => new StorageLocationInfo
        {
            LocationId = x.LocationId,
            LocationCode = x.LocationCode,
            StreetletId = x.Streetlet!.StreetletId,
            StreetletCode = x.Streetlet.StreetletCode,
            WeightLimit = x.WeightLimit,
            HeightLimit = x.HeightLimit,
            InboundCount = x.InboundCount,
            IsInboundDisabled = x.IsInboundDisabled,
            InboundDisabledComment = x.InboundDisabledComment,
            OutboundCount = x.OutboundCount,
            IsOutboundDisabled = x.IsOutboundDisabled,
            OutboundDisabledComment = x.OutboundDisabledComment,
            StorageGroup = x.StorageGroup,
            UnitloadCount = x.UnitloadCount,
        });
    }

    /// <summary>
    /// 关键点列表
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [HttpPost("get-key-point-list")]
    [Transaction]
    [OperationType(OperationTypes.查看位置)]
    public async Task<ListData<KeyPointInfo>> GetKeyPointList(KeyPointListArgs args)
    {
        var pagedList = await _session.Query<Location>().SearchAsync(args, args.Sort, args.Current, args.PageSize).ConfigureAwait(false);
        return this.ListData(pagedList, x => new KeyPointInfo
        {
            LocationId = x.LocationId,
            LocationCode = x.LocationCode,
            InboundCount = x.InboundCount,
            IsInboundDisabled = x.IsInboundDisabled,
            InboundDisabledComment = x.InboundDisabledComment,
            InboundLimit = x.InboundLimit,
            OutboundCount = x.OutboundCount,
            IsOutboundDisabled = x.IsOutboundDisabled,
            OutboundDisabledComment = x.OutboundDisabledComment,
            OutboundLimit = x.OutboundLimit,
            Tag = x.Tag,
            RequestType = x.RequestType,
            UnitloadCount = x.UnitloadCount,
        });
    }


    /// <summary>
    /// 禁止位置入站。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("disable-location-inbound")]
    [OperationType(OperationTypes.禁止位置入站)]
    [Transaction]
    public async Task<ApiData> DisableLocationInbound(EnableLocationArgs args)
    {
        List<Location> locs = await _session.Query<Location>()
            .Where(x => args.LocationIds.Contains(x.LocationId))
            .ToListAsync()
            .ConfigureAwait(false);

        List<string> updated = new List<string>();
        foreach (var loc in locs)
        {
            if (loc.LocationType == LocationTypes.N)
            {
                // N 位置无法禁入禁出
                _logger.LogWarning("不能禁用或启用 N 位置");
                continue;
            }

            if (loc.Cell != null)
            {
                foreach (var item in loc.Cell.Locations)
                {
                    await DisableOneAsync(item).ConfigureAwait(false);
                }
            }
            else
            {
                await DisableOneAsync(loc).ConfigureAwait(false);
            }
        }

        var streetlets = locs
            .Where(x => x.Streetlet != null)
            .Select(x => x.Streetlet)
            .Distinct();
        foreach (var streetlet in streetlets)
        {
            if (streetlet != null)
            {
                await _locHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("已将 {updated} 个位置禁止入站", updated.Count);
        _ = await this.SaveOpAsync($"位置：{string.Join(", ", updated)}。备注：{args.Comment}").ConfigureAwait(false);

        return this.Success();

        async Task DisableOneAsync(Location loc)
        {
            if (loc.IsInboundDisabled == false)
            {
                loc.IsInboundDisabled = true;
                loc.InboundDisabledComment = args.Comment;
                await _session.UpdateAsync(loc).ConfigureAwait(false);

                updated.Add(loc.LocationCode);

                _logger.LogInformation("已将位置 {locationCode} 禁止入站", loc.LocationCode);
            }
        }
    }

    /// <summary>
    /// 允许位置入站。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("enable-location-inbound")]
    [OperationType(OperationTypes.允许位置入站)]
    [Transaction]
    public async Task<ApiData> EnableLocationInbound(EnableLocationArgs args)
    {
        List<Location> locs = await _session.Query<Location>()
            .Where(x => args.LocationIds.Contains(x.LocationId))
            .ToListAsync()
            .ConfigureAwait(false);

        List<string> updated = new List<string>();
        foreach (var loc in locs)
        {
            if (loc.LocationType == LocationTypes.N)
            {
                // N 位置无法禁入禁出
                _logger.LogWarning("不能禁用或启用 N 位置");
                continue;
            }

            if (loc.Cell != null)
            {
                foreach (var item in loc.Cell.Locations)
                {
                    await EnableOneAsync(item).ConfigureAwait(false);
                }
            }
            else
            {
                await EnableOneAsync(loc).ConfigureAwait(false);
            }
        }

        var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
        foreach (var streetlet in streetlets)
        {
            await _locHelper.RebuildStreetletStatAsync(streetlet!).ConfigureAwait(false);
        }

        _logger.LogInformation("已将 {updated} 个位置允许入站", updated.Count);
        _ = await this.SaveOpAsync($"位置：{string.Join(", ", updated)}。备注：{args.Comment}").ConfigureAwait(false);

        return this.Success();

        async Task EnableOneAsync(Location loc)
        {
            if (loc.IsInboundDisabled)
            {
                loc.IsInboundDisabled = false;
                loc.InboundDisabledComment = args.Comment;
                await _session.UpdateAsync(loc).ConfigureAwait(false);

                updated.Add(loc.LocationCode);

                _logger.LogInformation("已将位置 {locationCode} 允许入站", loc.LocationCode);
            }
        }
    }

    /// <summary>
    /// 禁止位置出站。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("disable-location-outbound")]
    [OperationType(OperationTypes.禁止位置出站)]
    [Transaction]
    public async Task<ApiData> DisableLocationOutbound(EnableLocationArgs args)
    {
        List<Location> locs = await _session.Query<Location>()
            .Where(x => args.LocationIds.Contains(x.LocationId))
            .ToListAsync()
            .ConfigureAwait(false);
        if (locs.Count == 0)
        {
            throw new InvalidOperationException("未指定货位。");
        }

        List<string> updated = new List<string>();
        foreach (var loc in locs)
        {
            if (loc.LocationType == LocationTypes.N)
            {
                // N 位置无法禁入禁出
                _logger.LogWarning("不能禁用或启用 N 位置");
                continue;
            }

            if (loc.Cell != null)
            {
                foreach (var item in loc.Cell.Locations)
                {
                    await DisableOneAsync(item).ConfigureAwait(false);
                }
            }
            else
            {
                await DisableOneAsync(loc).ConfigureAwait(false);
            }
        }

        var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
        foreach (var streetlet in streetlets)
        {
            if (streetlet != null)
            {
                await _locHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("已将 {updated} 个位置禁止出站", updated.Count);
        _ = await this.SaveOpAsync($"位置：{string.Join(", ", updated)}。备注：{args.Comment}").ConfigureAwait(false);

        return this.Success();

        async Task DisableOneAsync(Location loc)
        {
            if (loc.IsOutboundDisabled == false)
            {
                loc.IsOutboundDisabled = true;
                loc.OutboundDisabledComment = args.Comment;
                await _session.UpdateAsync(loc).ConfigureAwait(false);

                updated.Add(loc.LocationCode);

                _logger.LogInformation("已将位置 {locationCode} 禁止出站", loc.LocationCode);
            }
        }
    }

    /// <summary>
    /// 允许位置出站。
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("enable-location-outbound")]
    [OperationType(OperationTypes.允许位置出站)]
    [Transaction]
    public async Task<ApiData> EnableLocationOutbound(EnableLocationArgs args)
    {
        List<Location> locs = await _session.Query<Location>()
            .Where(x => args.LocationIds.Contains(x.LocationId))
            .ToListAsync()
            .ConfigureAwait(false);
        if (locs.Count == 0)
        {
            throw new InvalidOperationException("未指定货位。");
        }

        List<string> updated = new List<string>();
        foreach (var loc in locs)
        {
            if (loc.LocationType == LocationTypes.N)
            {
                // N 位置无法禁入禁出
                _logger.LogWarning("不能禁用或启用 N 位置");
                continue;
            }

            if (loc.Cell != null)
            {
                foreach (var item in loc.Cell.Locations)
                {
                    await EnableOneAsync(item).ConfigureAwait(false);
                }
            }
            else
            {
                await EnableOneAsync(loc).ConfigureAwait(false);
            }
        }

        var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
        foreach (var streetlet in streetlets)
        {
            if (streetlet != null)
            {
                await _locHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("已将 {updated} 个位置允许出站", updated.Count);
        _ = await this.SaveOpAsync($"位置：{string.Join(", ", updated)}。备注：{args.Comment}").ConfigureAwait(false);

        return this.Success();

        async Task EnableOneAsync(Location loc)
        {
            if (loc.IsOutboundDisabled)
            {
                loc.IsOutboundDisabled = false;
                loc.OutboundDisabledComment = args.Comment;
                await _session.UpdateAsync(loc).ConfigureAwait(false);

                updated.Add(loc.LocationCode);

                _logger.LogInformation("已将位置 {locationCode} 允许出站", loc.LocationCode);
            }
        }
    }




    /// <summary>
    /// 创建关键点
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("create-key-point")]
    [OperationType(OperationTypes.创建关键点)]
    [Transaction]
    public async Task<ApiData> CreateKeyPoint(CreateUpdateKeyPointArgs args)
    {
        Location loc = _locFactory.Invoke();
        loc.LocationCode = args.LocationCode;
        loc.LocationType = LocationTypes.K;
        loc.RequestType = args.RequestType;
        loc.OutboundLimit = args.OutboundLimit;
        loc.InboundLimit = args.InboundLimit;
        loc.Tag = args.Tag;
        await _session.SaveAsync(loc).ConfigureAwait(false);
        _ = await this.SaveOpAsync("{0}#{1}", loc.LocationCode, loc.LocationId).ConfigureAwait(false);

        return this.Success();
    }

    /// <summary>
    /// 编辑关键点
    /// </summary>
    /// <param name="id">关键点Id</param>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("update-key-point/{id}")]
    [OperationType(OperationTypes.编辑关键点)]
    [Transaction]
    public async Task<ApiData> UpdateKeyPoint(int id, CreateUpdateKeyPointArgs args)
    {
        Location loc = await _session.GetAsync<Location>(id).ConfigureAwait(false);
        if (loc == null || loc.LocationType != LocationTypes.K)
        {
            throw new InvalidOperationException("关键点不存在。");
        }
        loc.LocationCode = args.LocationCode;
        loc.RequestType = args.RequestType;
        loc.OutboundLimit = args.OutboundLimit;
        loc.InboundLimit = args.InboundLimit;
        loc.Tag = args.Tag;
        await _session.UpdateAsync(loc).ConfigureAwait(false);
        _ = await this.SaveOpAsync("{0}#{1}", loc.LocationCode, loc.LocationId).ConfigureAwait(false);

        return this.Success();
    }

    /// <summary>
    /// 创建出口
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [OperationType(OperationTypes.创建出口)]
    [Transaction]
    [HttpPost("create-outlet")]
    public async Task<ApiData> CreateOutlet(CreateOutletArgs args)
    {
        Outlet outlet = new Outlet(args.OutletCode);
        if (string.IsNullOrWhiteSpace(args.KP1) == false)
        {
            outlet.KP1 = await CreateKeyPointAsync(_locFactory, _session, args.KP1).ConfigureAwait(false);
        }
        if (string.IsNullOrWhiteSpace(args.KP2) == false)
        {
            outlet.KP2 = await CreateKeyPointAsync(_locFactory, _session, args.KP2).ConfigureAwait(false);
        }

        await _session.SaveAsync(outlet).ConfigureAwait(false);

        return this.Success();

        static async Task<Location> CreateKeyPointAsync(Func<Location> locationFactory, ISession session, string locationCode)
        {
            Location loc = locationFactory.Invoke();
            loc.LocationCode = locationCode;
            loc.LocationType = LocationTypes.K;
            loc.RequestType = null;
            loc.Tag = "出口";
            loc.InboundLimit = 999;
            loc.OutboundLimit = 999;
            await session.SaveAsync(loc).ConfigureAwait(false);
            return loc;
        }

    }


    /// <summary>
    /// 设置分组
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("set-storage-group")]
    [OperationType(OperationTypes.设置分组)]
    [Transaction]
    public async Task<ApiData> SetStorageGroup(SetStorageGroupArgs args)
    {
        List<Location> locs = await _session.Query<Location>()
            .Where(x => args.LocationIds.Contains(x.LocationId))
            .ToListAsync()
            .ConfigureAwait(false);

        int affected = 0;
        foreach (var loc in locs)
        {
            if (loc.LocationType != LocationTypes.S)
            {
                // N 位置无法禁入禁出
                _logger.LogWarning("只能为类型为 S 的位置设置存储分组");
                continue;
            }

            loc.StorageGroup = args.StorageGroup;
            await _session.UpdateAsync(loc).ConfigureAwait(false);
        }

        var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
        foreach (var streetlet in streetlets)
        {
            if (streetlet != null)
            {
                await _locHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);
            }
        }
        _ = await this.SaveOpAsync($"将 {affected} 个位置的存储分组设为 {args.StorageGroup}").ConfigureAwait(false);

        return this.Success();

    }


    /// <summary>
    /// 设置限高
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("set-height-limit")]
    [OperationType(OperationTypes.设置限高)]
    [Transaction]
    public async Task<ApiData> SetHeightLimit(SetHeightLimitArgs args)
    {
        List<Location> locs = await _session.Query<Location>()
            .Where(x => args.LocationIds.Contains(x.LocationId))
            .ToListAsync()
            .ConfigureAwait(false);

        int affected = 0;
        foreach (var loc in locs)
        {
            if (loc.LocationType != LocationTypes.S)
            {
                // N 位置无法禁入禁出
                _logger.LogWarning("只能为类型为 S 的位置设置限高");
                continue;
            }

            loc.HeightLimit = args.HeightLimit;
            await _session.UpdateAsync(loc).ConfigureAwait(false);
        }

        var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
        foreach (var streetlet in streetlets)
        {
            if (streetlet != null)
            {
                await _locHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);
            }
        }
        _ = await this.SaveOpAsync($"将 {affected} 个位置的存储分组设为 {args.HeightLimit}").ConfigureAwait(false);

        return this.Success();

    }


    /// <summary>
    /// 设置限重
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    [HttpPost("set-weight-limit")]
    [OperationType(OperationTypes.设置限重)]
    [Transaction]
    public async Task<ApiData> SetWeightLimit(SetWeightLimitArgs args)
    {
        List<Location> locs = await _session.Query<Location>()
            .Where(x => args.LocationIds.Contains(x.LocationId))
            .ToListAsync()
            .ConfigureAwait(false);

        int affected = 0;
        foreach (var loc in locs)
        {
            if (loc.LocationType != LocationTypes.S)
            {
                // N 位置无法禁入禁出
                _logger.LogWarning("只能为类型为 S 的位置设置限重");
                continue;
            }

            loc.WeightLimit = args.WeightLimit;
            await _session.UpdateAsync(loc).ConfigureAwait(false);
        }

        var streetlets = locs.Where(x => x.Streetlet != null).Select(x => x.Streetlet).Distinct();
        foreach (var streetlet in streetlets)
        {
            if (streetlet != null)
            {
                await _locHelper.RebuildStreetletStatAsync(streetlet).ConfigureAwait(false);
            }
        }
        _ = await this.SaveOpAsync($"将 {affected} 个位置的存储分组设为 {args.WeightLimit}").ConfigureAwait(false);

        return this.Success();

    }




    /// <summary>
    /// 获取储位详情
    /// </summary>
    /// <param name="args">位置编码</param>
    /// <returns></returns>
    [HttpPost("get-storage-location-detail")]
    [OperationType(OperationTypes.查看位置)]
    [Transaction]
    public async Task<ApiData<StorageLocationDetail>> GetStorageLocationDetail(GetStorageLocationDetailArgs args)
    {
        ArgumentNullException.ThrowIfNull(args?.LocationCode);

        var loc = await _session.Query<Location>()
            .Where(x => x.LocationType == LocationTypes.S && x.LocationCode == args.LocationCode)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
        if (loc == null)
        {
            throw new InvalidOperationException("位置不存在");
        }

        var unitloads = await _session.Query<Unitload>()
            .Where(x => x.CurrentLocation == loc)
            .ToListAsync()
            .ConfigureAwait(false);

        var tasks = await _session
            .Query<TransportTask>()
            .Where(x => unitloads.Contains(x.Unitload))
            .ToListAsync()
            .ConfigureAwait(false);

        var detail = new StorageLocationDetail
        {
            LocationId = loc.LocationId,
            LocationCode = loc.LocationCode,
            Exists = loc.Exists,
            StreetletId = loc.Streetlet!.StreetletId,
            StreetletCode = loc.Streetlet.StreetletCode,
            WeightLimit = loc.WeightLimit,
            HeightLimit = loc.HeightLimit,
            InboundCount = loc.InboundCount,
            IsInboundDisabled = loc.IsInboundDisabled,
            InboundDisabledComment = loc.InboundDisabledComment,
            OutboundCount = loc.OutboundCount,
            IsOutboundDisabled = loc.IsOutboundDisabled,
            OutboundDisabledComment = loc.OutboundDisabledComment,
            StorageGroup = loc.StorageGroup,
            UnitloadCount = loc.UnitloadCount,
            Unitloads = unitloads.Select(u => DtoConvert.ToUnitloadDetail(u, tasks.SingleOrDefault(x => x.Unitload == u))).ToArray(),
        };

        return this.Success(detail);

    }
}

