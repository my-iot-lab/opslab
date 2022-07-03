using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Ops.Engine.App;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                        .CreateBootstrapLogger();

        try
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shut down complete");
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateWebHostBuilder(string[] args)
    {
        return
            Host.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((hostingContext, config) =>
              {
                  config.AddInMemoryCollection(new Dictionary<string, string> { { "HostRoot", hostingContext.HostingEnvironment.ContentRootPath } });
              })
             .ConfigureLogging((hostingContext, logging) =>
             {
                 //logging.ClearProviders();
                 //logging.AddConsole();
                 //logging.AddWTMLogger(); // 用 Serilog 代替 WTMLogger
             })
            .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.UseStartup<Startup>();
             })
            .UseSerilog((ctx, lc) =>
                lc.ReadFrom.Configuration(ctx.Configuration)
            );
    }
}
