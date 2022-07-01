namespace Ops.Exchange.Model;

/// <summary>
/// 设备驱动类型，如西门子1200、西门子1500、三菱、欧姆龙FinsTcp、AB CIP等。
/// </summary>
public enum DriverModel
{
    ModbusTcp = 1,

    S7_1500,

    S7_1200,

    S7_400,

    S7_300,

    S7_S200,

    S7_S200Smart,

    Melsec_A1E,

    Melsec_CIP,

    Melsec_MC,

    Melsec_MCR,

    Omron_FinsTcp,

    AllenBradley_CIP,
}
