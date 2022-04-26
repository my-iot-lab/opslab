namespace Ops.Communication.Ethernet.Profinet.Siemens;

/// <summary>
/// Contains the methods to convert between <see cref="DateTime" /> and S7 representation of datetime values.
/// </summary>
/// <remarks>
/// 这部分的代码参考了另一个s7的库，感谢原作者，此处贴出出处，遵循 MIT 协议
/// https://github.com/S7NetPlus/s7netplus
/// </remarks>
public class SiemensDateTime
{
	/// <summary>
	/// The minimum <see cref="T:System.DateTime" /> value supported by the specification.
	/// </summary>
	public static readonly DateTime SpecMinimumDateTime = new(1990, 1, 1);

	/// <summary>
	/// The maximum <see cref="T:System.DateTime" /> value supported by the specification.
	/// </summary>
	public static readonly DateTime SpecMaximumDateTime = new(2089, 12, 31, 23, 59, 59, 999);

	/// <summary>
	/// Parses a <see cref="T:System.DateTime" /> value from bytes.
	/// </summary>
	/// <param name="bytes">Input bytes read from PLC.</param>
	/// <returns>A <see cref="T:System.DateTime" /> object representing the value read from PLC.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the length of
	///   <paramref name="bytes" /> is not 8 or any value in <paramref name="bytes" />
	///   is outside the valid range of values.</exception>
	public static DateTime FromByteArray(byte[] bytes)
	{
		return FromByteArrayImpl(bytes);
	}

	/// <summary>
	/// Parses an array of <see cref="T:System.DateTime" /> values from bytes.
	/// </summary>
	/// <param name="bytes">Input bytes read from PLC.</param>
	/// <returns>An array of <see cref="T:System.DateTime" /> objects representing the values read from PLC.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the length of
	///   <paramref name="bytes" /> is not a multiple of 8 or any value in
	///   <paramref name="bytes" /> is outside the valid range of values.</exception>
	public static DateTime[] ToArray(byte[] bytes)
	{
		if (bytes.Length % 8 != 0)
		{
			throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Length, $"Parsing an array of DateTime requires a multiple of 8 bytes of input data, input data is '{bytes.Length}' long.");
		}

		int num = bytes.Length / 8;
		DateTime[] array = new DateTime[bytes.Length / 8];
		for (int i = 0; i < num; i++)
		{
			array[i] = FromByteArrayImpl(new ArraySegment<byte>(bytes, i * 8, 8).Array);
		}
		return array;
	}

	private static DateTime FromByteArrayImpl(IList<byte> bytes)
	{
		if (bytes.Count != 8)
		{
			throw new ArgumentOutOfRangeException(nameof(bytes), bytes.Count, $"Parsing a DateTime requires exactly 8 bytes of input data, input data is {bytes.Count} bytes long.");
		}

		int year = ByteToYear(bytes[0]);
		int month = AssertRangeInclusive(DecodeBcd(bytes[1]), 1, 12, "month");
		int day = AssertRangeInclusive(DecodeBcd(bytes[2]), 1, 31, "day of month");
		int hour = AssertRangeInclusive(DecodeBcd(bytes[3]), 0, 23, "hour");
		int minute = AssertRangeInclusive(DecodeBcd(bytes[4]), 0, 59, "minute");
		int second = AssertRangeInclusive(DecodeBcd(bytes[5]), 0, 59, "second");
		int num = AssertRangeInclusive(DecodeBcd(bytes[6]), 0, 99, "first two millisecond digits");
		int num2 = AssertRangeInclusive(bytes[7] >> 4, 0, 9, "third millisecond digit");
		int num3 = AssertRangeInclusive(bytes[7] & 0xF, 1, 7, "day of week");
		return new DateTime(year, month, day, hour, minute, second, num * 10 + num2);

		static int AssertRangeInclusive(int input, byte min, byte max, string field)
		{
			if (input < min)
			{
				throw new ArgumentOutOfRangeException(nameof(input), input, $"Value '{input}' is lower than the minimum '{min}' allowed for {field}.");
			}
			if (input > max)
			{
				throw new ArgumentOutOfRangeException(nameof(input), input, $"Value '{input}' is higher than the maximum '{max}' allowed for {field}.");
			}
			return input;
		}

		static int ByteToYear(byte bcdYear)
		{
			int num4 = DecodeBcd(bcdYear);
			if (num4 < 90)
			{
				return num4 + 2000;
			}
			if (num4 >= 100)
			{
				throw new ArgumentOutOfRangeException(nameof(bcdYear), bcdYear, $"Value '{num4}' is higher than the maximum '99' of S7 date and time representation.");
			}
			return num4 + 1900;
		}

		static int DecodeBcd(byte input)
		{
			return 10 * (input >> 4) + (input & 0xF);
		}
	}

	/// <summary>
	/// Converts a <see cref="T:System.DateTime" /> value to a byte array.
	/// </summary>
	/// <param name="dateTime">The DateTime value to convert.</param>
	/// <returns>A byte array containing the S7 date time representation of <paramref name="dateTime" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the value of
	///   <paramref name="dateTime" /> is before <see cref="P:SpecMinimumDateTime" />
	///   or after <see cref="P:SpecMaximumDateTime" />.</exception>
	public static byte[] ToByteArray(DateTime dateTime)
	{
		if (dateTime < SpecMinimumDateTime)
		{
			throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, $"Date time '{dateTime}' is before the minimum '{SpecMinimumDateTime}' supported in S7 date time representation.");
		}
		if (dateTime > SpecMaximumDateTime)
		{
			throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, $"Date time '{dateTime}' is after the maximum '{SpecMaximumDateTime}' supported in S7 date time representation.");
		}

		return new byte[8]
		{
			EncodeBcd(MapYear(dateTime.Year)),
			EncodeBcd(dateTime.Month),
			EncodeBcd(dateTime.Day),
			EncodeBcd(dateTime.Hour),
			EncodeBcd(dateTime.Minute),
			EncodeBcd(dateTime.Second),
			EncodeBcd(dateTime.Millisecond / 10),
			(byte)((dateTime.Millisecond % 10 << 4) | DayOfWeekToInt(dateTime.DayOfWeek))
		};

		static int DayOfWeekToInt(DayOfWeek dayOfWeek)
		{
			return (int)(dayOfWeek + 1);
		}

		static byte EncodeBcd(int value)
		{
			return (byte)((value / 10 << 4) | (value % 10));
		}

		static byte MapYear(int year)
		{
			return (byte)((year < 2000) ? (year - 1900) : (year - 2000));
		}
	}

	/// <summary>
	/// Converts an array of <see cref="T:System.DateTime" /> values to a byte array.
	/// </summary>
	/// <param name="dateTimes">The DateTime values to convert.</param>
	/// <returns>A byte array containing the S7 date time representations of <paramref name="dateTimes" />.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when any value of
	///   <paramref name="dateTimes" /> is before <see cref="P:SpecMinimumDateTime" />
	///   or after <see cref="P:SpecMaximumDateTime" />.</exception>
	public static byte[] ToByteArray(DateTime[] dateTimes)
	{
		var list = new List<byte>(dateTimes.Length * 8);
		foreach (DateTime dateTime in dateTimes)
		{
			list.AddRange(ToByteArray(dateTime));
		}
		return list.ToArray();
	}
}
