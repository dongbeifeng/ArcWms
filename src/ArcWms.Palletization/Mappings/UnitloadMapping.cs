using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernateUtils;

namespace ArcWms.Mappings;

internal class UnitloadMapping : ClassMapping<Unitload>
{
    public UnitloadMapping()
    {
        Table("Unitloads");
        DynamicUpdate(true);
        BatchSize(10);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.UnitloadId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        NaturalId(npm =>
        {
            npm.Property(c => c.PalletCode, prop => prop.Update(false));
        }, m => m.Mutable(false));

        Version(cl => cl.v, v => v.Column("v"));

        Property(cl => cl.CreationTime, prop => prop.Update(false));
        Property(cl => cl.ModificationTime);
        Property(cl => cl.CreationUser, prop => prop.Update(false));

        Property(cl => cl.StorageInfo, MapperUtils.MapPropertyOfCompositeUserType<StorageInfo, StorageInfoType>
        (
            nameof(StorageInfo.StorageGroup),
            nameof(StorageInfo.PalletSpecification),
            nameof(StorageInfo.OutFlag),
            nameof(StorageInfo.Weight),
            nameof(StorageInfo.Height)
        ));

        Property(cl => cl.HasCountingError);

        Set(cl => cl.Items, set =>
        {
            set.Inverse(true);
            set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            set.BatchSize(10);
            set.Key(key =>
            {
                key.Column(nameof(Unitload.UnitloadId));
                key.NotNullable(true);
                key.Update(false);
            });
        }, rel => rel.OneToMany());

        Property(cl => cl.HasTask);
        ManyToOne(cl => cl.CurrentLocation, m =>
        {
            m.Column("CurrentLocationId");
        });
        Property(cl => cl.CurrentLocationTime);

        Any(cl => cl.CurrentUat, typeof(int), m =>
        {
            m.Lazy(false);
            m.Columns(x =>
            {
                x.Name("CurrentUatId");
            }, x =>
            {
                x.Name("CurrentUatType");
                x.Length(50);
            });
        });

        Property(cl => cl.OpHintType);
        Property(cl => cl.OpHintInfo);
        Property(cl => cl.Comment);
    }
}
