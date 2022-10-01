namespace ArcWms.WebApi.MetaData;

class SupportedBizTypes
{
    public static readonly InboundBizType 独立入库 = new InboundBizType("独立入库");
    public static readonly OutboundBizType 独立出库 = new OutboundBizType("独立出库");
    public static readonly StatusChangingBizType 待检转合格 = new StatusChangingBizType("待检转合格", SupportedInventoryStatuses.待检, SupportedInventoryStatuses.合格);
    public static readonly StatusChangingBizType 待检转不合格 = new StatusChangingBizType("待检转不合格", SupportedInventoryStatuses.待检, SupportedInventoryStatuses.不合格);
    public static readonly StatusChangingBizType 合格转不合格 = new StatusChangingBizType("合格转不合格", SupportedInventoryStatuses.合格, SupportedInventoryStatuses.不合格);
    public static readonly StatusChangingBizType 不合格转合格 = new StatusChangingBizType("不合格转合格", SupportedInventoryStatuses.不合格, SupportedInventoryStatuses.合格);
}
