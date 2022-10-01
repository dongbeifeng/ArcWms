using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ArcWms.Mappings;


internal class MaterialMapping : ClassMapping<Material>
{
    public MaterialMapping()
    {
        Table("Materials");
        DynamicUpdate(true);
        Lazy(false);
        BatchSize(10);
        DiscriminatorValue(1);

        Id(cl => cl.MaterialId, id => id.Generator(Generators.Identity));
        Discriminator(dm =>
        {
            dm.NotNullable(true);
        });

        NaturalId(npm =>
        {
            npm.Property(cl => cl.MaterialCode);
        });
        Version(cl => cl.v, v => v.Column("v"));

        Property(cl => cl.CreationTime, prop => prop.Update(false));
        Property(cl => cl.CreationUser, prop => prop.Update(false));
        Property(cl => cl.ModificationTime);
        Property(cl => cl.ModificationUser);

        Property(cl => cl.MaterialType);

        Property(cl => cl.Description);
        Property(cl => cl.SpareCode);
        Property(cl => cl.Specification);
        Property(cl => cl.MnemonicCode);

        Property(cl => cl.BatchEnabled);
        Property(cl => cl.MaterialGroup);

        Property(cl => cl.ValidDays);
        Property(cl => cl.StandingTime);
        Property(cl => cl.AbcClass);

        Property(cl => cl.Uom);


        Property(cl => cl.LowerBound);
        Property(cl => cl.UpperBound);

        Property(cl => cl.DefaultQuantity);
        Property(cl => cl.DefaultStorageGroup);

        Property(cl => cl.Comment);
    }
}
