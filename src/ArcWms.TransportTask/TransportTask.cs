using NHibernateUtils;
using System.ComponentModel.DataAnnotations;

namespace ArcWms;

/// <summary>
/// 表示搬运托盘的任务。
/// </summary>
public class TransportTask
{

    public TransportTask()
    {
    }


    /// <summary>
    /// Id
    /// </summary>
    public virtual int TaskId { get; internal protected set; }

    /// <summary>
    /// 任务编码。
    /// </summary>
    [Required]
    [MaxLength(20)]
    public virtual string TaskCode { get; internal protected set; }

    /// <summary>
    /// 获取或设置任务类型，当任务完成时，使用此属性来决定分配给哪个处理程序。
    /// </summary>
    [Required]
    [MaxLength(20)]
    public virtual string TaskType { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    [CreationTime]
    public virtual DateTime CreationTime { get; set; } = DateTime.Now;


    /// <summary>
    /// 获取或设置此任务搬运的货载。
    /// </summary>
    [Required]
    public virtual Unitload Unitload { get; internal protected set; } = default!;


    /// <summary>
    /// 获取或设置此任务的起点位置。
    /// </summary>
    [Required]
    public virtual Location Start { get; internal protected set; } = default!;


    /// <summary>
    /// 获取或设置此任务的终点位置。
    /// </summary>
    [Required]
    public virtual Location End { get; internal protected set; } = default!;


    /// <summary>
    /// 指示此任务是否已下发。
    /// </summary>
    public virtual bool WasSent { get; set; }

    /// <summary>
    /// 获取货设置任务下发的时间。
    /// </summary>
    public virtual DateTime? SendTime { get; set; }

    /// <summary>
    /// 获取或设置此任务是为哪个Wms单据产生的。
    /// </summary>
    [MaxLength(20)]
    public virtual string? OrderCode { get; set; }

    [MaxLength(6)]
    public virtual string? OrderLine { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string? Comment { get; set; }



    public override string? ToString()
    {
        return $"{this.TaskType} {this.TaskCode}";
    }

}
