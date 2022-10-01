namespace ArcWms;

public interface IRule
{
    /// <summary>
    /// 获取此规则的名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取此规则的排序。
    /// </summary>
    int Order { get; }

    /// <summary>
    /// 判断此规则是否适用于特定巷道。
    /// </summary>
    /// <param name="streetlet"></param>
    /// <returns></returns>
    bool CanApplyTo(Streetlet streetlet);

    /// <summary>
    /// 在巷道内选择货位。
    /// </summary>
    /// <param name="streetlet">指定在哪个巷道中搜索货位。</param>
    /// <param name="storageInfo">货物的存储信息。</param>
    /// <param name="excludedIdList">要排除的货位。</param>
    /// <param name="excludedColumnList">要排除的列。</param>
    /// <param name="excludedLevelList">要排除的层。</param>
    /// <param name="orderBy">排序字段，这是 <see cref="Cell"/> 的字段。</param>
    /// <returns>若分配成功，则返回分配到的 <see cref="Location"/>，否则返回 null。</returns>
    Task<Location?> SelectAsync(
        Streetlet streetlet,
        StorageInfo storageInfo,
        int[]? excludedIdList,
        int[]? excludedColumnList,
        int[]? excludedLevelList,
        string orderBy);

    /// <summary>
    /// 测试一个货位是否可分配。
    /// </summary>
    /// <param name="location">要测试的货位。</param>
    /// <param name="streetlet">指定在哪个巷道中搜索货位。</param>
    /// <param name="storageInfo">货物的存储信息。</param>
    /// <param name="excludedIdList">要排除的货位。</param>
    /// <param name="excludedColumnList">要排除的列。</param>
    /// <param name="excludedLevelList">要排除的层。</param>
    /// <param name="messages"></param>
    /// <returns></returns>
    bool Check(Location location,
        Streetlet streetlet,
        StorageInfo storageInfo,
        int[]? excludedIdList,
        int[]? excludedColumnList,
        int[]? excludedLevelList,
        out List<string> messages);

}


