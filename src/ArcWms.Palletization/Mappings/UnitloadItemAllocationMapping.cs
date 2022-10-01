using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;

internal class UnitloadItemAllocationMapping : ClassMapping<UnitloadItemAllocation>
{
    public UnitloadItemAllocationMapping()
    {
        Table("UnitloadItemAllocations");
        BatchSize(10);
        DynamicUpdate(true);
        Lazy(false);

        Id(cl => cl.UnitloadItemAllocationId, id => id.Generator(Generators.Identity));

        ManyToOne(cl => cl.UnitloadItem, m =>
        {
            m.Column(nameof(UnitloadItem.UnitloadItemId));
            m.Update(false);
        });

        Any(cl => cl.OutboundDemand, typeof(int), m =>
        {
            m.Lazy(false);
            m.Columns(x =>
            {
                x.Name("OutboundDemandId");
            }, x =>
            {
                x.Name("OutboundDemandType");
                x.Length(50);
            });
        });
        Property(cl => cl.OutboundDemandRootType);
        Property(cl => cl.QuantityAllocated);
        Property(cl => cl.Comment);
    }
}
