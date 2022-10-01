using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;


internal class OutletMapping : ClassMapping<Outlet>
{
    public OutletMapping()
    {
        Table("Outlets");
        DynamicUpdate(true);
        BatchSize(10);
        Cache(cache => cache.Usage(CacheUsage.ReadWrite));

        Id(cl => cl.OutletId, id => id.Generator(Generators.Identity));

        NaturalId(npm =>
        {
            npm.Property(cl => cl.OutletCode, prop => prop.Update(false));
        }, m => m.Mutable(false));

        Version(cl => cl.v, v => v.Column("v"));

        ManyToOne(cl => cl.KP1, m =>
        {
            m.Column("KP1");
            m.NotNullable(true);
            m.Update(false);
        });

        ManyToOne(cl => cl.KP2, m =>
        {
            m.Column("KP2");
            m.Update(false);
        });
        Property(cl => cl.Comment);


        Set(cl => cl.Streetlets, set =>
        {
            set.Table("STREETLET_OUTLET");
            set.Cache(cache => cache.Usage(CacheUsage.ReadWrite));
            set.BatchSize(10);
            set.Key(key =>
            {
                key.Column("OutletId");
                key.NotNullable(true);
            });
        }, rel =>
        {
            rel.ManyToMany(m2m =>
            {
                m2m.Column("StreetletId");
            });
        });

        // TODO 重命名
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


        Property(cl => cl.LastCheckTime);
        Property(cl => cl.LastCheckMessage);

    }
}
