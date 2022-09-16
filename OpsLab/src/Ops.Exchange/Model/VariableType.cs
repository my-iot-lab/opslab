namespace Ops.Exchange.Model;

/// <summary>
/// 设备地址变量类型
/// </summary>
public enum VariableType
{
    /// <summary>
    /// bool
    /// </summary>
    [Description("Bit")]
    Bit = 1,

    /// <summary>
    /// byte (8 bits)
    /// </summary>
    [Description("Byte")]
    Byte,

    /// <summary>
    /// uint16 (16 bits)
    /// </summary>
    [Description("Word")]
    Word,

    /// <summary>
    /// uint32 (32 bits)
    /// </summary>
    [Description("DWord")]
    DWord,

    /// <summary>
    /// int16 (16 bits)
    /// </summary>
    [Description("Int")]
    Int,

    /// <summary>
    /// int32 (32 bits)
    /// </summary>
    [Description("常规")]
    DInt,

    /// <summary>
    /// float (32 bits)
    /// </summary>
    [Description("Real")]
    Real,

    /// <summary>
    /// double (64 bits)
    /// </summary>
    [Description("LReal")]
    LReal,

    /// <summary>
    /// string
    /// </summary>
    [Description("String")]
    String,

    /// <summary>
    /// 西门子 S7String
    /// </summary>
    [Description("常规S7String")]
    S7String,

    /// <summary>
    /// 西门子 S7WString
    /// </summary>
    [Description("S7WString")]
    S7WString,
}
