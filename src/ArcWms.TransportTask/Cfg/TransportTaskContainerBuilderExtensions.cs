using Autofac;
using Microsoft.Extensions.Logging;

namespace ArcWms.Cfg;

public static class TransportTaskContainerBuilderExtensions
{
    public static ContainerBuilder AddTransportTask(this ContainerBuilder builder, Action<TransportTaskModule> configure, ILoggerFactory loggerFactory)
    {
        TransportTaskModule module = new TransportTaskModule(loggerFactory.CreateLogger<TransportTaskModule>());
        configure?.Invoke(module);
        builder.RegisterModule(module);
        return builder;
    }
}
