using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Ops.Scada.Rpc;

public class OpsGrpcChannel
{
    public static GrpcChannel New(string addr, HttpMessageHandler httpHandler)
    {
        var options = new GrpcChannelOptions
        {
            HttpHandler = httpHandler,
            DisposeHttpClient = false, // httpHandler 不随 GrpcChannel Dispose 而 Dispose。
        };
        var channel = GrpcChannel.ForAddress(addr, options);
        //channel.Intercept(new ErrorHandlerInterceptor()); // 注册侦听器
        return channel;
    }

    /// <summary>
    /// 创建一个新的<see cref="GrpcChannel"/>实例
    /// </summary>
    /// <param name="addr">远程服务地址</param>
    /// <param name="isDangerousAcceptAnyServerCertificateValidator">使用使用非安全的凭证，例如，可允许不使用 HTTPS 进行访问</param>
    /// <returns></returns>
    public static GrpcChannel New(string addr, bool isDangerousAcceptAnyServerCertificateValidator = false)
    {
        // 新版本 .NET 建议使用 SocketsHttpHandler 对象，而不是 HttpClientHandler 对象。
        var httpHandler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan, // 池中连接的最大空闲时间。
            // 该通道在非活动期间每 60 秒向服务器发送一次保持活动 ping。 ping 确保服务器和使用中的任何代理不会由于不活动而关闭连接。
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
            ConnectTimeout = TimeSpan.FromSeconds(5),
            // 当达到并发流限制时，通道会创建额外的 HTTP/2 连接。
            //  HTTP/2 连接通常会限制一个连接上同时存在的最大并发流（活动 HTTP 请求）数。 默认情况下，大多数服务器将此限制设置为 100 个并发流。
            EnableMultipleHttp2Connections = true,
        };

        if (!isDangerousAcceptAnyServerCertificateValidator)
        {
            httpHandler.SslOptions = new()
            {
                RemoteCertificateValidationCallback = (_, _, _, _) => true, // 允许用不安全的凭证访问
            };
        }

        var options = new GrpcChannelOptions
        {
            HttpHandler = httpHandler,
            DisposeHttpClient = true, // GrpcChannel Dispose 后 httpHandler 也随着 Dispose。
        };
        var channel = GrpcChannel.ForAddress(addr, options);
        //channel.Intercept(new ErrorHandlerInterceptor()); // 注册侦听器
        return channel;
    }

    public class ErrorHandlerInterceptor : Interceptor
    {
        private readonly ILogger _logger;

        public ErrorHandlerInterceptor()
        {

        }

        public ErrorHandlerInterceptor(ILogger<ErrorHandlerInterceptor> logger)
        {
            _logger = logger;
        }
    }
}
