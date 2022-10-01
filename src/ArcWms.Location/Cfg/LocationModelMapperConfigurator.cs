using ArcWms.Mappings;
using NHibernate.Mapping.ByCode;
using NHibernateUtils;

namespace ArcWms.Cfg;

internal class LocationModelMapperConfigurator : IModelMapperConfigurator
{
    public void Configure(ModelMapper mapper)
    {
        mapper.AddMapping<CellMapping>();
        mapper.AddMapping<StreetletMapping>();
        mapper.AddMapping<LocationMapping>();
        mapper.AddMapping<OutletMapping>();
    }
}
