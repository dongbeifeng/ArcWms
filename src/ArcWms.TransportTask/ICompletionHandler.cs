namespace ArcWms;

/// <summary>
/// 定义任务完成处理程序。
/// </summary>
public interface ICompletionHandler
{
    /// <summary>
    /// 处理已完成的任务。
    /// </summary>
    /// <param name="taskInfo">已完成的任务信息。</param>
    /// <param name="task">任务对象，此对象中的数据处于完成前的状态。</param>
    Task ProcessCompletedTaskAsync(CompletedTaskInfo taskInfo, TransportTask task);
}
