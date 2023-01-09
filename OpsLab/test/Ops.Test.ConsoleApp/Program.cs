using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ops.Exchange.DependencyInjection;
using Ops.Exchange.Forwarder;
using Ops.Test.ConsoleApp.Forwarder;
using Ops.Test.ConsoleApp.Suits;
using Serilog;

string logformat = @"{Timestamp:yyyy-MM-dd HH:mm:ss:fff }[{Level:u3}] {Message:lj}{NewLine}{Exception}";
Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .MinimumLevel.Information()
              .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
              .WriteTo.Console(outputTemplate: logformat)
              .CreateLogger();

try
{
    Log.Information("控制台应用程序启动");
    var host = CreateHostBuilder(null).Build();
    host.Start();

    //var modbusTcpSuit = host.Services.GetRequiredService<ModbusTcpSuit>();
    //await modbusTcpSuit.InitAsync();
    //await modbusTcpSuit.RunAsync();

    //var s7Suit = host.Services.GetRequiredService<SimaticS7Suit>();
    //await s7Suit.InitAsync();
    //await s7Suit.RunAsync();

    var melsecMCSuit = host.Services.GetRequiredService<MelsecMCSuit>();
    await melsecMCSuit.InitAsync();
    await melsecMCSuit.RunAsync();

    Console.ReadLine();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}

static IHostBuilder CreateHostBuilder(string[]? args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((builder, services) =>
                {
                    ConfigureServices(services, builder.Configuration);
                })
                .UseSerilog();

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // 注册后台服务
    //services.AddHostedService<Worker>();

    services.AddOpsExchange(configuration);

    services.AddTransient<INoticeForwarder, MyNoticeForwarder>();
    services.AddTransient<IReplyForwarder, MyReplyForwarder>();
    services.AddTransient<ISwitchForwarder, MySwitchForwarder>();
    services.AddTransient<IUnderlyForwarder, MyUnderlyForwarder>();

    services.AddTransient<ModbusTcpSuit>();
    services.AddTransient<SimaticS7Suit>();
    services.AddTransient<MelsecMCSuit>();
}