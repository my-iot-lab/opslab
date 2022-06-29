namespace Ops.Exchange.Model;

/// <summary>
/// 设备地址变量类型
/// </summary>
public enum VariableType
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
    /// uint16 (16 bits)
    /// </summary>
    Word,

    /// <summary>
    /// uint32 (32 bits)
    /// </summary>
    DWord,

    /// <summary>
    /// int16 (16 bits)
    /// </summary>
    Int,

    /// <summary>
    /// int32 (32 bits)
    /// </summary>
    DInt,

    /// <summary>
    /// float (32 bits)
    /// </summary>
    Real,

    /// <summary>
    /// double (64 bits)
    /// </summary>
    LReal,

    /// <summary>
    /// string
    /// </summary>
    String,

    /// <summary>
    /// 西门子 S7String
    /// </summary>
    S7String,

    /// <summary>
    /// 西门子 S7WString
    /// </summary>
    S7WString,
}
