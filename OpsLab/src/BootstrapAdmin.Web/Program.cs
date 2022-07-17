using Serilog;
using BootstrapAdmin.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR().AddHubOptions<ScadaHub>(options =>
{
    options.MaximumParallelInvocationsPerClient = 10;
});

// 注入项目服务
builder.Services.AddBootstrapBlazorAdmin();

// 添加 Serilog 日志
builder.Host.UseSerilog((ctx, lc) =>
                    lc.ReadFrom.Configuration(ctx.Configuration)
                );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseBootstrapBlazorAdmin();

// 开启 webapi
app.MapDefaultControllerRoute();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// 添加 Hub
app.MapHub<ScadaHub>("/Scada");

app.Run();
