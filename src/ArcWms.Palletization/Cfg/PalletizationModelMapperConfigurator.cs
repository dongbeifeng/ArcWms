using ArcWms.Mappings;
using NHibernate.Mapping.ByCode;
using NHibernateUtils;

namespace ArcWms.Cfg;

internal class PalletizationModelMapperConfigurator : IModelMapperConfigurator
{
    public void Configure(ModelMapper mapper)
    {
        mapper.AddMapping<UnitloadItemAllocationMapping>();
        mapper.AddMapping<UnitloadItemMapping>();
        mapper.AddMapping<UnitloadItemSnapshotMapping>();
        mapper.AddMapping<UnitloadMapping>();
        mapper.AddMapping<UnitloadSnapshotMapping>();
        mapper.AddMapping<KeepingMapping>();
    }
}
