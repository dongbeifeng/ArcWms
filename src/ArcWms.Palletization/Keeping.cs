using System.ComponentModel.DataAnnotations;

namespace ArcWms;


/// <summary>
/// 内部类，利用数据库外键引用 Unitload，防止 <see cref="ArcWms.Unitload"/> 在与 <see cref="ArcWms.Location"/> 存在关联时被直接删除，
/// 造成 <see cref="Location.UnitloadCount"/> 属性不正确。
/// </summary>
internal class Keeping
{
    public Keeping()
    {
    }

    [Required]
    public virtual Unitload? Unitload { get; set; }

    [Required]
    public virtual Location? Location { get; set; }

}
