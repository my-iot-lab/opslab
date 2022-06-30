using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ops.Exchange.DependencyInjection;
using Ops.Test.ConsoleApp.Suits;
using Serilog;

string logformat = @"{Timestamp:yyyy-MM-dd HH:mm:ss }[{Level:u3}] {Message:lj}{NewLine}{Exception}";
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

    var modbusTcpSuit = host.Services.GetRequiredService<ModbusTcpSuit>();
    await modbusTcpSuit.InitAsync();
    await modbusTcpSuit.RunAsync();

    Console.ReadLine();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}

static IHostBuilder CreateHostBuilder(string[]? args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    ConfigureServices(services);
                })
                .UseSerilog();

static void ConfigureServices(IServiceCollection services)
{
    // 注册后台服务
    //services.AddHostedService<Worker>();

    // 注入 Services

    // 注入 ViewModels
    // 示例如下：
    // public ContactsView()
    // {
    //    this.InitializeComponent();
    //    this.DataContext = App.Current.Services.GetService<ContactsViewModel>();
    // }
    //

    services.AddOpsExchange();

    services.AddTransient<ModbusTcpSuit>();
}