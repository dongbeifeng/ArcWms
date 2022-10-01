using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;

internal class OutboundLineMapping : ClassMapping<OutboundLine>
{
    public OutboundLineMapping()
    {
        Table("OutboundLines");
        DynamicUpdate(true);
        BatchSize(10);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.OutboundLineId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        ManyToOne(cl => cl.OutboundOrder, m =>
        {
            m.Column("OutboundOrderId");
            m.Update(false);
        });

        ManyToOne(cl => cl.Material, m =>
        {
            m.Column("MaterialId");
            m.Update(false);
        });

        Property(cl => cl.Batch);
        Property(cl => cl.InventoryStatus);

        Property(cl => cl.Uom);
        Property(cl => cl.QuantityDemanded);
        Property(cl => cl.QuantityFulfilled);
        Property(cl => cl.Dirty);

        Property(cl => cl.Comment);

        Set(cl => cl.Allocations, set =>
        {
            set.Inverse(true);
            set.BatchSize(10);
            set.Where($"outbound_demand_root_type = N'{OutboundLine.OutboundDemandRootType}'");
            set.Key(key =>
            {
                key.Column("OutboundDemandId");
            });
        }, rel => rel.OneToMany());
    }
}
