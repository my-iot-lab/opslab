namespace Ops.Communication;

internal static class OpsSecurity
{
	/// <summary>
	/// 加密方法
	/// </summary>
	/// <param name="enBytes">等待加密的数据</param>
	/// <returns>加密后的字节数据</returns>
	internal static byte[] ByteEncrypt(byte[] enBytes)
	{
		byte[] array = new byte[enBytes.Length];
		for (int i = 0; i < enBytes.Length; i++)
		{
			array[i] = (byte)(enBytes[i] ^ 0xB5u);
		}
		return array;
	}

	internal static void ByteEncrypt(byte[] enBytes, int offset, int count)
	{
		for (int i = offset; i < offset + count && i < enBytes.Length; i++)
		{
			enBytes[i] = (byte)(enBytes[i] ^ 0xB5u);
		}
	}

	/// <summary>
	/// 解密方法，只对当前的程序集开放
	/// </summary>
	/// <param name="deBytes">等待解密的数据</param>
	/// <returns>解密后的字节数据</returns>
	internal static byte[] ByteDecrypt(byte[] deBytes)
	{
		return ByteEncrypt(deBytes);  // 思考：byte ^ 0xB5u 加密解密原理是什么？
	}
}