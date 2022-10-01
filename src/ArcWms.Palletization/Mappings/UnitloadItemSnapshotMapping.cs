using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;

internal class UnitloadItemSnapshotMapping : ClassMapping<UnitloadItemSnapshot>
{
    public UnitloadItemSnapshotMapping()
    {
        Table("UnitloadItemSnapshots");
        BatchSize(10);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.UnitloadItemSnapshotId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        ManyToOne(cl => cl.Unitload, m =>
        {
            m.Column(nameof(UnitloadSnapshot.UnitloadSnapshotId));
            m.Update(false);
        });
        ManyToOne(cl => cl.Material, m =>
        {
            m.Column(nameof(Material.MaterialId));
            m.Update(false);
        });

        Property(cl => cl.Batch, m => m.Update(false));
        Property(cl => cl.InventoryStatus, m => m.Update(false));
        Property(cl => cl.Quantity, m => m.Update(false));
        Property(cl => cl.Uom, m => m.Update(false));
        Property(cl => cl.ProductionTime, m => m.Update(false));

    }
}
