using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernateUtils;

namespace ArcWms.Mappings;

internal class UnitloadSnapshotMapping : ClassMapping<UnitloadSnapshot>
{
    public UnitloadSnapshotMapping()
    {
        Table("UnitloadSnapshots");
        BatchSize(10);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.UnitloadSnapshotId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        Property(cl => cl.SnapshotTime, m => m.Update(false));
        Property(cl => cl.PalletCode, m => m.Update(false));
        Property(cl => cl.CreationTime, m => m.Update(false));
        Property(cl => cl.ModificationTime, m => m.Update(false));
        Property(cl => cl.CreationUser, m => m.Update(false));
        Property(cl => cl.StorageInfo, MapperUtils.MapPropertyOfCompositeUserType<StorageInfo, StorageInfoType>
        (
            nameof(StorageInfo.StorageGroup),
            nameof(StorageInfo.PalletSpecification),
            nameof(StorageInfo.OutFlag),
            nameof(StorageInfo.Weight),
            nameof(StorageInfo.Height)
        ));
        
        Property(cl => cl.HasCountingError, m => m.Update(false));

        Set(cl => cl.Items, set =>
        {
            set.Inverse(true);
            set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            set.BatchSize(10);
            set.Key(key =>
            {
                key.Column(nameof(UnitloadSnapshot.UnitloadSnapshotId));
                key.NotNullable(true);
                key.Update(false);
            });
        }, rel => rel.OneToMany());

        Property(cl => cl.Comment, m => m.Update(false));
    }
}
