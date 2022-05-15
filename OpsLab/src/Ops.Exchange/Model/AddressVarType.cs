namespace Ops.Exchange.Model;

/// <summary>
/// 地址变量类型
/// </summary>
public enum AddressVarType
{
    /// <summary>
    /// bool
    /// </summary>
    Bit,

    /// <summary>
    /// byte (8 bits)
    /// </summary>
    Byte,

    /// <summary>
    /// ushort (16 bits)
    /// </summary>
    Word,

    /// <summary>
    /// Double Word, ulong  (64 bits)
    /// </summary>
    DWord,

    /// <summary>
    /// ushort (16 bits)
    /// </summary>
    Int,

    /// <summary>
    /// long (32 bits)
    /// </summary>
    DInt,

    /// <summary>
    /// float (32 bits)
    /// </summary>
    Real,

    /// <summary>
    /// Double (64 bits)
    /// </summary>
    LReal,

    /// <summary>
    /// string (include S7String, S7WString）
    /// </summary>
    String,

    /// <summary>
    /// Timer variable type
    /// </summary>
    Timer,

    /// <summary>
    /// Counter
    /// </summary>
    Counter,

    /// <summary>
    /// DateTIme
    /// </summary>
    /// <summary>
    DateTime,

    /// DateTimeLong
    /// </summary>
    DateTimeLong,
}
