namespace ArcWms;

public interface IBatchService
{
    /// <summary>
    /// 获取一个特殊值，这个值表示没有批号。
    /// </summary>
    /// <returns></returns>
    string GetValueForNoBatch();

    /// <summary>
    /// 对用户录入的批号值进行规范化。
    /// </summary>
    /// <param name="batch"></param>
    /// <returns></returns>
    string Normalize(string? batch);
}
