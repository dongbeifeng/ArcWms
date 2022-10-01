using Arc.AppSeqs;
using Autofac.Features.Indexed;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ArcWms;

/// <summary>
///  提供任务生成、完成、取消操作。
/// </summary>
public sealed class TaskHelper
{
    readonly ISession _session;
    readonly ILogger<TaskHelper> _logger;
    readonly Func<TransportTask> _createTransportTask;
    readonly Func<ArchivedTransportTask> _createArchivedTransportTask;

    readonly IAppSeqService _appSeqService;
    readonly UnitloadHelper _unitloadHelper;
    readonly IIndex<string, ICompletionHandler> _completionHandlers;
    readonly IIndex<string, IRequestHandler> _requestHandlers;

    public const string ChangeUnitloadLocationTaskType = "更改托盘位置";

    [DoesNotReturn]
    static void ThrowInvalidRequestException(string error)
    {
        throw new InvalidRequestException(error);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    /// <exception cref="CreateTaskFailedException"></exception>
    [DoesNotReturn]
    static void ThrowCreateTaskFailedException(string error)
    {
        throw new CreateTaskFailedException(error);
    }

    [DoesNotReturn]
    static void ThrowCompleteTaskFailedException(string error)
    {
        throw new CompleteTaskFailedException(error);
    }


    public TaskHelper(
        ISession session,
        IAppSeqService appSeqService,
        UnitloadHelper unitloadHelper,
        Func<TransportTask> createTransportTask,
        Func<ArchivedTransportTask> createArchivedTransportTask,
        IIndex<string, IRequestHandler> requestHandlers,
        IIndex<string, ICompletionHandler> completedTaskHandlers,
        ILogger<TaskHelper> logger)
    {
        _session = session;
        _appSeqService = appSeqService;
        _unitloadHelper = unitloadHelper;
        _createTransportTask = createTransportTask;
        _createArchivedTransportTask = createArchivedTransportTask;
        _logger = logger;
        _completionHandlers = completedTaskHandlers;
        _requestHandlers = requestHandlers;
    }

    [return: NotNull]
    public IRequestHandler GetRequestHandler(string requestType)
    {
        Guard.IsNotNullOrWhiteSpace(requestType, nameof(requestType));

        if (_requestHandlers.TryGetValue(requestType, out var handler) == false)
        {
            ThrowInvalidRequestException($"不支持的请求类型 {requestType}");
        }

        return handler;
    }


    [return: NotNull]
    public ICompletionHandler GetCompletionHandler(string taskType)
    {
        Guard.IsNotNullOrWhiteSpace(taskType, nameof(taskType));

        if (_completionHandlers.TryGetValue(taskType, out var handler) == false)
        {
            ThrowCompleteTaskFailedException($"不支持的任务类型 {taskType}");
        }

        return handler;
    }


    public IEnumerable<string>? GetSupportedRequestTypes()
    {
        return SupportedRequestTypes;
    }

    public IEnumerable<string>? GetSupportedTaskTypes()
    {
        return SupportedTaskTypes;
    }

    internal static IEnumerable<string>? SupportedRequestTypes { get; set; }

    internal static IEnumerable<string>? SupportedTaskTypes { get; set; }

    /// <summary>
    /// 生成任务，并保存到数据库。
    /// </summary>
    /// <param name="transTask">空白的、尚未保存到数据库的任务对象。</param>
    /// <param name="start">起点。</param>
    /// <param name="end">终点。</param>
    /// <param name="unitload">要移动的货载。</param>
    /// <param name="taskType">任务类型。</param>
    /// <param name="populate">为派生类扩展的属性赋值。</param>
    /// <exception cref="CreateTaskFailedException"></exception>
    public async Task<TransportTask> CreateAsync(string taskType, Location start, Location end, Unitload unitload)
    {
        Guard.IsNotNullOrWhiteSpace(taskType, nameof(taskType));
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);
        ArgumentNullException.ThrowIfNull(unitload);

        // 检查        
        this.CheckForCreate(start, end, unitload, taskType);
        
        _logger.LogInformation("正在创建任务");

        // 创建任务
        var taskCode = await GetNextTaskCode().ConfigureAwait(false);
        TransportTask transportTask = _createTransportTask();
        transportTask.TaskCode = taskCode;
        transportTask.TaskType = taskType;
        transportTask.Unitload = unitload;
        transportTask.Start = start;
        transportTask.End = end;

        await _session.SaveAsync(transportTask).ConfigureAwait(false);

        // 维护货载的属性
        unitload.HasTask = true;
        await _session.UpdateAsync(unitload).ConfigureAwait(false);

        // 维护起点和终点的属性
        start.OutboundCount += 1;
        await _session.UpdateAsync(start).ConfigureAwait(false);
        end.InboundCount += 1;
        await _session.UpdateAsync(end).ConfigureAwait(false);

        _logger.LogInformation("已创建任务，任务号 {taskCode}, 任务类型 {taskType}", transportTask.TaskCode, transportTask.TaskType);

        return transportTask;
    }


