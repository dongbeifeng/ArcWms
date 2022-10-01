using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernateUtils;

namespace ArcWms.Mappings;

internal class StreetletMapping : ClassMapping<Streetlet>
{
    public StreetletMapping()
    {
        Table("Streetlets");
        DynamicUpdate(true);
        BatchSize(10);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.StreetletId, id => id.Generator(Generators.Identity));

        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        NaturalId(npm =>
        {
            npm.Property(cl => cl.StreetletCode, prop => prop.Update(false));
        }, m => m.Mutable(false));

        Version(cl => cl.v, v => v.Column("v"));

        Property(cl => cl.Area);

        Property(cl => cl.Comment);

        Set(cl => cl.Locations, set =>
        {
            set.Inverse(true);
            set.BatchSize(10);
            set.Cache(cache => cache.Usage(CacheUsage.ReadWrite));
            set.Key(key =>
            {
                key.Column("StreetletId");
                key.NotNullable(true);
                key.Update(false);
            });
        }, rel => rel.OneToMany());

        Property(cl => cl.IsInboundDisabled);
        Property(cl => cl.InboundDisabledComment);
        Property(cl => cl.IsOutboundDisabled);
        Property(cl => cl.OutboundDisabledComment);

        Property(cl => cl.IsDoubleDeep, prop => prop.Update(false));
        Property(cl => cl.ReservedLocationCount);

        Set(cl => cl.Outlets, set =>
        {
            set.Table("STREETLET_OUTLET");
            set.BatchSize(10);
            set.Cache(cache => cache.Usage(CacheUsage.ReadWrite));
            set.Key(key =>
            {
                key.Column("StreetletId");
                key.NotNullable(true);
                key.Update(false);
            });
        }, rel =>
        {
            rel.ManyToMany(m2m => m2m.Column("OutletId"));
        });


        Map(
            cl => cl.Usage,
            map =>
            {
                map.Table("StreetletUsage");
                map.BatchSize(100);
                map.Cascade(Cascade.All | Cascade.DeleteOrphans);
                map.Key(key =>
                {
                    key.Column("StreetletId");
                    key.NotNullable(true);
                });
            },
            key => key.Element(MapperUtils.MapMapKeyOfCompositeUserType<StreetletUsageKey, StreetletUsageKeyType>
            (
                nameof(StreetletUsageKey.StorageGroup),
                nameof(StreetletUsageKey.Specification),
                nameof(StreetletUsageKey.WeightLimit),
                nameof(StreetletUsageKey.HeightLimit)
            )),          
            
            el => el.Element(MapperUtils.MapElementOfCompositeUserType<StreetletUsageData, StreetletUsageDataType>(
                nameof(StreetletUsageData.Total),
                nameof(StreetletUsageData.Available),
                nameof(StreetletUsageData.Loaded),
                nameof(StreetletUsageData.InboundDisabled)
            ))            
        );
    }
}
