using Autofac.Extensions.DependencyInjection;
using News;
using NLog;
using NLog.Web;
using System;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");
try
{
    var host = Host.CreateDefaultBuilder(args)
       .UseServiceProviderFactory(new AutofacServiceProviderFactory())
       .ConfigureWebHostDefaults(webHostBuilder => {
           webHostBuilder
              .UseContentRoot(Directory.GetCurrentDirectory())
              .UseIISIntegration()
              .ConfigureLogging(logging =>
              {
                  logging.ClearProviders();
                  logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
              })
              .UseNLog()
              .UseStartup<Startup>();
       })
       .Build();

    host.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
