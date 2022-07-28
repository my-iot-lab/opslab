namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseBootstrapBlazorAdmin(this WebApplication builder)
    {
        // 开启健康检查
        builder.MapBootstrapHealthChecks();

        builder.UseBootstrapBlazor();

        builder.UseAuthentication();
        builder.UseAuthorization();

        // 激活 ICacheManager
        builder.Services.GetRequiredService<BootstrapAdmin.Caching.ICacheManager>();

        return builder;
    }
}
