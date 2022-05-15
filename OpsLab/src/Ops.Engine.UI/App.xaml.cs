using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using Ops.Engine.UI.Domain.ViewModels;

namespace Ops.Engine.UI
{
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();

            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RTConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:53353/ChatHub")
                .WithAutomaticReconnect()
                .Build();

            //RTConnection.Closed += async (ex) =>
            //{
            //    await Task.Delay(2000);
            //    await RTConnection.StartAsync();
            //};

            // 警告用户连接已丢失
            //RTConnection.Reconnecting += ex =>
            //{
            //    return Task.CompletedTask;
            //};
        }

        /// <summary>
        /// 获取当前正在使用的 <see cref="App"/> 实例。
        /// </summary>
        public new static App Current => (App)Application.Current;

        public HubConnection RTConnection { get; private set; }

        /// <summary>
        /// 获取能解析应用服务的 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            string logformat = @"{Timestamp:yyyy-MM-dd HH:mm:ss }[{Level:u3}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                          .Enrich.FromLogContext()
                          .MinimumLevel.Information()
                          .WriteTo.File("logs\\mlog.log", outputTemplate: logformat, rollingInterval: RollingInterval.Day)
                          .WriteTo.Seq("http://localhost:5341") // seq 日志平台，参考 https://docs.datalust.co/docs/using-serilog
                          .CreateLogger();

            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfiguration configuration = builder.Build();  // 需要显示指定类型

            var services = new ServiceCollection()
                            .AddOptions()
                            .AddSingleton(configuration);

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

            return services.BuildServiceProvider();
        }
    }
}
