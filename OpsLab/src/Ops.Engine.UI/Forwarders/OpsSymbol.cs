
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
    /// 进站请求信号
    /// </summary>
    public const string PLC_Sign_Inbound = nameof(PLC_Sign_Inbound);

    /// <summary>
    /// 进站请求产品码
    /// </summary>
    public const string PLC_Inbound_SN = nameof(PLC_Inbound_SN);

    /// <summary>
    /// 出站信号
    /// </summary>
    public const string PLC_Sign_Outbound = nameof(PLC_Sign_Outbound);

    /// <summary>
    /// 出站的产品码
    /// </summary>
    public const string PLC_Outbound_SN = nameof(PLC_Outbound_SN);

    /// <summary>
    /// 出站的结果（OK/NG）
    /// </summary>
    public const string PLC_Outbound_Pass = nameof(PLC_Outbound_Pass);

    /// <summary>
    /// CT 时长
    /// </summary>
    public const string PLC_Outbound_Cycletime = nameof(PLC_Outbound_Cycletime);

    /// <summary>
    /// 操作人员
    /// </summary>
    public const string PlC_Outbound_Operator = nameof(PlC_Outbound_Operator);

    /// <summary>
    /// 扫关键物料信号
    /// </summary>
    public const string PLC_Sign_Critical_Material = nameof(PLC_Sign_Critical_Material);

    /// <summary>
    /// 扫描的关键物料条码
    /// </summary>
    public const string PLC_Critical_Material_Barcode = nameof(PLC_Critical_Material_Barcode);

    /// <summary>
    /// 扫描的物料物料序号
    /// </summary>
    public const string PLC_Critical_Material_Index = nameof(PLC_Critical_Material_Index);

    /// <summary>
    /// 扫批次料信号
    /// </summary>
    public const string PLC_Sign_Batch_Material = nameof(PLC_Sign_Batch_Material);

    /// <summary>
    /// 扫描的批次料条码
    /// </summary>
    public const string PLC_Batch_Material_Barcode = nameof(PLC_Batch_Material_Barcode);

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