    /// <summary>
    /// 检查任务是否合法。
    /// </summary>
    /// <param name="start">任务起点。</param>
    /// <param name="end">任务终点。</param>
    /// <param name="unitload">要移动的货载。</param>
    /// <exception cref="CreateTaskFailedException"></exception>
    internal void CheckForCreate(Location start, Location end, Unitload unitload, string taskType)
    {
        _logger.LogDebug("正在进行创建前检查");

        if (start == end)
        {
            ThrowCreateTaskFailedException($"起点和终点相同");
        }

        // 检查货载
        if (unitload.HasTask)
        {
            ThrowCreateTaskFailedException($"托盘已有任务");
        }

        if (taskType == ChangeUnitloadLocationTaskType)
        {
            if (unitload.CurrentLocation != start)
            {
                ThrowCreateTaskFailedException($"起点与托盘所在位置不同");
            }
        }
        else
        {
            // 检查起点
            if (start.Exists == false)
            {
                ThrowCreateTaskFailedException($"起点不存在");
            }

            if (start.IsOutboundDisabled)
            {
                ThrowCreateTaskFailedException($"起点禁止出站");
            }

            if (start.Streetlet?.IsOutboundDisabled == true)
            {
                ThrowCreateTaskFailedException($"起点巷道禁止出站");
            }

            if (start.OutboundCount >= start.OutboundLimit)
            {
                ThrowCreateTaskFailedException($"起点达到出站数限制");
            }

            // 检查货载当前位置和起点的关系
            if (unitload.CurrentLocation is not null)
            {
                if (unitload.CurrentLocation.LocationType == LocationTypes.N)
                {
                    if (start.LocationType != LocationTypes.K)
                    {
                        ThrowCreateTaskFailedException($"托盘在N位置时任务起点必须是K位置");
                    }
                }
                else if (unitload.CurrentLocation != start)
                {
                    ThrowCreateTaskFailedException($"起点与托盘所在位置不同");
                }
            }

            // 检查终点
            if (end.Exists == false)
            {
                ThrowCreateTaskFailedException($"终点不存在");
            }

            if (end.IsInboundDisabled)
            {
                ThrowCreateTaskFailedException($"终点禁止入站");
            }

            if (end.Streetlet?.IsInboundDisabled == true)
            {
                ThrowCreateTaskFailedException($"终点巷道禁止入站");
            }

            if (end.InboundCount >= end.InboundLimit)
            {
                ThrowCreateTaskFailedException($"终点达到入站数限制");
            }
        }

        _logger.LogDebug("已通过创建前检查");

    }

