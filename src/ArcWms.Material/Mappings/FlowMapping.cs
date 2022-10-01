using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;


internal class FlowMapping : ClassMapping<Flow>
{
    public FlowMapping()
    {
        Table("Flows");
        BatchSize(10);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.FlowId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        Property(cl => cl.CreationTime, prop => prop.Update(false));
        Property(cl => cl.CreationUser, prop => prop.Update(false));

        ManyToOne(cl => cl.Material, m =>
        {
            m.Column("MaterialId");
            m.Update(false);
        });

        Property(cl => cl.Batch);
        Property(cl => cl.InventoryStatus);
        Property(cl => cl.Quantity);
        Property(cl => cl.Uom);


        Property(cl => cl.BizType);
        Property(cl => cl.Direction);
        Property(cl => cl.OperationType);

        Property(cl => cl.OrderCode);
        Property(cl => cl.BizOrder);
        Property(cl => cl.PalletCode);

        Property(cl => cl.Balance);
        Property(cl => cl.Comment);
    }
}
