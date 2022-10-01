using ArcWms.Mappings;
using NHibernate.Mapping.ByCode;
using NHibernateUtils;

namespace ArcWms.Cfg;

internal class InboundOrderModelMapperConfigurator : IModelMapperConfigurator
{
    public void Configure(ModelMapper mapper)
    {
        mapper.AddMapping<InboundOrderMapping>();
        mapper.AddMapping<InboundLineMapping>();
    }
}
