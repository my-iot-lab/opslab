namespace Ops.Scada.Rpc;

/// <summary>
/// 代理对象
/// </summary>
internal class AgentTable
{
    /// <summary>
    /// 代理服务地址
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// 代理服务端口
    /// </summary>
    public int Port { get; }

    public AgentTable(string host, int port)
    {
        Host = host;
        Port = port;
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is not AgentTable agent)
        {
            return false;
        }

        return this.Host == agent.Host && this.Port == agent.Port;
    }

    public override int GetHashCode()
    {
        return this.Host.GetHashCode() ^ this.Port;
    }
}
