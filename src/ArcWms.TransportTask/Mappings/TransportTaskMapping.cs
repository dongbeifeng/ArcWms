using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;

internal class TransportTaskMapping : ClassMapping<TransportTask>
{
    public TransportTaskMapping()
    {
        Table("TransportTasks");
        DynamicUpdate(true);
        BatchSize(10);
        DiscriminatorValue(1);

        Id(cl => cl.TaskId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });
        NaturalId(npm =>
        {
            npm.Property(cl => cl.TaskCode, prop => prop.Update(false));
        }, m => m.Mutable(false));

        Property(cl => cl.TaskType, prop => prop.Update(false));
        Property(cl => cl.CreationTime, prop => prop.Update(false));

        ManyToOne(cl => cl.Unitload, m =>
        {
            m.Column("UnitloadId");
            m.Update(false);
            m.Unique(true);
        });

        ManyToOne(cl => cl.Start, m =>
        {
            m.Column("StartLocationId");
            m.Update(false);
        });
        ManyToOne(cl => cl.End, m =>
        {
            m.Column("EndLocationId");
        });

        Property(cl => cl.WasSent);
        Property(cl => cl.SendTime);

        Property(cl => cl.OrderCode);
        Property(cl => cl.OrderLine);
        Property(cl => cl.Comment);
    }
}
