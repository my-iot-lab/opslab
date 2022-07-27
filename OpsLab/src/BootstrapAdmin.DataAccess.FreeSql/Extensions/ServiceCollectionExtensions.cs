using Microsoft.Extensions.DependencyInjection.Extensions;
using BootStarpAdmin.DataAccess.FreeSql.Extensions;
using FreeSql;
using BootstrapAdmin.Web.Core.Services;
using BootstrapAdmin.DataAccess.FreeSql.Services;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// FreeSql ORM 注入服务扩展类
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注入 FreeSql 数据服务类
    /// </summary>
    /// <param name="services"></param>
    /// <param name="freeSqlBuilder"></param>
    /// <returns></returns>
    public static IServiceCollection AddFreeSql(this IServiceCollection services, Action<IServiceProvider, FreeSqlBuilder> freeSqlBuilder)
    {
        services.TryAddSingleton(provider =>
        {
            var builder = new FreeSqlBuilder();
            freeSqlBuilder(provider, builder);
            var instance = builder.Build();
            instance.Mapper();
            return instance;
        });

        // 增加 FreeSql 仓储
        services.AddFreeRepository();

        // 增加 scada 业务服务
        services.AddTransient<IAlarmService, AlarmService>();
        services.AddTransient<INoticeService, NoticeService>();
        services.AddTransient<IInboundService, InboundService>();
        services.AddTransient<IArchiveService, ArchiveService>();
        services.AddTransient<IMaterialService, MaterialService>();

        // 添加主业务


        return services;
    }
}
