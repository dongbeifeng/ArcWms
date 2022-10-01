namespace ArcWms;

/// <summary>
/// 定义请求处理程序。
/// </summary>
public interface IRequestHandler
{
    /// <summary>
    /// 对请求进行处理。
    /// </summary>
    /// <param name="requestInfo">请求信息</param>
    Task ProcessRequestAsync(RequestInfo requestInfo);
}
