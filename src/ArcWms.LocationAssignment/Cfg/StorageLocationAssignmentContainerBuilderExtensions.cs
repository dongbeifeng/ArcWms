using Autofac;
using Microsoft.Extensions.Logging;

namespace ArcWms.Cfg;

public static class StorageLocationAssignmentContainerBuilderExtensions
{
    public static ContainerBuilder AddStorageLocationAssignment(this ContainerBuilder builder, Action<StorageLocationAssignmentModule> configure, ILoggerFactory loggerFactory)
    {
        StorageLocationAssignmentModule module = new StorageLocationAssignmentModule(loggerFactory.CreateLogger<StorageLocationAssignmentModule>());
        configure?.Invoke(module);
        builder.RegisterModule(module);
        return builder;
    }
}


