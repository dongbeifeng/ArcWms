namespace ArcWms;

/// <summary>
/// 货位单元格。单元格是一组货位，在出入库操作时需要一起考虑。例如：双深的一深和二深。
/// </summary>
public class Cell
{
    /// <summary>
    /// 初始化新实例
    /// </summary>
    public Cell()
    {
        this.Locations = new HashSet<Location>();
    }

    /// <summary>
    /// Id
    /// </summary>
    public virtual int CellId { get; protected set; }

    /// <summary>
    /// 获取此单元中的货位。
    /// </summary>
    public virtual ISet<Location> Locations { get; protected set; }


    /// <summary>
    /// 入次序 1。在分配货位时使用。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int i1 { get; set; }

    /// <summary>
    /// 入次序 2。在分配货位时使用。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int i2 { get; set; }

    /// <summary>
    /// 入次序 3。在分配货位时使用。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int i3 { get; set; }


    /// <summary>
    /// 出次序 1。在分配库存和下架时使用。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int o1 { get; set; }

    /// <summary>
    /// 出次序 2。在分配库存和下架时使用。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int o2 { get; set; }

    /// <summary>
    /// 出次序 3。在分配库存和下架时使用。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名样式", Justification = "特殊属性")]
    public virtual int o3 { get; set; }

}