    /// <summary>
    /// 完成任务。
    /// </summary>
    /// <param name="transportTask">要完成的任务。</param>
    /// <param name="actualEnd">货载实际到达的位置。</param>
    /// <param name="checkEnd">指示是否对终点进行检查。
    /// 设为 true 时，将检查终点的 <see cref="Location.UnitloadCount"/>，若大于 0，则抛出 InvalidOperationException 异常。
    /// 
    /// 出现此问题的典型场景是巷道口，底层可能会出现错误，
    /// 在前一个货载尚未发走时，将下一个移动提前报完成。
    /// 这种情况下，如果允许完成，将导致巷道口上存在两个先后到达的货载。
    /// 
    /// 到货架货位的任务不会出现此问题，因为在任务生成之前会执行分配操作，排除了有货的货位。
    /// </param>
    public async Task<ArchivedTransportTask> CompleteAsync(TransportTask transportTask, Location actualEnd, bool checkEnd = true)
    {
        ArgumentNullException.ThrowIfNull(transportTask);
        ArgumentNullException.ThrowIfNull(actualEnd);

        _logger.LogDebug("正在完成任务 {taskCode}，任务类型 {taskType}，完成位置 {actualEnd}", transportTask.TaskCode, transportTask.TaskType, actualEnd.LocationCode);

        if (checkEnd)
        {
            if (actualEnd.UnitloadCount - actualEnd.OutboundCount > 0)
            {
                throw new CompleteTaskFailedException("终点上有其他托盘");
            }
        }

        await _unitloadHelper.LeaveCurrentLocationAsync(transportTask.Unitload).ConfigureAwait(false);
        await _unitloadHelper.EnterAsync(transportTask.Unitload, actualEnd).ConfigureAwait(false);

        transportTask.Unitload.HasTask = false;
        var archived = await ArchiveAsync(transportTask, false, actualEnd);
        transportTask.Start.OutboundCount -= 1;
        transportTask.End.InboundCount -= 1;

        _logger.LogInformation("任务已完成 {taskCode}", transportTask.TaskCode);

        return archived;
    }


    /// <summary>
    /// 取消任务。
    /// </summary>
    /// <param name="transportTask"></param>
    /// <param name="checkStart">
    /// 指示取消前是否对起点进行检查。设为 true 时，将检查起点是否有其他托盘。
    /// 
    /// 出现此问题的典型场景是巷道口，堆垛机将巷道口上的货载取走后，
    /// 后面的货载会抵达巷道口，如果允许取消，将导致巷道口上存在两个托盘。
    /// </param>
    public async Task<ArchivedTransportTask> CancelAsync(TransportTask transportTask, bool checkStart = true)
    {
        ArgumentNullException.ThrowIfNull(transportTask);

        if (transportTask.Unitload is null)
        {
            throw new();
        }

        _logger.LogDebug("正在取消任务 {taskCode}。", transportTask.TaskCode);

        if (checkStart)
        {
            // 当托盘在 N 位置上时，当前位置和起点不同
            if (transportTask.Unitload.CurrentLocation == transportTask.Start && transportTask.Start.UnitloadCount - transportTask.Start.OutboundCount > 0)
            {
                throw new CancelTaskFailedException("起点上有其他托盘");
            }
        }

        transportTask.Unitload.HasTask = false;
        var archived = await ArchiveAsync(transportTask, true, transportTask.Start!).ConfigureAwait(false);
        transportTask.Start.OutboundCount -= 1;
        transportTask.End.InboundCount -= 1;

        _logger.LogInformation("已取消任务 {taskCode}", transportTask.TaskCode);

        return archived;
    }


