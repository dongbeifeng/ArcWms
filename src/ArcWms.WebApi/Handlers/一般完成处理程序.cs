using NHibernate.Linq;

namespace ArcWms.WebApi.Handlers;


public class 一般完成处理程序 : ICompletionHandler
{
    readonly ISession _session;
    readonly ILogger<一般完成处理程序> _logger;
    readonly TaskHelper _taskHelper;

    public 一般完成处理程序(TaskHelper taskHelper, ISession session, ILogger<一般完成处理程序> logger)
    {
        _taskHelper = taskHelper;
        _session = session;
        _logger = logger;
    }

    public async Task ProcessCompletedTaskAsync(CompletedTaskInfo taskInfo, TransportTask task)
    {
        _logger.LogInformation("任务信息 {taskInfo}", taskInfo);

        Location actualEnd = task.End ?? throw new();
        if (string.IsNullOrEmpty(taskInfo.ActualEnd) == false)
        {
            _logger.LogInformation("实际完成位置是 {actualEnd}", taskInfo.ActualEnd);
            actualEnd = await _session.Query<Location>()
                .Where(x => x.LocationCode == taskInfo.ActualEnd)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        switch (taskInfo.Cancelled)
        {
            case false:
                await _taskHelper.CompleteAsync(task, actualEnd, false).ConfigureAwait(false);
                break;
            case true:
            default:
                await _taskHelper.CancelAsync(task).ConfigureAwait(false);
                break;
        }
    }
}
