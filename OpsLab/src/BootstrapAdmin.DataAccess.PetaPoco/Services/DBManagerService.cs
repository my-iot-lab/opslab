using System.Collections.Specialized;
using System.Data.Common;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BootstrapBlazor.Components;
using PetaPoco;
using PetaPoco.Providers;

namespace BootstrapAdmin.DataAccess.PetaPoco.Services;

/// <summary>
/// DB 管理器
/// </summary>
internal class DBManagerService : IDBManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IWebHostEnvironment _webHost;

    public DBManagerService(IConfiguration configuration, ILogger<DBManagerService> logger, IWebHostEnvironment host)
    {
        _configuration = configuration;
        _logger = logger;
        _webHost = host;
    }

    /// <summary>
    /// 创建 IDatabase 实例方法
    /// </summary>
    /// <param name="connectionName">连接字符串键值, 默认为 "ba"</param>
    /// <param name="keepAlive"></param>
    /// <returns></returns>
    public IDatabase Create(string? connectionName = "ba", bool keepAlive = false)
    {
        var conn = _configuration.GetConnectionString(connectionName) ?? throw new ArgumentNullException(nameof(connectionName));

        var option = DatabaseConfiguration.Build();
        option.UsingDefaultMapper<BootstrapAdminConventionMapper>();

        // connectionstring
        option.UsingConnectionString(conn);

        // provider
        option.UsingProvider<MySqlConnectorDatabaseProvider>();

        var db = new Database(option) { KeepConnectionAlive = keepAlive };

        db.ExceptionThrown += (sender, e) =>
        {
            var message = e.Exception.Format(new NameValueCollection()
            {
                [nameof(db.LastCommand)] = db.LastCommand,
                [nameof(db.LastArgs)] = string.Join(",", db.LastArgs)
            });
            _logger.LogError(new EventId(1001, "GlobalException"), e.Exception, message);
        };

        if (_webHost.IsDevelopment())
        {
            db.CommandExecuted += (sender, args) =>
            {
                var parameters = new StringBuilder();
                foreach (DbParameter p in args.Command.Parameters)
                {
                    parameters.AppendFormat("{0}: {1}  ", p.ParameterName, p.Value);
                }
                _logger.LogInformation(args.Command.CommandText);
                _logger.LogInformation(parameters.ToString());
            };
        };
        return db;
    }
}