    /// <summary>
    /// 更改货载位置。会生成一条归档任务以方便在历史任务列表中查看货载位置的变动情况。
    /// </summary>
    /// <param name="unitload">要更改位置的货载。</param>
    /// <param name="target">要将货载更改到的目标位置。</param>
    /// <param name="comment">更改的备注，将记在 ArchivedMove.Comments 属性中。</param>
    /// <param name="throwIfHasInbound">指示在目标位置有入站时，是否抛出异常。</param>
    /// <param name="throwIfHasOutbound">指示在目标位置有出站时，是否抛出异常。</param>
    /// <param name="throwIfLoaded">指示在目标位置有货时，是否抛出异常。</param>
    public async Task<ArchivedTransportTask> ChangeUnitloadLocationAsync(
        Unitload unitload, 
        Location target, 
        string comment,
        bool throwIfLoaded = true,
        bool throwIfHasInbound = true,
        bool throwIfHasOutbound = false
        )
    {
        ArgumentNullException.ThrowIfNull(unitload);
        ArgumentNullException.ThrowIfNull(target);

        _logger.LogInformation("正在更改托盘 {palletCode} 的位置，目标是 {dest}", unitload.PalletCode, target.LocationCode);

        if (unitload.HasTask)
        {
            ThrowHelper.ThrowInvalidOperationException("无法更改托盘位置：托盘有任务。");
        }

        if (throwIfLoaded && target.IsLoaded())
        {
            ThrowHelper.ThrowInvalidOperationException("无法更改托盘位置：目标位置上有其他托盘。");
        }
        if (throwIfHasInbound && target.InboundCount > 0)
        {
            ThrowHelper.ThrowInvalidOperationException("无法更改托盘位置：目标位置有入站任务。");
        }
        if (throwIfHasOutbound && target.OutboundCount > 0)
        {
            ThrowHelper.ThrowInvalidOperationException("无法更改托盘位置：目标位置有出站任务。");
        }


        Location? orig = unitload.CurrentLocation;
        if (orig == null)
        {
            orig = await _session.Query<Location>()
                .Where(x => x.LocationCode == NLocationCode.Value)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        // 创建任务并立即完成
        var transTask = await CreateAsync(ChangeUnitloadLocationTaskType, orig, target, unitload).ConfigureAwait(false);
        transTask.Comment = comment;
        var archived = await CompleteAsync(transTask, target, false).ConfigureAwait(false);

        _logger.LogInformation("已将托盘 {palletCode} 的位置改为 {dest}", unitload.PalletCode, target.LocationCode);

        return archived;
    }


    private async Task<string> GetNextTaskCode()
    {
        DateTime today = DateTime.Now.Date;
        string seqName = string.Format("TaskCode-{0:yyMMdd}", today);
        int nextSn = await _appSeqService.GetNextAsync(seqName).ConfigureAwait(false);
        return $"T{today:yyMMdd}{nextSn:000000}";
    }

    public async Task<ArchivedTransportTask> ArchiveAsync(TransportTask transportTask, bool cancelled, Location actualEnd)
    {
        ArgumentNullException.ThrowIfNull(transportTask);
        ArgumentNullException.ThrowIfNull(actualEnd);

        _logger.LogInformation("正在归档任务 {taskCode}", transportTask.TaskCode);
        ArchivedTransportTask archived = _createArchivedTransportTask.Invoke();
        archived.Unitload = _unitloadHelper.GetSnapshot(transportTask.Unitload);
        await _session.SaveAsync(archived.Unitload).ConfigureAwait(false);
        await _session.FlushAsync().ConfigureAwait(false);

        CopyProperties(transportTask, archived, new[]
        {
            nameof(ArchivedTransportTask.ArchivedAt),
            nameof(ArchivedTransportTask.Cancelled),
            nameof(ArchivedTransportTask.ActualEnd),
            nameof(ArchivedTransportTask.Unitload),
        });
        archived.ArchivedAt = DateTime.Now;
        archived.Cancelled = cancelled;
        archived.ActualEnd = actualEnd;

        await _session.DeleteAsync(transportTask).ConfigureAwait(false);
        await _session.SaveAsync(archived).ConfigureAwait(false);

        _logger.LogInformation("已归档任务 {taskCode}", transportTask.TaskCode);
        return archived;
    }

    /// <summary>
    /// 遍历目标对象的属性，从源对象复制同名属性的值，属性名不区分大小写。
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dest"></param>
    /// <param name="excluded">目标类型中要排除的属性名称，属性名不区分大小写。</param>
    internal static void CopyProperties(TransportTask src, ArchivedTransportTask dest, string[]? excluded = null)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(dest);

        excluded ??= new string[0];

        var destProps = dest.GetType()
            .GetProperties()
            .Where(x => excluded.Contains(x.Name, StringComparer.OrdinalIgnoreCase) == false)
            .ToArray();
        foreach (var destProp in destProps)
        {
            var srcProp = src.GetType().GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (srcProp is not null && destProp.PropertyType.IsAssignableFrom(srcProp.PropertyType))
            {
                object? val = srcProp.GetValue(src);
                destProp.SetValue(dest, val);
            }
        }
    }
}
