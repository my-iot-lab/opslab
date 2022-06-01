using System.Collections.Concurrent;

namespace Ops.Exchange.Net;

/// <summary>
/// 网络连接池管理
/// </summary>
internal class NetworkConnectionPoolManager
{
    internal readonly struct NetworkConnectionKey : IEquatable<NetworkConnectionKey>
    {
        public readonly string Host;

        public readonly int Port;

        public NetworkConnectionKey(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public bool Equals(NetworkConnectionKey other)
        {
            return Host == other.Host && Port == other.Port;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Host, Port);
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkConnectionKey key && Equals(key);
        }
    }

    private readonly TimeSpan _keepAlivePingDelay;

    private readonly Timer _heartBeatTimer;

    private readonly ConcurrentDictionary<NetworkConnectionKey, NetworkConnectionPool> _pools = new();

    public NetworkConnectionPoolManager()
    {
        long num = (long)_keepAlivePingDelay.TotalMilliseconds;
        var state2 = new WeakReference<NetworkConnectionPoolManager>(this);
        _heartBeatTimer = new Timer(delegate (object state)
        {
            var weakReference = (WeakReference<NetworkConnectionPoolManager>)state;
            if (weakReference.TryGetTarget(out var target))
            {
                target.HeartBeat();
            }
        }, state2, num, num);
    }

    private void HeartBeat()
    {
        foreach (var pool in _pools)
        {
            pool.Value.HeartBeat();
        }
    }
}
