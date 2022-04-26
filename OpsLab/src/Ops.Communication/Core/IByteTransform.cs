using System.Text;

namespace Ops.Communication.Core;

/// <summary>
/// 支持转换器的基础接口，规定了实际的数据类型和字节数组进行相互转换的方法。
/// </summary>
/// <remarks>
/// 所有的设备通讯类都内置了该转换的模型，并且已经配置好数据的高地位模式，可以方便的转换信息
/// </remarks>
public interface IByteTransform
{
	/// <summary>
	/// 获取或设置数据解析的格式，可选ABCD, BADC，CDAB，DCBA格式，对int,uint,float,double,long,ulong类型有作用
	/// </summary>
	DataFormat DataFormat { get; set; }

	/// <summary>
	/// 获取或设置在解析字符串的时候是否将字节按照字单位反转
	/// </summary>
	bool IsStringReverseByteWord { get; set; }

	/// <summary>
	/// 从缓存中提取出bool结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">位的索引</param>
	/// <returns>bool对象</returns>
	bool TransBool(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取出bool数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">位的索引</param>
	/// <param name="length">bool长度</param>
	/// <returns>bool数组</returns>
	bool[] TransBool(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取byte结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <returns>byte对象</returns>
	byte TransByte(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取byte数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>byte数组对象</returns>
	byte[] TransByte(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取short结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <returns>short对象</returns>
	short TransInt16(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取short数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>short数组对象</returns>
	short[] TransInt16(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取short二维数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="row">二维数组行</param>
	/// <param name="col">二维数组列</param>
	/// <returns>二维short数组</returns>
	short[,] TransInt16(byte[] buffer, int index, int row, int col);

	/// <summary>
	/// 从缓存中提取ushort结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <returns>ushort对象</returns>
	ushort TransUInt16(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取ushort数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>ushort数组对象</returns>
	ushort[] TransUInt16(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取ushort二维数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="row">二维数组行</param>
	/// <param name="col">二维数组列</param>
	/// <returns>二维ushort数组</returns>
	ushort[,] TransUInt16(byte[] buffer, int index, int row, int col);

	/// <summary>
	/// 从缓存中提取int结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <returns>int对象</returns>
	int TransInt32(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取int数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>int数组对象</returns>
	int[] TransInt32(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取int二维数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="row">二维数组行</param>
	/// <param name="col">二维数组列</param>
	/// <returns>二维int数组</returns>
	int[,] TransInt32(byte[] buffer, int index, int row, int col);

	/// <summary>
	/// 从缓存中提取uint结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <returns>uint对象</returns>
	uint TransUInt32(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取uint数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>uint数组对象</returns>
	uint[] TransUInt32(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取uint二维数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="row">二维数组行</param>
	/// <param name="col">二维数组列</param>
	/// <returns>uint二维数组对象</returns>
	uint[,] TransUInt32(byte[] buffer, int index, int row, int col);

	/// <summary>
	/// 从缓存中提取long结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <returns>long对象</returns>
	long TransInt64(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取long数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>long数组对象</returns>
	long[] TransInt64(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取long二维数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="row">二维数组行</param>
	/// <param name="col">二维数组列</param>
	/// <returns>long二维数组对象</returns>
	long[,] TransInt64(byte[] buffer, int index, int row, int col);

	/// <summary>
	/// 从缓存中提取ulong结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <returns>ulong对象</returns>
	ulong TransUInt64(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取ulong数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>ulong数组对象</returns>
	ulong[] TransUInt64(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取ulong二维数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="row">二维数组行</param>
	/// <param name="col">二维数组列</param>
	/// <returns>ulong二维数组对象</returns>
	ulong[,] TransUInt64(byte[] buffer, int index, int row, int col);

	/// <summary>
	/// 从缓存中提取float结果
	/// </summary>
	/// <param name="buffer">缓存对象</param>
	/// <param name="index">索引位置</param>
	/// <returns>float对象</returns>
	float TransSingle(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取float数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>float数组</returns>
	float[] TransSingle(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取float二维数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="row">二维数组行</param>
	/// <param name="col">二维数组列</param>
	/// <returns>float二维数组对象</returns>
	float[,] TransSingle(byte[] buffer, int index, int row, int col);

	/// <summary>
	/// 从缓存中提取double结果
	/// </summary>
	/// <param name="buffer">缓存对象</param>
	/// <param name="index">索引位置</param>
	/// <returns>double对象</returns>
	double TransDouble(byte[] buffer, int index);

	/// <summary>
	/// 从缓存中提取double数组结果
	/// </summary>
	/// <param name="buffer">缓存对象</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">读取的数组长度</param>
	/// <returns>double数组</returns>
	double[] TransDouble(byte[] buffer, int index, int length);

	/// <summary>
	/// 从缓存中提取double二维数组结果
	/// </summary>
	/// <param name="buffer">缓存数据</param>
	/// <param name="index">索引位置</param>
	/// <param name="row">二维数组行</param>
	/// <param name="col">二维数组列</param>
	/// <returns>double二维数组对象</returns>
	double[,] TransDouble(byte[] buffer, int index, int row, int col);

	/// <summary>
	/// 从缓存中提取string结果，使用指定的编码
	/// </summary>
	/// <param name="buffer">缓存对象</param>
	/// <param name="encoding">字符串的编码</param>
	/// <returns>string对象</returns>
	string TransString(byte[] buffer, Encoding encoding);

	/// <summary>
	/// 从缓存中提取string结果，使用指定的编码
	/// </summary>
	/// <param name="buffer">缓存对象</param>
	/// <param name="index">索引位置</param>
	/// <param name="length">byte数组长度</param>
	/// <param name="encoding">字符串的编码</param>
	/// <returns>string对象</returns>
	string TransString(byte[] buffer, int index, int length, Encoding encoding);

	/// <summary>
	/// bool变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(bool value);

	/// <summary>
	/// bool数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(bool[] values);

	/// <summary>
	/// byte变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(byte value);

	/// <summary>
	/// short变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(short value);

	/// <summary>
	/// short数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(short[] values);

	/// <summary>
	/// ushort变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(ushort value);

	/// <summary>
	/// ushort数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(ushort[] values);

	/// <summary>
	/// int变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(int value);

	/// <summary>
	/// int数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(int[] values);

	/// <summary>
	/// uint变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(uint value);

	/// <summary>
	/// uint数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(uint[] values);

	/// <summary>
	/// long变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(long value);

	/// <summary>
	/// long数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(long[] values);

	/// <summary>
	/// ulong变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(ulong value);

	/// <summary>
	/// ulong数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(ulong[] values);

	/// <summary>
	/// float变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(float value);

	/// <summary>
	/// float数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(float[] values);

	/// <summary>
	/// double变量转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(double value);

	/// <summary>
	/// double数组变量转化缓存数据
	/// </summary>
	/// <param name="values">等待转化的数组</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(double[] values);

	/// <summary>
	/// 使用指定的编码字符串转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <param name="encoding">字符串的编码方式</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(string value, Encoding encoding);

	/// <summary>
	/// 使用指定的编码字符串转化缓存数据
	/// </summary>
	/// <param name="value">等待转化的数据</param>
	/// <param name="length">转换之后的数据长度</param>
	/// <param name="encoding">字符串的编码方式</param>
	/// <returns>buffer数据</returns>
	byte[] TransByte(string value, int length, Encoding encoding);

	/// <summary>
	/// 根据指定的<see cref="IByteTransform.DataFormat" />格式，来实例化一个新的对象，除了<see cref="IByteTransform.DataFormat" />不同，其他都相同
	/// </summary>
	/// <param name="dataFormat">数据格式</param>
	/// <returns>新的<see cref="IByteTransform" />对象</returns>
	IByteTransform CreateByDateFormat(DataFormat dataFormat);
}
