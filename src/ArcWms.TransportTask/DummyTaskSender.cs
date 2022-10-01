using NHibernate;

namespace ArcWms;

/// <summary>
/// <see cref="ITaskSender"/> 的哑实现。
/// </summary>
public class DummyTaskSender : ITaskSender
{
    ISession _session;
    public DummyTaskSender(ISession session)
    {
        _session = session;
    }
    public Task SendTaskAsync(TransportTask task)
    {
        task.WasSent = true;
        task.SendTime = DateTime.Now;
        return _session.SaveAsync(task);
    }
}
