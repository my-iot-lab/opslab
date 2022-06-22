namespace Ops.Exchange.Adapter;

/// <summary>
/// 设备驱动写入数据
/// </summary>
public interface IDriverWriter
{
    Result Write(string address, byte[] value);

    Result Write(string address, bool value);

    Result Write(string address, bool[] values);

    Result Write(string address, short value);

    Result Write(string address, short[] values);

    Result Write(string address, ushort value);

    Result Write(string address, ushort[] values);

    Result Write(string address, int value);

    Result Write(string address, int[] values);

    Result Write(string address, uint value);

    Result Write(string address, uint[] values);

    Result Write(string address, long value);

    Result Write(string address, long[] values);

    Result Write(string address, ulong value);

    Result Write(string address, ulong[] values);

    Result Write(string address, float value);

    Result Write(string address, float[] values);

    Result Write(string address, double value);

    Result Write(string address, double[] values);

    Result Write(string address, string value);

    Result Write(string address, string value, ushort length);
}
