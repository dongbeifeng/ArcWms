namespace ArcWms;

/// <summary>
/// 自动下架处理程序。
/// </summary>
public interface IAutoMoveDownHandler
{
    /// <summary>
    /// 检查一次。
    /// </summary>
    /// <param name="unitloadAllocationTable"></param>
    /// <param name="outlet"></param>
    /// <returns></returns>
    Task<string> CheckAsync(IUnitloadAllocationTable unitloadAllocationTable, Outlet outlet);
}
