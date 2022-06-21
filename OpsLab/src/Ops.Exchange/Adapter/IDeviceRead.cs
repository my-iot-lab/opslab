namespace Ops.Exchange.Adapter;

/// <summary>
/// 设备读取数据
/// </summary>
public interface IDeviceRead
{
    Result<byte[]> Read(string address, ushort length);

    Result<bool> ReadBool(string address);

    Result<bool[]> ReadBool(string address, ushort length);

    Result<short> ReadInt16(string address);

    Result<short[]> ReadInt16(string address, ushort length);

    Result<ushort> ReadUInt16(string address);

    Result<ushort[]> ReadUInt16(string address, ushort length);

    Result<int> ReadInt32(string address);

    Result<int[]> ReadInt32(string address, ushort length);

    Result<uint> ReadUInt32(string address);

    Result<uint[]> ReadUInt32(string address, ushort length);

    Result<long> ReadInt64(string address);

    Result<long[]> ReadInt64(string address, ushort length);

    Result<ulong> ReadUInt64(string address);

    Result<ulong[]> ReadUInt64(string address, ushort length);

    Result<float> ReadFloat(string address);

    Result<float[]> ReadFloat(string address, ushort length);

    Result<double> ReadDouble(string address);

    Result<double[]> ReadDouble(string address, ushort length);

    Result<string> ReadString(string address, ushort length);
}
