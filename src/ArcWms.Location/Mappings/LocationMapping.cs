using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;

internal class LocationMapping : ClassMapping<Location>
{
    public LocationMapping()
    {
        Table("Locations");
        DynamicUpdate(true);
        BatchSize(10);
        Lazy(false);
        DiscriminatorValue(1);

        Id(cl => cl.LocationId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });
        NaturalId(npm =>
        {
            npm.Property(cl => cl.LocationCode, prop => prop.Update(false));
        }, m => m.Mutable(false));

        Version(cl => cl.v, v => v.Column("v"));

        Property(cl => cl.CreationTime, prop => prop.Update(false));
        Property(cl => cl.ModificationTime);

        Property(cl => cl.LocationType, prop => prop.Update(false));

        Property(cl => cl.InboundCount);
        Property(cl => cl.InboundLimit);
        Property(cl => cl.IsInboundDisabled);
        Property(cl => cl.InboundDisabledComment);

        Property(cl => cl.OutboundCount);
        Property(cl => cl.OutboundLimit);
        Property(cl => cl.IsOutboundDisabled);
        Property(cl => cl.OutboundDisabledComment);

        Property(cl => cl.Exists);
        Property(cl => cl.WeightLimit);
        Property(cl => cl.HeightLimit);
        Property(cl => cl.Specification);

        ManyToOne(cl => cl.Streetlet, m =>
        {
            m.Column("StreetletId");
            m.Update(false);
        });
        Property(cl => cl.Side);
        Property(cl => cl.Deep);

        Property(cl => cl.Bay, prop => prop.Update(false));
        Property(cl => cl.Level, prop => prop.Update(false));
        Property(cl => cl.StorageGroup);

        Property(cl => cl.UnitloadCount);

        ManyToOne(cl => cl.Cell, m =>
        {
            m.Column("CellId");
            m.Update(false);
        });

        Property(cl => cl.Tag);
        Property(cl => cl.RequestType);

    }
}
