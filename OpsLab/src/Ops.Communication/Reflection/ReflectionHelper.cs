using System.Linq.Expressions;
using System.Reflection;
using Ops.Communication.Attributes;
using Ops.Communication.Core;

namespace Ops.Communication.Reflection;

/// <summary>
/// 反射的辅助类
/// </summary>
public class ReflectionHelper
{
	/// <summary>
	/// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="DeviceAddressAttribute" />，详细参考论坛的操作说明。
	/// </summary>
	/// <typeparam name="T">自定义的数据类型对象</typeparam>
	/// <param name="readWrite">读写接口的实现</param>
	/// <returns>包含是否成功的结果对象</returns>
	public static OperateResult<T> Read<T>(IReadWriteNet readWrite) where T : class, new()
	{
		Type typeFromHandle = typeof(T);
		var obj = new T();
		PropertyInfo[] properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(DeviceAddressAttribute), inherit: false);
			if (customAttributes == null)
			{
				continue;
			}

			DeviceAddressAttribute? attr = null;
			for (int j = 0; j < customAttributes.Length; j++)
			{
				var attr2 = (DeviceAddressAttribute)customAttributes[j];
				if (attr2.DeviceType != null && attr2.DeviceType == readWrite.GetType())
				{
					attr = attr2;
					break;
				}
			}

			if (attr == null)
			{
				for (int k = 0; k < customAttributes.Length; k++)
				{
					var attr3 = (DeviceAddressAttribute)customAttributes[k];
					if (attr3.DeviceType == null)
					{
						attr = attr3;
						break;
					}
				}
			}

			if (attr == null)
			{
				continue;
			}

