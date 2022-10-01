#if SETUP

using ArcWms.WebApi;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using System.CommandLine;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

try
{
    Startup startup = new Startup(configuration);
    startup.ConfigureServices(builder.Services);

    // Using a custom DI container.
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(startup.ConfigureContainer);

    builder.Host.ConfigureAppConfiguration(b => b.AddConfiguration(configuration));

    builder.Services.AddScoped<SetupCommandBuilder>();
    builder.Services.AddScoped<SetupHelper>();
    var app = builder.Build();

    var setupCommandBuilder = app.Services.GetRequiredService<SetupCommandBuilder>();
    await setupCommandBuilder.Build().InvokeAsync(args);

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine();
    Console.WriteLine(ex.ToString());
    Console.WriteLine();
}

#endif