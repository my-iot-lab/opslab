using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using FreeSql;
using Ops.Host.Common.Domain;
using Ops.Host.Core.Services;

namespace Ops.Host.App.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注入 FreeSql
    /// </summary>
    /// <param name="services">服务</param>
    /// <param name="dataType">DB 类型，默认 MySQL</param>
    /// <param name="connectionName">连接字符名称，默认 mes</param>
    /// <returns></returns>
    public static IServiceCollection AddFreeSql(this IServiceCollection services, DataType dataType = DataType.MySql, string connectionName = "mes")
    {
        services.TryAddSingleton(provider =>
        {
            var builder = new FreeSqlBuilder();

            var configuration = provider.GetRequiredService<IConfiguration>();
            var connString = configuration.GetConnectionString(connectionName);
            builder.UseConnectionString(dataType, connString);

#if DEBUG
            ILogger logger = provider.GetRequiredService<ILogger<FreeSqlBuilder>>();

            // 调试sql语句输出
            builder.UseMonitorCommand(cmd =>
            {
                var parameters = cmd.Parameters.OfType<System.Data.Common.DbParameter>().Select(p => $"{p.ParameterName}={p.Value}");
                //System.Console.WriteLine(cmd.CommandText + "\n" + string.Join("&", parameters) + "\n");
                logger.LogInformation(cmd.CommandText);
                logger.LogInformation(string.Join("&", parameters));
            });
#endif

            var instance = builder.Build();
            instance.Mapper();
            return instance;
        });

        // 增加 FreeSql 仓储
        services.AddFreeRepository();

        return services;
    }

    /// <summary>
    /// 添加领域服务对象
    /// </summary>

    public static IServiceCollection AddHostAppServices(this IServiceCollection services)
    {
        // 添加 SCADA 服务
        services.AddSingleton<IAlarmService, AlarmService>();
        services.AddSingleton<INoticeService, NoticeService>();
        services.AddSingleton<IInboundService, InboundService>();
        services.AddSingleton<IArchiveService, ArchiveService>();
        services.AddSingleton<IMaterialService, MaterialService>();
        services.AddSingleton<ICustomService, CustomService>();

        // 添加自定义服务
        var types = typeof(IAlarmService).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface && typeof(IDomainService).IsAssignableFrom(t));
        foreach (var type in types)
        {
            var interfaceType = type.GetInterfaces().FirstOrDefault(t => !typeof(IDomainService).IsAssignableFrom(t));
            if (interfaceType != null)
            {
                services.AddTransient(interfaceType, type);
            }
        }

        return services;
    }
}
