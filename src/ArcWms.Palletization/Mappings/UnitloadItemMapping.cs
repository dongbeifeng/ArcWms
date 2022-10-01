using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;

internal class UnitloadItemMapping : ClassMapping<UnitloadItem>
{
    public UnitloadItemMapping()
    {
        Table("UnitloadItems");
        BatchSize(10);
        DynamicUpdate(true);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.UnitloadItemId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        ManyToOne(cl => cl.Unitload, m =>
        {
            m.Column(nameof(Unitload.UnitloadId));
            m.Update(false);
        });

        ManyToOne(cl => cl.Material, m =>
        {
            m.Column(nameof(Material.MaterialId));
            m.Update(false);
        });

        Property(cl => cl.Batch);
        Property(cl => cl.Fifo);
        Property(cl => cl.Quantity);
        Property(cl => cl.Uom);

        Property(cl => cl.ProductionTime);
        Property(cl => cl.AgeBaseline);
        Property(cl => cl.InventoryStatus);

        Set(cl => cl.Allocations, set =>
        {
            set.Inverse(true);
            set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            set.BatchSize(10);
            set.Key(key =>
            {
                key.Column(nameof(UnitloadItem.UnitloadItemId));
                key.NotNullable(true);
                key.Update(false);
            });
        }, rel => rel.OneToMany());

    }
}
