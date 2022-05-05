using System.IO.Compression;

namespace Ops.Communication.Utils;

/// <summary>
/// 一个负责压缩解压数据字节的类
/// </summary>
public class Zipped
{
	/// <summary>
	/// 压缩字节数据
	/// </summary>
	/// <param name="bytes">等待被压缩的数据</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <returns>压缩之后的字节数据</returns>
	public static byte[] CompressBytes(byte[] bytes)
	{
		if (bytes.Length == 0)
		{
			return Array.Empty<byte>();
		}

		using var memoryStream = new MemoryStream();
		using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
		gZipStream.Write(bytes, 0, bytes.Length);

		return memoryStream.ToArray();
	}

	/// <summary>
	/// 解压压缩后的数据
	/// </summary>
	/// <param name="bytes">压缩后的数据</param>
	/// <exception cref="ArgumentNullException"></exception>
	/// <returns>压缩前的原始字节数据</returns>
	public static byte[] Decompress(byte[] bytes)
	{
		if (bytes.Length == 0)
		{
			return Array.Empty<byte>();
		}

		using var stream = new MemoryStream(bytes);
		using var gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using var memoryStream = new MemoryStream();
		int num = 1024;
		byte[] buffer = new byte[num];
		int num2 = 0;
		while ((num2 = gZipStream.Read(buffer, 0, num)) > 0)
		{
			memoryStream.Write(buffer, 0, num2);
		}

		return memoryStream.ToArray();
	}
}
