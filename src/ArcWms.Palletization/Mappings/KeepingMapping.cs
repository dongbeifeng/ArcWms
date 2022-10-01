using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;

internal class KeepingMapping : ClassMapping<Keeping>
{
    public KeepingMapping()
    {
        Table("Keepings");
        BatchSize(10);


        Id(null, id =>
        {
            id.Generator(Generators.Foreign<Keeping>(x => x.Unitload));
            id.Column("UnitloadId");
        });

        OneToOne(cl => cl.Unitload, o2o =>
        {
            o2o.Constrained(true);
        });

        ManyToOne(cl => cl.Location, m2o =>
        {
            m2o.Column("LocationId");
        });
    }
}
