using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;


internal class ArchivedTransportTaskMapping : ClassMapping<ArchivedTransportTask>
{
    public ArchivedTransportTaskMapping()
    {
        Table("ArchivedTransportTasks");
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

        Property(cl => cl.TaskType);
        Property(cl => cl.CreationTime, prop => prop.Update(false));

        ManyToOne(cl => cl.Unitload, m =>
        {
            m.Column("UnitloadId");
            m.Cascade(Cascade.All | Cascade.DeleteOrphans);
        });
        ManyToOne(cl => cl.Start, m =>
        {
            m.Column("StartLocationId");
        });
        ManyToOne(cl => cl.End, m =>
        {
            m.Column("EndLocationId");
        });
        ManyToOne(cl => cl.ActualEnd, m =>
        {
            m.Column("ActualEndLocationId");
        });

        Property(cl => cl.WasSent);
        Property(cl => cl.SendTime);
        Property(cl => cl.OrderCode);
        Property(cl => cl.OrderLine);
        Property(cl => cl.Comment);
        Property(cl => cl.ArchivedAt);
        Property(cl => cl.Cancelled);
    }
}
