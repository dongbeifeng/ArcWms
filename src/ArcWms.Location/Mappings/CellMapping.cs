using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;


internal class CellMapping : ClassMapping<Cell>
{
    public CellMapping()
    {
        Table("Cells");
        DynamicUpdate(true);
        BatchSize(100);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.CellId, id => id.Generator(Generators.Identity));

        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        Set(cl => cl.Locations, set =>
        {
            set.Inverse(true);
            set.BatchSize(10);
            set.Cache(cache => cache.Usage(CacheUsage.ReadWrite));
            set.Key(key =>
            {
                key.Column("CellId");
                key.Update(false);
            });
        }, rel => rel.OneToMany());


        Property(cl => cl.i1);
        Property(cl => cl.i2);
        Property(cl => cl.i3);

        Property(cl => cl.o1);
        Property(cl => cl.o2);
        Property(cl => cl.o3);
    }
}
