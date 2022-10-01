namespace ArcWms;

/// <summary>
/// 定义任务下发程序。
/// </summary>
public interface ITaskSender
{
    /// <summary>
    /// 下发任务。
    /// </summary>
    /// <param name="task"></param>
    Task SendTaskAsync(TransportTask task);
}
