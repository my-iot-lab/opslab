using Ops.Contrib.Kepware.IotGateway.MQTT;
using Ops.Contrib.Kepware.IotGateway.RESTful;

namespace Ops.Contrib.Kepware;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddIotGatewayRESTful(this IServiceCollection services)
    {
        services.AddHttpClient(RESTServer.HttpClientName, (sp, httpClient) =>
        {
            var options = sp.GetRequiredService<IOptions<RESTfulOptions>>().Value;
            httpClient.BaseAddress = new Uri($"{options.RESTServerBaseAddress}/iotgateway");
        })
        .ConfigureHttpMessageHandlerBuilder(builder =>
        {
            var options = builder.Services.GetRequiredService<IOptions<RESTfulOptions>>().Value;
            if (!options.AllowAnonymous)
            {
                var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate(options.CertificatePath);
                if (builder.PrimaryHandler is HttpClientHandler handler1)
                {
                    handler1.ClientCertificates.Add(certificate);
                }
                else if (builder.PrimaryHandler is SocketsHttpHandler handler2)
                {
                    (handler2.SslOptions.ClientCertificates ??= new()).Add(certificate);
                }
            }
        });

        services.AddTransient<IRESTServerApi, RESTServer>();

        return services;
    }

    public static IServiceCollection AddIotGatewayMQTT(this IServiceCollection services)
    {
        MQTTClientOptions options = new();
        MQTTClientFactory.Create(options);

        return services;
    }
}
