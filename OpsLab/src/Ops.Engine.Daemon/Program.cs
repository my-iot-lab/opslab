using Serilog;
using Ops.Engine.Daemon.Hubs;
using Ops.Engine.Daemon.HostedServices;
using Ops.Engine.Daemon.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // 使用 Serilog 记录日志
    builder.Host.UseSerilog((ctx, lc) =>
                lc.ReadFrom.Configuration(ctx.Configuration)
            );

    builder.Services.AddCors();
    builder.Services.AddSignalR();
    builder.Services.AddHostedService<MonitorLoopHostedService>();
    builder.Services.AddDomainServices();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseStaticFiles();
    app.UseCors();

    app.MapRouters();
    app.MapHub<OpsExchangeHub>("Exchange");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
