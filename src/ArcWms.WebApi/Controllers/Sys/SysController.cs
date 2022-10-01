using Arc.Ops;
using ArcWms.WebApi.MetaData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NHibernateAspNetCoreFilters;
using NHibernateUtils;
using OperationTypeAspNetCoreAuthorization;
using Quartz;
using Quartz.Impl.Matchers;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 提供系统 api
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SysController : ControllerBase
{
    readonly ISession _session;
    readonly ILogger<SysController> _logger;
    readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    readonly ISchedulerFactory _schedulerFactory;


    /// <summary>
    /// 初始化新实例。
    /// </summary>
    /// <param name="appSettingService"></param>
    /// <param name="session"></param>
    /// <param name="actionDescriptorCollectionProvider"></param>
    /// <param name="schedulerFactory"></param>
    /// <param name="logger"></param>
    public SysController(
        ISession session,
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        ISchedulerFactory schedulerFactory,
        ILogger<SysController> logger
        )
    {
        _session = session;
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取操作记录列表。
    /// </summary>
    /// <param name="args">查询参数</param>
    /// <returns></returns>
    [HttpPost("get-op-list")]
    [Transaction]
    [OperationType(OperationTypes.查看操作记录)]
    public async Task<ListData<OpListInfo>> GetOpList(OpListArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Sort))
        {
            args.Sort = "opId desc";
        }
        var pagedList = await _session.Query<Op>().SearchAsync(args, args.Sort, args.Current, args.PageSize).ConfigureAwait(false);
        return this.ListData(pagedList, x => new OpListInfo
        {
            OpId = x.OpId,
            CreationTime = x.CreationTime,
            CreationUser = x.CreationUser,
            OperationType = x.OperationType,
            Url = x.Url,
            Comment = x.Comment
        });
    }

    /// <summary>
    /// 获取操作类型。
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-operation-types")]
    public OptionsData<string> GetOperationTypes()
    {
        var list = _actionDescriptorCollectionProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Select(x => x.MethodInfo.GetCustomAttribute<OperationTypeAttribute>()?.OperationType ?? "")
            .Where(x => string.IsNullOrWhiteSpace(x) == false)
            .Distinct()
            .ToList();
        return this.OptionsData(list);
    }


    /// <summary>
    /// 触发器列表
    /// </summary>
    /// <returns></returns>
    [OperationType(OperationTypes.查看触发器)]
    [HttpPost("get-trigger-list")]
    public async Task<ListData<QuartzTriggerInfo>> GetTriggerList()
    {
        var _scheduler = (await _schedulerFactory.GetScheduler("Wes.WebApi").ConfigureAwait(false)) ?? throw new();

        List<QuartzTriggerInfo> list = new List<QuartzTriggerInfo>();
        var triggerKeys = await _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).ConfigureAwait(false);
        foreach (var triggerKey in triggerKeys)
        {
            if (triggerKey.Group == "XMLSchedulingDataProcessorPlugin")
            {
                continue;
            }

            var trigger = await _scheduler.GetTrigger(triggerKey).ConfigureAwait(false);
            if (trigger == null)
            {
                continue;
            }

            var previousFireTimeUtc = trigger.GetPreviousFireTimeUtc();
            var nextFireTimeUtc = trigger.GetNextFireTimeUtc();

            QuartzTriggerInfo triggerInfo = new QuartzTriggerInfo
            {
                TriggerGroup = triggerKey.Group,
                TriggerName = triggerKey.Name,
                CronExpressionString = (trigger as ICronTrigger)?.CronExpressionString,
                PreviousFireTime = previousFireTimeUtc == null ? null : TimeZoneInfo.ConvertTimeFromUtc(previousFireTimeUtc.Value.DateTime, TimeZoneInfo.Local),
                NextFireTime = nextFireTimeUtc == null ? null : TimeZoneInfo.ConvertTimeFromUtc(nextFireTimeUtc.Value.DateTime, TimeZoneInfo.Local),
                TriggerDescription = trigger.Description,
                TriggerState = (await _scheduler.GetTriggerState(triggerKey).ConfigureAwait(false)).ToString(),
            };

            list.Add(triggerInfo);
        }
        list = list.OrderBy(x => x.TriggerGroup).ThenBy(x => x.TriggerName).ToList();

        return this.ListData(new PagedList<QuartzTriggerInfo>(list, 1, list.Count, list.Count));
    }

    /// <summary>
    /// 暂停触发器。注意：如果程序重启，则触发器会自动继续执行。
    /// </summary>
    /// <returns></returns>
    [OperationType(OperationTypes.暂停触发器)]
    [HttpPost("pause-trigger")]
    public async Task<ApiData> PauseTrigger(QuartzTriggerKeyInfo triggerKey)
    {
        var _scheduler = (await _schedulerFactory.GetScheduler("Wes.WebApi").ConfigureAwait(false)) ?? throw new();
        await _scheduler.PauseTrigger(triggerKey).ConfigureAwait(false);
        return this.Success();
    }

    /// <summary>
    /// 重新启动触发器。
    /// </summary>
    /// <returns></returns>
    [OperationType(OperationTypes.重新启动触发器)]
    [HttpPost("resume-trigger")]
    public async Task<ApiData> ResumeTrigger(QuartzTriggerKeyInfo triggerKey)
    {
        var _scheduler = (await _schedulerFactory.GetScheduler("Wes.WebApi").ConfigureAwait(false)) ?? throw new();
        await _scheduler.ResumeTrigger(triggerKey).ConfigureAwait(false);
        return this.Success();
    }
}


