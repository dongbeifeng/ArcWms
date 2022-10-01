using NHibernate.Mapping.ByCode;
using NHibernateUtils;
using ArcWms.Mappings;

namespace ArcWms.Cfg;

internal class OutboundOrderModelMapperConfigurator : IModelMapperConfigurator
{
    public void Configure(ModelMapper mapper)
    {
        mapper.AddMapping<OutboundOrderMapping>();
        mapper.AddMapping<OutboundLineMapping>();
    }
}