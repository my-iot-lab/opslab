using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Ops.Scada.Engine.Domain.ViewModels;

namespace Ops.Scada.Engine
{
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();

            InitializeComponent();
        }

        /// <summary>
        /// 获取当前正在使用的 <see cref="App"/> 实例。
        /// </summary>
        public new static App Current => (App)Application.Current;

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
