using ArcWms.Mappings;
using NHibernate.Mapping.ByCode;
using NHibernateUtils;

namespace ArcWms.Cfg;

internal class MaterialModelMapperConfigurator : IModelMapperConfigurator
{
    public void Configure(ModelMapper mapper)
    {
        mapper.AddMapping<MaterialMapping>();
        mapper.AddMapping<FlowMapping>();
    }
}
