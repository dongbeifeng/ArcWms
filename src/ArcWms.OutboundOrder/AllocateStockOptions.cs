namespace ArcWms;

/// <summary>
/// 出库单库存分配选项
/// </summary>
public class AllocateStockOptions
{
    /// <summary>
    /// 指示在哪些区域内分配，null 或空数组表示在全部区域分配
    /// </summary>
    public string[]? Areas { get; set; }

    ///// <summary>
    ///// 指示排除哪些巷道
    ///// </summary>
    //public Streetlet[] ExcludeStreetlets { get; init; } = new Streetlet[0];

    /// <summary>
    /// 指示包含哪些托盘，这些托盘优先参与分配
    /// </summary>
    public string[]? IncludePallets { get; set; }

    /// <summary>
    /// 指示排除哪些托盘，这些托盘不会参与分配，即使出现在 <see cref="IncludePallets"/> 中，也不参与分配
    /// </summary>
    public string[]? ExcludePallets { get; set; }

    ///// <summary>
    ///// 指示是否跳过已禁止出站的货位。已禁止出站的货位上的托盘无法下架，但跳过会打破先入先出规则。默认为 false
    ///// </summary>
    //public bool SkipLocationsOutboundDisabled { get; init; } = false;

    // TODO 实现 IncludeOutside
    ///// <summary>
    ///// 指示是否包含库外的托盘。库外托盘分配后存在找不到的可能性。默认为 false
    ///// </summary>
    //public bool IncludeOutside { get; init; } = false;

    /// <summary>
    /// 指示是否跳过脱机的巷道。已脱机巷道的托盘无法下架，但跳过会打破先入先出规则。默认为 false
    /// </summary>
    public bool SkipOfflineStreetlets { get; set; } = false;

    /// <summary>
    /// 从数据库加载数据的块大小，默认为 10
    /// </summary>
    public int ChunkSize { get; set; } = 10;

    public void Normalize()
    {
        this.Areas = HandleArray(this.Areas);
        this.IncludePallets = HandleArray(this.IncludePallets);
        this.ExcludePallets = HandleArray(this.ExcludePallets);

        if (this.ChunkSize <= 0)
        {
            this.ChunkSize = 10;
        }

        static string[] HandleArray(string[]? arr)
        {
            if (arr == null)
            {
                arr = new string[0];
            }
            arr = arr.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            return arr;
        }
    }
}
