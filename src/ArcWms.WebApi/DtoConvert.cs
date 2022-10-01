using ArcWms.WebApi.Controllers;
using System.Data;

namespace ArcWms.WebApi;

/// <summary>
/// 将业务模型对象转换为 DTO。
/// </summary>
public static class DtoConvert
{
    /// <summary>
    /// 将 <see cref="UnitloadItem"/> 转换为 <see cref="UnitloadItemInfo"/>
    /// </summary>
    /// <param name="unitloadItem"></param>
    /// <returns></returns>
    public static UnitloadItemInfo ToUnitloadItemInfo(UnitloadItem unitloadItem)
    {
        OutboundOrder? obo = unitloadItem.Unitload?.CurrentUat as OutboundOrder;

        return new UnitloadItemInfo
        {
            UnitloadItemId = unitloadItem.UnitloadItemId,
            MaterialId = unitloadItem.Material?.MaterialId ?? 0,
            MaterialCode = unitloadItem.Material?.MaterialCode,
            MaterialType = unitloadItem.Material?.MaterialType,
            Description = unitloadItem.Material?.Description,
            Specification = unitloadItem.Material?.Specification,
            Batch = unitloadItem.Batch,
            InventoryStatus = unitloadItem.InventoryStatus,
            Quantity = unitloadItem.Quantity,
            AllocationsToOutboundOrder = unitloadItem.Allocations
                .Where(x => x.OutboundDemand != null && x.OutboundDemand is OutboundLine)
                .Select(x => new UnitloadItemInfo.AllocationInfoToOutboundOrder
                {
                    UnitloadItemAllocationId = x.UnitloadItemAllocationId,
                    OutboundLineId = ((OutboundLine)x.OutboundDemand!).OutboundLineId,
                    QuantityAllocated = x.QuantityAllocated,
                })
                .ToArray(),
            Uom = unitloadItem.Uom,
        };
    }


    /// <summary>
    /// 将 <see cref="UnitloadItemSnapshot"/> 转换为 <see cref="UnitloadItemInfo"/>
    /// </summary>
    /// <param name="unitloadItem"></param>
    /// <returns></returns>
    public static UnitloadItemInfo ToUnitloadItemInfo(UnitloadItemSnapshot unitloadItem)
    {

        return new UnitloadItemInfo
        {
            MaterialId = unitloadItem.Material?.MaterialId ?? 0,
            MaterialCode = unitloadItem.Material?.MaterialCode,
            MaterialType = unitloadItem.Material?.MaterialType,
            Description = unitloadItem.Material?.Description,
            Specification = unitloadItem.Material?.Specification,
            Batch = unitloadItem.Batch,
            InventoryStatus = unitloadItem.InventoryStatus,
            Quantity = unitloadItem.Quantity,
            Uom = unitloadItem.Uom,
        };
    }

    /// <summary>
    /// 将 <see cref="Unitload"/> 转换为 <see cref="UnitloadDetail"/>
    /// </summary>
    /// <param name="unitload"></param>
    /// <returns></returns>
    public static UnitloadDetail ToUnitloadDetail(Unitload unitload, TransportTask? task)
    {
        OutboundOrder? obo = unitload.CurrentUat as OutboundOrder;

        return new UnitloadDetail
        {
            UnitloadId = unitload.UnitloadId,
            PalletCode = unitload.PalletCode,
            CreationTime = unitload.CreationTime,
            LocationCode = unitload.CurrentLocation?.LocationCode,
            LocationType = unitload.CurrentLocation?.LocationType,
            LocationTime = unitload.CurrentLocationTime,
            StreetletCode = unitload.CurrentLocation?.Streetlet?.StreetletCode,
            HasTask = unitload.HasTask,
            Items = unitload.Items.Select(i => ToUnitloadItemInfo(i)).ToList(),
            Comment = unitload.Comment,

            CurrentTaskCode = task?.TaskCode,
            CurrentTaskType = task?.TaskType,
            CurrentTaskStartLocationCode = task?.Start?.LocationCode,
            CurrentTaskEndLocationCode = task?.End?.LocationCode,

            CurrentUat = unitload.CurrentUat?.ToString(),
            OpHintInfo = unitload.OpHintInfo,
            OpHintType = unitload.OpHintType,

            Weight = unitload.StorageInfo.Weight,
            Height = unitload.StorageInfo.Height,
            StorageGroup = unitload.StorageInfo.StorageGroup,
            OutFlag = unitload.StorageInfo.OutFlag,
            PalletSpecification = unitload.StorageInfo.PalletSpecification,

        };
    }

    /// <summary>
    /// 将 <see cref="Unitload"/> 转换为 <see cref="UnitloadInfo"/>
    /// </summary>
    /// <param name="unitload"></param>
    /// <returns></returns>
    public static UnitloadInfo ToUnitloadInfo(Unitload unitload)
    {
        return new UnitloadInfo
        {
            UnitloadId = unitload.UnitloadId,
            PalletCode = unitload.PalletCode,
            CreationTime = unitload.CreationTime,
            ModificationTime = unitload.ModificationTime,
            LocationCode = unitload.CurrentLocation?.LocationCode,
            LocationType = unitload.CurrentLocation?.LocationType,
            StreetletCode = unitload.CurrentLocation?.Streetlet?.StreetletCode,
            HasTask = unitload.HasTask,
            Items = unitload.Items.Select(i => ToUnitloadItemInfo(i)).ToList(),
            Allocated = (unitload.CurrentUat != null),
            Comment = unitload.Comment
        };

    }

}
