using System.ComponentModel.DataAnnotations;

namespace ArcWms.WebApi.Controllers;

/// <summary>
/// 库存状态变更的操作参数
/// </summary>
public class ChangeInventoryStatusArgs
{
    /// <summary>
    /// 要变更状态的货载项
    /// </summary>
    public int[] UnitloadItemIds { get; set; } = new int[0];

    /// <summary>
    /// 业务类型
    /// </summary>
    [Required]
    public string? BizType { get; set; }

}

