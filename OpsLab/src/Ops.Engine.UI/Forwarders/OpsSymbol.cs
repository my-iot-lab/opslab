namespace Ops.Engine.UI.Forwarders;

/// <summary>
/// 主要变量地址符号定义。
/// 其中 PLC 触发信号都需要以 "PLC_Sign_" 为前缀。 
/// </summary>
public static class OpsSymbol
{
    #region PLC To MES

    /// <summary>
    /// 指定的 PLC 触发信号前缀
    /// </summary>
    public const string PLC_Sign_Prefix = "PLC_Sign_";

    /// <summary>
    /// 通知对象前缀
    /// </summary>
    public const string PLC_Notice_Prefix = "PLC_Notice_";

    #region 基础

    /// <summary>
    /// 心跳信号，INT 类型
    /// </summary>
    public const string PLC_Sys_Heartbeat = nameof(PLC_Sys_Heartbeat);

    /// <summary>
    /// 警报信号。
    /// 采用 DWord 记录，最多同时发生 64 种警报。
    /// </summary>
    public const string PLC_Sys_Alarm = nameof(PLC_Sys_Alarm);

    /// <summary>
    /// Andon 信号，采用 DWord 记录，最多同时发生 64 种警报。
    /// </summary>
    public const string PLC_Sys_Andon = nameof(PLC_Sys_Andon);

    #endregion

    #region 进站

    /// <summary>
    /// 进站请求信号，INT 类型
    /// </summary>
    public const string PLC_Sign_Inbound = nameof(PLC_Sign_Inbound);

    /// <summary>
    /// 程序配方，INT 类型
    /// </summary>
    public const string PLC_Inbound_Formula = nameof(PLC_Inbound_Formula);

    /// <summary>
    /// 进站请求产品码, String[40] 类型。
    /// </summary>
    public const string PLC_Inbound_SN = nameof(PLC_Inbound_SN);

    /// <summary>
    /// 进站请求托盘码, String[40] 类型。
    /// </summary>
    public const string PLC_Inbound_Pallet = nameof(PLC_Inbound_Pallet);

    #endregion

    #region 出站/存档

    /// <summary>
    /// 出站信号，INT 类型。
    /// </summary>
    public const string PLC_Sign_Archive = nameof(PLC_Sign_Archive);

    /// <summary>
    /// 出站的产品码, String[40] 类型。
    /// </summary>
    public const string PLC_Archive_SN = nameof(PLC_Archive_SN);

    /// <summary>
    /// 出站的结果（OK/NG），INT 类型。
    /// </summary>
    public const string PLC_Archive_Pass = nameof(PLC_Archive_Pass);

    /// <summary>
    /// CT 时长，INT 类型。
    /// </summary>
    public const string PLC_Archive_Cycletime = nameof(PLC_Archive_Cycletime);

    /// <summary>
    /// 操作人员， String[20] 类型。
    /// </summary>
    public const string PlC_Archive_Operator = nameof(PlC_Archive_Operator);

    /// <summary>
    /// 班次，int 类型。
    /// </summary>
    public const string PlC_Archive_Shift = nameof(PlC_Archive_Shift);

    /// <summary>
    /// 托盘号, String[40] 类型。
    /// </summary>
    public const string PlC_Archive_Pallet = nameof(PlC_Archive_Pallet);

    #endregion

    #region 扫物料

    /// <summary>
    /// 扫关键物料信号，INT 类型。
    /// </summary>
    public const string PLC_Sign_Critical_Material = nameof(PLC_Sign_Critical_Material);

    /// <summary>
    /// 扫描的关键物料条码, String[40] 类型。
    /// </summary>
    public const string PLC_Critical_Material_Barcode = nameof(PLC_Critical_Material_Barcode);

    /// <summary>
    /// 扫描的物料物料序号，INT 类型。
    /// </summary>
    public const string PLC_Critical_Material_Index = nameof(PLC_Critical_Material_Index);

    /// <summary>
    /// 扫批次料信号，INT 类型。
    /// </summary>
    public const string PLC_Sign_Batch_Material = nameof(PLC_Sign_Batch_Material);

    /// <summary>
    /// 扫描的批次料条码, String[40] 类型。
    /// </summary>
    public const string PLC_Batch_Material_Barcode = nameof(PLC_Batch_Material_Barcode);

    #endregion

    #endregion

    #region MES To PLC

    /// <summary>
    /// MES 错误描述，用于将错误消息写给 PLC。
    /// </summary>
    public const string MES_Message_Error_Desc = nameof(MES_Message_Error_Desc);

    #endregion

    /// <summary>
    /// 获取PLC信号标签的 action 名称。
    /// </summary>
    /// <param name="tag">信号标签</param>
    /// <returns></returns>
    public static string GetPlcSignAction(string tag)
    {
        if (!tag.StartsWith(PLC_Sign_Prefix))
        {
            return string.Empty;
        }

        return tag[PLC_Sign_Prefix.Length..];
    }
}
