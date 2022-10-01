using ArcWms.Mappings;
using NHibernate.Mapping.ByCode;
using NHibernateUtils;

namespace ArcWms.Cfg;

internal class TransportTaskModelMapperConfigurator : IModelMapperConfigurator
{
    public void Configure(ModelMapper mapper)
    {
        mapper.AddMapping<TransportTaskMapping>();
        mapper.AddMapping<ArchivedTransportTaskMapping>();
    }
}
