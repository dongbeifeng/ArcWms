using System.ComponentModel.DataAnnotations;

namespace ArcWms;

public class ArchivedTransportTask
{
    public ArchivedTransportTask()
    {
    }

    public virtual int TaskId { get; set; }

    [Required]
    [MaxLength(20)]
    public virtual string TaskCode { get; internal protected set; }

    [Required]
    [MaxLength(20)]
    public virtual string TaskType { get; set; }

    public virtual DateTime CreationTime { get; set; }


    [Required]
    public virtual UnitloadSnapshot Unitload { get; internal protected set; }

    [Required]
    public virtual Location Start { get; set; }

    [Required]
    public virtual Location End { get; set; }

    [Required]
    public virtual Location ActualEnd { get; internal protected set; }


    public virtual bool WasSent { get; set; }

    public virtual DateTime? SendTime { get; set; }

    [MaxLength(20)]
    public virtual string? OrderCode { get; set; }

    [MaxLength(6)]
    public virtual string? OrderLine { get; set; }


    public virtual string? Comment { get; set; }

    public virtual DateTime ArchivedAt { get; set; }

    public virtual bool Cancelled { get; set; }

}
