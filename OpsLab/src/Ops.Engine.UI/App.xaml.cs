using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Ops.Exchange.DependencyInjection;
using Ops.Engine.UI.Forwarders;
using Ops.Engine.UI.Config;
using Ops.Engine.UI.ViewModels;

namespace Ops.Engine.UI
{
    public partial class App : Application
    {
        private System.Threading.Mutex? _mutex;
        private IHost? _host;

        public App()
        {
        }

        static IHostBuilder CreateHostBuilder(string[]? args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((builder, services) =>
                {
                    services.AddHttpClient();
                    ConfigureServices(services, builder.Configuration);
                })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                );

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
            // 只允许开启一个
            _mutex = new System.Threading.Mutex(true, "Ops.Engine.UI", out var createdNew);
            if (!createdNew)
            {
                MessageBox.Show("已有一个程序在运行");
                Environment.Exit(0);
                return;
            }

            base.OnStartup(e);

            try
            {
                Log.Information("应用程序启动");
                _host = CreateHostBuilder(null).Build();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }

            _host.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("应用程序关闭退出");
            _host?.Dispose();
            Log.CloseAndFlush();

            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // options
            services.Configure<OpsUIOptions>(configuration.GetSection("OpsUI"));

            services.AddOpsExchange(configuration, options =>
            {
                options.AddNoticeForword<OpsHttpNoticeForwarder>();
                options.AddReplyForword<OpsHttpReplyForwarder>();
            });

            services.AddTransient<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<Home2ViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<AddressViewModel>();
            services.AddTransient<AppDiagnosticViewModel>();
        }
    }
}
