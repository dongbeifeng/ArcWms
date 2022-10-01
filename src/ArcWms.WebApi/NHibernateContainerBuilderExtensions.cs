using Autofac;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Event;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;
using NHibernateUtils;
using Serilog.Extensions.Logging;
using System.Security.Principal;

namespace ArcWms.WebApi;

public static class NHibernateContainerBuilderExtensions
{
    class Helper
    {
        private readonly ContainerBuilder _builder;
        private readonly ILogger<Helper> _logger;

        public Helper(ContainerBuilder builder, ILogger<Helper> logger)
        {
            _builder = builder;
            _logger = logger;
        }

        public void Configure()
        {
            _logger.LogInformation("正在配置 NHibernate");

            NHibernateLogger.SetLoggersFactory(new NHibernate.Logging.Serilog.SerilogLoggerFactory());

            _logger.LogInformation("使用 LoggerFactory: {loggerFactory}", typeof(SerilogLoggerFactory));

            _builder.RegisterType<ModelMapper>().SingleInstance();
            _builder.RegisterType<DataAnnotationsModelMapperConfigurator>().As<IModelMapperConfigurator>();

            _builder.Register(c =>
            {
                Configuration configuration = new Configuration();
                configuration.Configure();
                configuration.SetNamingStrategy(ImprovedNamingStrategy.Instance);
                var mapper = c.Resolve<ModelMapper>();

                var modelMapperConfigurators = c.Resolve<IEnumerable<IModelMapperConfigurator>>();
                foreach (var configurator in modelMapperConfigurators)
                {
                    configurator.Configure(mapper);
                }

                var mappings = mapper.CompileMappingForEachExplicitlyAddedEntity();
                //mappings.WriteAllXmlMapping();
                foreach (var mapping in mappings)
                {
                    configuration.AddMapping(mapping);
                }
                _logger.LogInformation("从 ModelMapper 读取了 {0} 个映射", mappings.Count());

                var auxiliaryDatabaseObjects = c.Resolve<IEnumerable<IAuxiliaryDatabaseObject>>();
                foreach (var obj in auxiliaryDatabaseObjects)
                {
                    configuration.AddAuxiliaryDatabaseObject(obj);
                }

                // 开始：nh 事件，检查事务，要求必须打开事务
                CheckTransactionListener checkTransactionListener = new CheckTransactionListener();
                configuration.AppendListeners(ListenerType.PreInsert, new IPreInsertEventListener[] { checkTransactionListener });
                configuration.AppendListeners(ListenerType.PreUpdate, new IPreUpdateEventListener[] { checkTransactionListener });
                configuration.AppendListeners(ListenerType.PreDelete, new IPreDeleteEventListener[] { checkTransactionListener });
                configuration.AppendListeners(ListenerType.PreLoad, new IPreLoadEventListener[] { checkTransactionListener });
                // 结束：nh 事件

                _logger.LogInformation("向 NHibernate.Cfg.Configuration 添加了事件侦听程序 CheckTransactionListener");

                return configuration;

            }).SingleInstance();

            _builder.Register(c =>
            {
                // 生成 SessionFactory
                var configuration = c.Resolve<Configuration>();
                ISessionFactory sessionFactory = configuration.BuildSessionFactory();
                return sessionFactory;
            }).SingleInstance().ExternallyOwned();

            _builder.Register(c =>
            {
                var principal = c.ResolveOptional<IPrincipal>();
                AuditingInterceptor interceptor = new AuditingInterceptor(principal);
                return c.Resolve<ISessionFactory>()
                    .WithOptions()
                    // 添加审计拦截器
                    .Interceptor(interceptor)
                    .OpenSession();
            }).InstancePerLifetimeScope();


            _logger.LogInformation("已配置 NHibernate");
        }

    }


    /// <summary>
    /// 设置 NHiberernate 使用 <see cref="SerilogLoggerFactory"/>；
    /// 使用 hibernate.cfg.xml 配置 NHibernate；
    /// 从容器解析 <see cref="IModelMapperConfigurator"/> 添加到 <see cref="Configuration"/>；
    /// 根据选项添加 <see cref="CheckTransactionListener"/>；
    /// 向容器注册 <see cref="Configuration"/>；
    /// 向容器注册添加了 <see cref="AuditInterceptor"/> 的 <see cref="ISessionFactory"/>；
    /// 向容器注册 <see cref="ISession"/>；
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="loggerFactory">用于记录配置过程的 <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>。</param>
    public static ContainerBuilder AddNHibernate(this ContainerBuilder builder, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        ILogger<Helper> logger = loggerFactory.CreateLogger<Helper>();
        Helper helper = new Helper(builder, logger);
        helper.Configure();
        return builder;
    }

}
