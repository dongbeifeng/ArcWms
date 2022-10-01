#if !SETUP

using ArcWms.WebApi;
using Autofac.Extensions.DependencyInjection;
using Autofac;
// using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    // .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("程序正在启动");
    Startup startup = new Startup(configuration);
    startup.ConfigureServices(builder.Services);

    // Using a custom DI container.
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(startup.ConfigureContainer);

//    builder.Host.UseWindowsService();
    builder.Host.ConfigureAppConfiguration(b => b.AddConfiguration(configuration));
    builder.Host.UseSerilog();

    var app = builder.Build();
    startup.Configure(app);
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "程序意外停止");
}
finally
{
    Log.CloseAndFlush();
}

#endif