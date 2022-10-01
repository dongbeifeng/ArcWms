using NHibernate.Linq;

namespace ArcWms.WebApi.Handlers;

public class 上架请求处理程序 : IRequestHandler
{
    readonly TaskHelper _taskHelper;
    readonly ITaskSender _taskSender;
    readonly ISession _session;
    readonly SAllocationHelper _sallocHelper;
    readonly ILogger<上架请求处理程序> _logger;

    public 上架请求处理程序(ISession session, TaskHelper taskHelper, ITaskSender taskSender, SAllocationHelper sallocHelper, ILogger<上架请求处理程序> logger)
    {
        _session = session;
        _taskHelper = taskHelper;
        _taskSender = taskSender;
        _sallocHelper = sallocHelper;
        _logger = logger;
    }


    private record StreetletData(Streetlet Streetlet)
    {
        public int InboundCount { get; set; }
        public float VacancyRate { get; set; }
    }

    public async Task ProcessRequestAsync(RequestInfo requestInfo)
    {
        _logger.LogDebug(requestInfo.ToString());
        CheckRequest(requestInfo);

        // 1 加载入口数据
        Location entrance = await PrepareEntranceAsync(requestInfo).ConfigureAwait(false);

        // 2 加载托盘数据
        Unitload unitload = await PrepareUnitloadAsync(requestInfo).ConfigureAwait(false);

        // 将请求中的高度和重量记录到货载
        unitload.StorageInfo = unitload.StorageInfo with
        {
            Height = requestInfo.Height,
            Weight = requestInfo.Weight
        };

        // 3 分配货位
        Streetlet[] arr = await PrepareStreetlets(unitload.StorageInfo.StorageGroup).ConfigureAwait(false);

        Location? target = null;
        foreach (var streetlet in arr)
        {
            _logger.LogDebug("正在检查巷道 {streetletCode}", streetlet.StreetletCode);

            if (streetlet.IsDoubleDeep)
            {
                _logger.LogDebug("跳过 {streetletCode}：这是双深巷道 ", streetlet.StreetletCode);
                continue;
            }

            if (streetlet.IsInboundDisabled)
            {
                _logger.LogWarning("跳过 {streetletCode}：巷道已禁入", streetlet.StreetletCode);
                continue;
            }

            target = await _sallocHelper.AllocateAsync(streetlet, unitload.StorageInfo).ConfigureAwait(false);

            if (target != null)
            {
                _logger.LogInformation("在 {streetletCode} 分配到货位 {locationCode}", streetlet.StreetletCode, target.LocationCode);
                break;
            }
            else
            {
                _logger.LogInformation("在 {streetletCode} 未分配到货位。", streetlet.StreetletCode);
                continue;
            }
        }


        if (target == null)
        {
            // 分配货位失败
            throw new Exception("未分配到货位。");
        }

        // 4 生成任务
        const string taskType = "上架";
        var task = await _taskHelper.CreateAsync(taskType, entrance, target, unitload).ConfigureAwait(false);

        // 5 下发任务
        await _taskSender.SendTaskAsync(task).ConfigureAwait(false);
    }

    private async Task<Streetlet[]> PrepareStreetlets(string storageGroup)
    {
        var streetlets = await _session.Query<Streetlet>()
            .ToListAsync()
            .ConfigureAwait(false);

        var data = streetlets.Select(x => new StreetletData(x));
        foreach (var item in data)
        {
            item.InboundCount = await _session
                .Query<TransportTask>()
                .Where(t => t.End.Streetlet == item.Streetlet)
                .CountAsync()
                .ConfigureAwait(false);
            var usage = item.Streetlet.Usage.Where(y => y.Key.StorageGroup == storageGroup);
            item.VacancyRate = usage.Sum(y => y.Value.Available) / (float)usage.Sum(y => y.Value.Total);
        }

        return data
            .OrderBy(x => x.InboundCount)           // 优选选择入站少的巷道
            .ThenByDescending(x => x.VacancyRate)   // 其次选择空闲率大的巷道
            .Select(x => x.Streetlet)
            .ToArray();
    }

    private async Task<Unitload> PrepareUnitloadAsync(RequestInfo requestInfo)
    {
        string containerCode = requestInfo.PalletCode ?? throw new InvalidOperationException("未提供托盘号"); ;
        var unitload = await _session.Query<Unitload>()
            .Where(x => x.PalletCode == containerCode)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
        if (unitload == null)
        {
            string msg = string.Format("货载不存在。容器编码【{0}】。", containerCode);
            throw new InvalidRequestException(msg);
        }

        return unitload;
    }

    private async Task<Location> PrepareEntranceAsync(RequestInfo requestInfo)
    {
        string entranceLocationCode = requestInfo.LocationCode ?? throw new InvalidOperationException("未提供请求位置");
        var entrance = await _session.Query<Location>()
            .Where(x => x.LocationCode == entranceLocationCode)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
        if (entrance == null)
        {
            string msg = string.Format("请求位置在 Wms 中不存在。【{0}】。", entranceLocationCode);
            throw new InvalidRequestException(msg);
        }

        return entrance;
    }

    public virtual void CheckRequest(RequestInfo requestInfo)
    {
        if (string.IsNullOrWhiteSpace(requestInfo.LocationCode))
        {
            throw new InvalidRequestException("请求信息中应提供请求位置。");
        }

        if (string.IsNullOrWhiteSpace(requestInfo.PalletCode))
        {
            throw new InvalidRequestException("请求中未提供容器编码。");
        }

        if (requestInfo.Height < 0)
        {
            throw new InvalidRequestException("请求中提供的 Height 值无效，应大于等于 0。");
        }

        if (requestInfo.Weight < 0)
        {
            throw new InvalidRequestException("请求中提供的 Weight 值无效，应大于等于 0。");
        }
    }

}

