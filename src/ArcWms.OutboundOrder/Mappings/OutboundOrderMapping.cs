using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;

internal class OutboundOrderMapping : ClassMapping<OutboundOrder>
{
    public OutboundOrderMapping()
    {
        Table("OutboundOrders");
        DynamicUpdate(true);
        BatchSize(10);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.OutboundOrderId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        NaturalId(npm =>
        {
            npm.Property(cl => cl.OutboundOrderCode, prop => prop.Update(false));
        }, m => m.Mutable(false));

        Version(cl => cl.v, v => v.Column("v"));

        Property(cl => cl.CreationTime, prop => prop.Update(false));
        Property(cl => cl.CreationUser, prop => prop.Update(false));
        Property(cl => cl.ModificationTime);
        Property(cl => cl.ModificationUser);

        Property(cl => cl.BizType);
        Property(cl => cl.BizOrder);

        Property(cl => cl.Closed);
        Property(cl => cl.ClosedBy);
        Property(cl => cl.ClosedAt);

        Property(cl => cl.Comment);

        Set(cl => cl.Lines, set =>
        {
            set.Inverse(true);
            set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            set.BatchSize(10);
            set.Key(key =>
            {
                key.Column("OutboundOrderId");
                key.NotNullable(true);
                key.Update(false);
            });
        }, rel => rel.OneToMany());

    }
}