			Type propertyType = propertyInfo.PropertyType;
			if (propertyType == typeof(short))
			{
				OperateResult<short> operateResult = readWrite.ReadInt16(attr.Address);
				if (!operateResult.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult);
				}
				propertyInfo.SetValue(obj, operateResult.Content, null);
			}
			else if (propertyType == typeof(short[]))
			{
				var operateResult2 = readWrite.ReadInt16(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult2.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult2);
				}
				propertyInfo.SetValue(obj, operateResult2.Content, null);
			}
			else if (propertyType == typeof(ushort))
			{
				OperateResult<ushort> operateResult3 = readWrite.ReadUInt16(attr.Address);
				if (!operateResult3.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult3);
				}
				propertyInfo.SetValue(obj, operateResult3.Content, null);
			}
			else if (propertyType == typeof(ushort[]))
			{
				var operateResult4 = readWrite.ReadUInt16(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult4.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult4);
				}
				propertyInfo.SetValue(obj, operateResult4.Content, null);
			}
			else if (propertyType == typeof(int))
			{
				OperateResult<int> operateResult5 = readWrite.ReadInt32(attr.Address);
				if (!operateResult5.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult5);
				}
				propertyInfo.SetValue(obj, operateResult5.Content, null);
			}
			else if (propertyType == typeof(int[]))
			{
				var operateResult6 = readWrite.ReadInt32(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult6.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult6);
				}
				propertyInfo.SetValue(obj, operateResult6.Content, null);
			}
			else if (propertyType == typeof(uint))
			{
				OperateResult<uint> operateResult7 = readWrite.ReadUInt32(attr.Address);
				if (!operateResult7.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult7);
				}
				propertyInfo.SetValue(obj, operateResult7.Content, null);
			}
			else if (propertyType == typeof(uint[]))
			{
				OperateResult<uint[]> operateResult8 = readWrite.ReadUInt32(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult8.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult8);
				}
				propertyInfo.SetValue(obj, operateResult8.Content, null);
			}
			else if (propertyType == typeof(long))
			{
				OperateResult<long> operateResult9 = readWrite.ReadInt64(attr.Address);
				if (!operateResult9.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult9);
				}
				propertyInfo.SetValue(obj, operateResult9.Content, null);
			}
			else if (propertyType == typeof(long[]))
			{
				OperateResult<long[]> operateResult10 = readWrite.ReadInt64(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult10.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult10);
				}
				propertyInfo.SetValue(obj, operateResult10.Content, null);
			}
			else if (propertyType == typeof(ulong))
			{
				OperateResult<ulong> operateResult11 = readWrite.ReadUInt64(attr.Address);
				if (!operateResult11.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult11);
				}
				propertyInfo.SetValue(obj, operateResult11.Content, null);
			}
			else if (propertyType == typeof(ulong[]))
			{
				OperateResult<ulong[]> operateResult12 = readWrite.ReadUInt64(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult12.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult12);
				}
				propertyInfo.SetValue(obj, operateResult12.Content, null);
			}
			else if (propertyType == typeof(float))
			{
				OperateResult<float> operateResult13 = readWrite.ReadFloat(attr.Address);
				if (!operateResult13.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult13);
				}
				propertyInfo.SetValue(obj, operateResult13.Content, null);
			}
			else if (propertyType == typeof(float[]))
			{
				OperateResult<float[]> operateResult14 = readWrite.ReadFloat(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult14.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult14);
				}
				propertyInfo.SetValue(obj, operateResult14.Content, null);
			}
			else if (propertyType == typeof(double))
			{
				OperateResult<double> operateResult15 = readWrite.ReadDouble(attr.Address);
				if (!operateResult15.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult15);
				}
				propertyInfo.SetValue(obj, operateResult15.Content, null);
			}
			else if (propertyType == typeof(double[]))
			{
				OperateResult<double[]> operateResult16 = readWrite.ReadDouble(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult16.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult16);
				}
				propertyInfo.SetValue(obj, operateResult16.Content, null);
			}
			else if (propertyType == typeof(string))
			{
				OperateResult<string> operateResult17 = readWrite.ReadString(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult17.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult17);
				}
				propertyInfo.SetValue(obj, operateResult17.Content, null);
			}
			else if (propertyType == typeof(byte[]))
			{
				OperateResult<byte[]> operateResult18 = readWrite.Read(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult18.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult18);
				}
				propertyInfo.SetValue(obj, operateResult18.Content, null);
			}
			else if (propertyType == typeof(bool))
			{
				OperateResult<bool> operateResult19 = readWrite.ReadBool(attr.Address);
				if (!operateResult19.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult19);
				}
				propertyInfo.SetValue(obj, operateResult19.Content, null);
			}
			else if (propertyType == typeof(bool[]))
			{
				OperateResult<bool[]> operateResult20 = readWrite.ReadBool(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!operateResult20.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(operateResult20);
				}
				propertyInfo.SetValue(obj, operateResult20.Content, null);
			}
		}

		return OperateResult.CreateSuccessResult((T)obj);
	}

	/// <summary>
	/// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="DeviceAddressAttribute" />，详细参考论坛的操作说明。
	/// </summary>
	/// <typeparam name="T">自定义的数据类型对象</typeparam>
	/// <param name="data">自定义的数据对象</param>
	/// <param name="readWrite">数据读写对象</param>
	/// <returns>包含是否成功的结果对象</returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static OperateResult Write<T>(T data, IReadWriteNet readWrite) where T : class, new()
	{
		if (data == null)
		{
			throw new ArgumentNullException(nameof(data));
		}

		Type typeFromHandle = typeof(T);
		var properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			var customAttributes = propertyInfo.GetCustomAttributes<DeviceAddressAttribute>(false);
			if (!customAttributes.Any())
			{
				continue;
			}

			var attr = customAttributes.FirstOrDefault(s => s.DeviceType == readWrite.GetType());
			if (attr == null)
			{
				attr = customAttributes.FirstOrDefault(s => s.DeviceType == null);
			}

			if (attr == null)
			{
				continue;
			}

			Type propertyType = propertyInfo.PropertyType;
			if (propertyType == typeof(short))
			{
				short value = (short)propertyInfo.GetValue(data, null);
				OperateResult operateResult = readWrite.Write(attr.Address, value);
				if (!operateResult.IsSuccess)
				{
					return operateResult;
				}
			}
			else if (propertyType == typeof(short[]))
			{
				short[] values = (short[])propertyInfo.GetValue(data, null);
				OperateResult operateResult2 = readWrite.Write(attr.Address, values);
				if (!operateResult2.IsSuccess)
				{
					return operateResult2;
				}
			}
			else if (propertyType == typeof(ushort))
			{
				ushort value2 = (ushort)propertyInfo.GetValue(data, null);
				OperateResult operateResult3 = readWrite.Write(attr.Address, value2);
				if (!operateResult3.IsSuccess)
				{
					return operateResult3;
				}
			}
			else if (propertyType == typeof(ushort[]))
			{
				ushort[] values2 = (ushort[])propertyInfo.GetValue(data, null);
				OperateResult operateResult4 = readWrite.Write(attr.Address, values2);
				if (!operateResult4.IsSuccess)
				{
					return operateResult4;
				}
			}
			else if (propertyType == typeof(int))
			{
				int value3 = (int)propertyInfo.GetValue(data, null);
				OperateResult operateResult5 = readWrite.Write(attr.Address, value3);
				if (!operateResult5.IsSuccess)
				{
					return operateResult5;
				}
			}
			else if (propertyType == typeof(int[]))
			{
				int[] values3 = (int[])propertyInfo.GetValue(data, null);
				OperateResult operateResult6 = readWrite.Write(attr.Address, values3);
				if (!operateResult6.IsSuccess)
				{
					return operateResult6;
				}
			}
			else if (propertyType == typeof(uint))
			{
				uint value4 = (uint)propertyInfo.GetValue(data, null);
				OperateResult operateResult7 = readWrite.Write(attr.Address, value4);
				if (!operateResult7.IsSuccess)
				{
					return operateResult7;
				}
			}
			else if (propertyType == typeof(uint[]))
			{
				uint[] values4 = (uint[])propertyInfo.GetValue(data, null);
				OperateResult operateResult8 = readWrite.Write(attr.Address, values4);
				if (!operateResult8.IsSuccess)
				{
					return operateResult8;
				}
			}
			else if (propertyType == typeof(long))
			{
				long value5 = (long)propertyInfo.GetValue(data, null);
				OperateResult operateResult9 = readWrite.Write(attr.Address, value5);
				if (!operateResult9.IsSuccess)
				{
					return operateResult9;
				}
			}
			else if (propertyType == typeof(long[]))
			{
				long[] values5 = (long[])propertyInfo.GetValue(data, null);
				OperateResult operateResult10 = readWrite.Write(attr.Address, values5);
				if (!operateResult10.IsSuccess)
				{
					return operateResult10;
				}
			}
			else if (propertyType == typeof(ulong))
			{
				ulong value6 = (ulong)propertyInfo.GetValue(data, null);
				OperateResult operateResult11 = readWrite.Write(attr.Address, value6);
				if (!operateResult11.IsSuccess)
				{
					return operateResult11;
				}
			}
			else if (propertyType == typeof(ulong[]))
			{
				ulong[] values6 = (ulong[])propertyInfo.GetValue(data, null);
				OperateResult operateResult12 = readWrite.Write(attr.Address, values6);
				if (!operateResult12.IsSuccess)
				{
					return operateResult12;
				}
			}
			else if (propertyType == typeof(float))
			{
				float value7 = (float)propertyInfo.GetValue(data, null);
				OperateResult operateResult13 = readWrite.Write(attr.Address, value7);
				if (!operateResult13.IsSuccess)
				{
					return operateResult13;
				}
			}
			else if (propertyType == typeof(float[]))
			{
				float[] values7 = (float[])propertyInfo.GetValue(data, null);
				OperateResult operateResult14 = readWrite.Write(attr.Address, values7);
				if (!operateResult14.IsSuccess)
				{
					return operateResult14;
				}
			}
			else if (propertyType == typeof(double))
			{
				double value8 = (double)propertyInfo.GetValue(data, null);
				OperateResult operateResult15 = readWrite.Write(attr.Address, value8);
				if (!operateResult15.IsSuccess)
				{
					return operateResult15;
				}
			}
			else if (propertyType == typeof(double[]))
			{
				double[] values8 = (double[])propertyInfo.GetValue(data, null);
				OperateResult operateResult16 = readWrite.Write(attr.Address, values8);
				if (!operateResult16.IsSuccess)
				{
					return operateResult16;
				}
			}
			else if (propertyType == typeof(string))
			{
				string value9 = (string)propertyInfo.GetValue(data, null);
				OperateResult operateResult17 = readWrite.Write(attr.Address, value9);
				if (!operateResult17.IsSuccess)
				{
					return operateResult17;
				}
			}
			else if (propertyType == typeof(byte[]))
			{
				byte[] value10 = (byte[])propertyInfo.GetValue(data, null);
				OperateResult operateResult18 = readWrite.Write(attr.Address, value10);
				if (!operateResult18.IsSuccess)
				{
					return operateResult18;
				}
			}
			else if (propertyType == typeof(bool))
			{
				bool value11 = (bool)propertyInfo.GetValue(data, null);
				OperateResult operateResult19 = readWrite.Write(attr.Address, value11);
				if (!operateResult19.IsSuccess)
				{
					return operateResult19;
				}
			}
			else if (propertyType == typeof(bool[]))
			{
				bool[] value12 = (bool[])propertyInfo.GetValue(data, null);
				OperateResult operateResult20 = readWrite.Write(attr.Address, value12);
				if (!operateResult20.IsSuccess)
				{
					return operateResult20;
				}
			}
		}
		return OperateResult.CreateSuccessResult(data);
	}

	/// <summary>
	/// 使用表达式树的方式来给一个属性赋值
	/// </summary>
	/// <param name="propertyInfo">属性信息</param>
	/// <param name="obj">对象信息</param>
	/// <param name="objValue">实际的值</param>
	public static void SetPropertyExp<T, K>(PropertyInfo propertyInfo, T obj, K objValue)
	{
		ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "obj");
		ParameterExpression parameterExpression2 = Expression.Parameter(propertyInfo.PropertyType, "objValue");
		MethodCallExpression body = Expression.Call(parameterExpression, propertyInfo.GetSetMethod(), parameterExpression2);
		Expression<Action<T, K>> expression = Expression.Lambda<Action<T, K>>(body, new ParameterExpression[2] { parameterExpression, parameterExpression2 });
		expression.Compile()(obj, objValue);
	}

	/// <summary>
	/// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:HslCommunication.Reflection.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
	/// </summary>
	/// <typeparam name="T">自定义的数据类型对象</typeparam>
	/// <param name="readWrite">读写接口的实现</param>
	/// <returns>包含是否成功的结果对象</returns>
	public static async Task<OperateResult<T>> ReadAsync<T>(IReadWriteNet readWrite) where T : class, new()
	{
		Type type = typeof(T);
		var obj = new T();
		var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo property in properties)
		{
			var attributes = property.GetCustomAttributes<DeviceAddressAttribute>(false);
			if (!attributes.Any())
			{
				continue;
			}

			var attr = attributes.FirstOrDefault(s => s.DeviceType == readWrite.GetType()); ;
			if (attr == null)
			{
				attr = attributes.FirstOrDefault(s => s.DeviceType == null);
			}

			if (attr == null)
			{
				continue;
			}

			Type propertyType = property.PropertyType;
			if (propertyType == typeof(short))
			{
				OperateResult<short> valueResult8 = await readWrite.ReadInt16Async(attr.Address);
				if (!valueResult8.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult8);
				}
				property.SetValue(obj, valueResult8.Content, null);
			}
			else if (propertyType == typeof(short[]))
			{
				OperateResult<short[]> valueResult9 = await readWrite.ReadInt16Async(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult9.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult9);
				}
				property.SetValue(obj, valueResult9.Content, null);
			}
			else if (propertyType == typeof(ushort))
			{
				OperateResult<ushort> valueResult12 = await readWrite.ReadUInt16Async(attr.Address);
				if (!valueResult12.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult12);
				}
				property.SetValue(obj, valueResult12.Content, null);
			}
			else if (propertyType == typeof(ushort[]))
			{
				OperateResult<ushort[]> valueResult13 = await readWrite.ReadUInt16Async(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult13.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult13);
				}
				property.SetValue(obj, valueResult13.Content, null);
			}
			else if (propertyType == typeof(int))
			{
				OperateResult<int> valueResult14 = await readWrite.ReadInt32Async(attr.Address);
				if (!valueResult14.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult14);
				}
				property.SetValue(obj, valueResult14.Content, null);
			}
			else if (propertyType == typeof(int[]))
			{
				OperateResult<int[]> valueResult17 = await readWrite.ReadInt32Async(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult17.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult17);
				}
				property.SetValue(obj, valueResult17.Content, null);
			}
			else if (propertyType == typeof(uint))
			{
				OperateResult<uint> valueResult18 = await readWrite.ReadUInt32Async(attr.Address);
				if (!valueResult18.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult18);
				}
				property.SetValue(obj, valueResult18.Content, null);
			}
			else if (propertyType == typeof(uint[]))
			{
				OperateResult<uint[]> valueResult19 = await readWrite.ReadUInt32Async(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult19.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult19);
				}
				property.SetValue(obj, valueResult19.Content, null);
			}
			else if (propertyType == typeof(long))
			{
				OperateResult<long> valueResult20 = await readWrite.ReadInt64Async(attr.Address);
				if (!valueResult20.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult20);
				}
				property.SetValue(obj, valueResult20.Content, null);
			}
			else if (propertyType == typeof(long[]))
			{
				OperateResult<long[]> valueResult16 = await readWrite.ReadInt64Async(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult16.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult16);
				}
				property.SetValue(obj, valueResult16.Content, null);
			}
			else if (propertyType == typeof(ulong))
			{
				OperateResult<ulong> valueResult15 = await readWrite.ReadUInt64Async(attr.Address);
				if (!valueResult15.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult15);
				}
				property.SetValue(obj, valueResult15.Content, null);
			}
			else if (propertyType == typeof(ulong[]))
			{
				OperateResult<ulong[]> valueResult11 = await readWrite.ReadUInt64Async(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult11.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult11);
				}
				property.SetValue(obj, valueResult11.Content, null);
			}
			else if (propertyType == typeof(float))
			{
				OperateResult<float> valueResult10 = await readWrite.ReadFloatAsync(attr.Address);
				if (!valueResult10.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult10);
				}
				property.SetValue(obj, valueResult10.Content, null);
			}
			else if (propertyType == typeof(float[]))
			{
				OperateResult<float[]> valueResult7 = await readWrite.ReadFloatAsync(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult7.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult7);
				}
				property.SetValue(obj, valueResult7.Content, null);
			}
			else if (propertyType == typeof(double))
			{
				OperateResult<double> valueResult6 = await readWrite.ReadDoubleAsync(attr.Address);
				if (!valueResult6.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult6);
				}
				property.SetValue(obj, valueResult6.Content, null);
			}
			else if (propertyType == typeof(double[]))
			{
				OperateResult<double[]> valueResult5 = await readWrite.ReadDoubleAsync(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult5.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult5);
				}
				property.SetValue(obj, valueResult5.Content, null);
			}
			else if (propertyType == typeof(string))
			{
				OperateResult<string> valueResult4 = await readWrite.ReadStringAsync(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult4.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult4);
				}
				property.SetValue(obj, valueResult4.Content, null);
			}
			else if (propertyType == typeof(byte[]))
			{
				OperateResult<byte[]> valueResult3 = await readWrite.ReadAsync(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult3.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult3);
				}
				property.SetValue(obj, valueResult3.Content, null);
			}
			else if (propertyType == typeof(bool))
			{
				OperateResult<bool> valueResult2 = await readWrite.ReadBoolAsync(attr.Address);
				if (!valueResult2.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult2);
				}
				property.SetValue(obj, valueResult2.Content, null);
			}
			else if (propertyType == typeof(bool[]))
			{
				OperateResult<bool[]> valueResult = await readWrite.ReadBoolAsync(attr.Address, (ushort)((attr.Length <= 0) ? 1u : ((uint)attr.Length)));
				if (!valueResult.IsSuccess)
				{
					return OperateResult.CreateFailedResult<T>(valueResult);
				}
				property.SetValue(obj, valueResult.Content, null);
			}
		}
		return OperateResult.CreateSuccessResult((T)obj);
	}

	/// <summary>
	/// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="DeviceAddressAttribute" />，详细参考论坛的操作说明。
	/// </summary>
	/// <typeparam name="T">自定义的数据类型对象</typeparam>
	/// <param name="data">自定义的数据对象</param>
	/// <param name="readWrite">数据读写对象</param>
	/// <returns>包含是否成功的结果对象</returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static async Task<OperateResult> WriteAsync<T>(T data, IReadWriteNet readWrite) where T : class, new()
	{
		if (data == null)
		{
			throw new ArgumentNullException(nameof(data));
		}

		Type type = typeof(T);
		var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo property in properties)
		{
			var attributes = property.GetCustomAttributes<DeviceAddressAttribute>(false);
			if (!attributes.Any())
			{
				continue;
			}

			var attr = attributes.FirstOrDefault(s => s.DeviceType == readWrite.GetType()); ;
			if (attr == null)
			{
				attr = attributes.FirstOrDefault(s => s.DeviceType == null);
			}

			if (attr == null)
			{
				continue;
			}

			Type propertyType = property.PropertyType;
			if (propertyType == typeof(short))
			{
				OperateResult writeResult20 = await readWrite.WriteAsync(value: (short)property.GetValue(data, null), address: attr.Address);
				if (!writeResult20.IsSuccess)
				{
					return writeResult20;
				}
			}
			else if (propertyType == typeof(short[]))
			{
				OperateResult writeResult19 = await readWrite.WriteAsync(values: (short[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult19.IsSuccess)
				{
					return writeResult19;
				}
			}
			else if (propertyType == typeof(ushort))
			{
				OperateResult writeResult18 = await readWrite.WriteAsync(value: (ushort)property.GetValue(data, null), address: attr.Address);
				if (!writeResult18.IsSuccess)
				{
					return writeResult18;
				}
			}
			else if (propertyType == typeof(ushort[]))
			{
				OperateResult writeResult17 = await readWrite.WriteAsync(values: (ushort[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult17.IsSuccess)
				{
					return writeResult17;
				}
			}
			else if (propertyType == typeof(int))
			{
				OperateResult writeResult16 = await readWrite.WriteAsync(value: (int)property.GetValue(data, null), address: attr.Address);
				if (!writeResult16.IsSuccess)
				{
					return writeResult16;
				}
			}
			else if (propertyType == typeof(int[]))
			{
				OperateResult writeResult15 = await readWrite.WriteAsync(values: (int[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult15.IsSuccess)
				{
					return writeResult15;
				}
			}
			else if (propertyType == typeof(uint))
			{
				OperateResult writeResult14 = await readWrite.WriteAsync(value: (uint)property.GetValue(data, null), address: attr.Address);
				if (!writeResult14.IsSuccess)
				{
					return writeResult14;
				}
			}
			else if (propertyType == typeof(uint[]))
			{
				OperateResult writeResult13 = await readWrite.WriteAsync(values: (uint[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult13.IsSuccess)
				{
					return writeResult13;
				}
			}
			else if (propertyType == typeof(long))
			{
				OperateResult writeResult12 = await readWrite.WriteAsync(value: (long)property.GetValue(data, null), address: attr.Address);
				if (!writeResult12.IsSuccess)
				{
					return writeResult12;
				}
			}
			else if (propertyType == typeof(long[]))
			{
				OperateResult writeResult11 = await readWrite.WriteAsync(values: (long[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult11.IsSuccess)
				{
					return writeResult11;
				}
			}
			else if (propertyType == typeof(ulong))
			{
				OperateResult writeResult10 = await readWrite.WriteAsync(value: (ulong)property.GetValue(data, null), address: attr.Address);
				if (!writeResult10.IsSuccess)
				{
					return writeResult10;
				}
			}
			else if (propertyType == typeof(ulong[]))
			{
				OperateResult writeResult9 = await readWrite.WriteAsync(values: (ulong[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult9.IsSuccess)
				{
					return writeResult9;
				}
			}
			else if (propertyType == typeof(float))
			{
				OperateResult writeResult8 = await readWrite.WriteAsync(value: (float)property.GetValue(data, null), address: attr.Address);
				if (!writeResult8.IsSuccess)
				{
					return writeResult8;
				}
			}
			else if (propertyType == typeof(float[]))
			{
				OperateResult writeResult7 = await readWrite.WriteAsync(values: (float[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult7.IsSuccess)
				{
					return writeResult7;
				}
			}
			else if (propertyType == typeof(double))
			{
				OperateResult writeResult6 = await readWrite.WriteAsync(value: (double)property.GetValue(data, null), address: attr.Address);
				if (!writeResult6.IsSuccess)
				{
					return writeResult6;
				}
			}
			else if (propertyType == typeof(double[]))
			{
				OperateResult writeResult5 = await readWrite.WriteAsync(values: (double[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult5.IsSuccess)
				{
					return writeResult5;
				}
			}
			else if (propertyType == typeof(string))
			{
				OperateResult writeResult4 = await readWrite.WriteAsync(value: (string)property.GetValue(data, null), address: attr.Address);
				if (!writeResult4.IsSuccess)
				{
					return writeResult4;
				}
			}
			else if (propertyType == typeof(byte[]))
			{
				OperateResult writeResult3 = await readWrite.WriteAsync(value: (byte[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult3.IsSuccess)
				{
					return writeResult3;
				}
			}
			else if (propertyType == typeof(bool))
			{
				OperateResult writeResult2 = await readWrite.WriteAsync(value: (bool)property.GetValue(data, null), address: attr.Address);
				if (!writeResult2.IsSuccess)
				{
					return writeResult2;
				}
			}
			else if (propertyType == typeof(bool[]))
			{
				OperateResult writeResult = await readWrite.WriteAsync(value: (bool[])property.GetValue(data, null), address: attr.Address);
				if (!writeResult.IsSuccess)
				{
					return writeResult;
				}
			}
		}

		return OperateResult.CreateSuccessResult(data);
	}
}
