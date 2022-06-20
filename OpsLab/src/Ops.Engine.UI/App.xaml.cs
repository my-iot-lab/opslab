using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Ops.Engine.UI.Domain.ViewModels;
using Ops.Engine.UI.BackgroundServices;

namespace Ops.Engine.UI
{
    public partial class App : Application
    {
        private readonly IHost? _host;

        public App()
        {
            string logformat = @"{Timestamp:yyyy-MM-dd HH:mm:ss }[{Level:u3}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                          .Enrich.FromLogContext()
                          .MinimumLevel.Information()
                          .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                          .WriteTo.File("logs\\mlog.log", outputTemplate: logformat, rollingInterval: RollingInterval.Day)
                          .WriteTo.Seq("http://localhost:5341") // seq 日志平台，参考 https://docs.datalust.co/docs/using-serilog
                          .CreateLogger();

            try
            {
                Log.Information("应用程序启动");
                _host = CreateHostBuilder(null).Build();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
        }

        static IHostBuilder CreateHostBuilder(string[]? args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    ConfigureServices(services);
                })
                .UseSerilog();

        /// <summary>
        /// 获取当前正在使用的 <see cref="App"/> 实例。
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// 获取能解析应用服务的 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        public IServiceProvider Services => _host!.Services;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("应用程序关闭退出");
            //_host?.StopAsync().GetAwaiter().GetResult();
            _host?.Dispose();
            Log.CloseAndFlush();

            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // 注册后台服务
            services.AddHostedService<Worker>();

            // 注入 Services

            // 注入 ViewModels
            // 示例如下：
            // public ContactsView()
            // {
            //    this.InitializeComponent();
            //    this.DataContext = App.Current.Services.GetService<ContactsViewModel>();
            // }
            //
            services.AddTransient<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<LoginViewModel>();
        }
    }
}
