﻿using BootstrapAdmin.Web.Core;
using BootstrapAdmin.Web.Core.Services;
using BootstrapAdmin.Web.HealthChecks;
using BootstrapAdmin.Web.Services;
using BootstrapAdmin.Web.Services.SMS;
using BootstrapAdmin.Web.Services.SMS.Tencent;
using BootstrapAdmin.Web.Utils;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBootstrapBlazorAdmin(this IServiceCollection services)
    {
        //services.AddLogging(logging => logging.AddFileLogger()AddDBLogger(ExceptionsHelper.Log));
        services.AddCors();
        services.AddResponseCompression();

        // 增加后台任务
        services.AddTaskServices();
        services.AddHostedService<AdminTaskService>();

        // 增加 健康检查服务（健康检查中有检查 Longbow.lic 文件是否存在）
        //services.AddAdminHealthChecks();

        // 增加 BootstrapBlazor 组件
        services.AddBootstrapBlazor();

        // 增加 Table Excel 导出服务
        services.AddBootstrapBlazorTableExcelExport();

        // 配置地理位置定位器
        //services.ConfigureIPLocatorOption(op => op.LocatorFactory = LocatorHelper.CreateLocator);

        // 增加手机短信服务
        services.AddSingleton<ISMSProvider, TencentSMSProvider>();

        // 增加认证授权服务
        services.AddBootstrapAdminSecurity<AdminService>();

        // 增加 BootstrapApp 上下文服务
        services.AddScoped<BootstrapAppContext>();

        // 增加缓存服务
        services.AddCacheManager();

        // 增加 FreeSql 数据服务
        services.AddFreeSql((provider, builder) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connString = configuration.GetConnectionString("ba");
            builder.UseConnectionString(FreeSql.DataType.MySql, connString); // 此处注入 MySQL，需要时切换

#if DEBUG
            ILogger logger = provider.GetRequiredService<ILogger<FreeSql.FreeSqlBuilder>>();

            // 调试sql语句输出
            builder.UseMonitorCommand(cmd =>
            {
                var parameters = cmd.Parameters.OfType<System.Data.Common.DbParameter>().Select(p => $"{p.ParameterName}={p.Value}");
                //System.Console.WriteLine(cmd.CommandText + "\n" + string.Join("&", parameters) + "\n");
                logger.LogInformation(cmd.CommandText);
                logger.LogInformation(string.Join("&", parameters));
            });
#endif
        });

        // 添加 PetaPoco 数据服务（针对基础服务）
        services.AddPetaPocoDataAccessServices();

        return services;
    }
}
