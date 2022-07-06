using System.ComponentModel.DataAnnotations.Schema;

namespace Ops.Engine.App.Model;

/// <summary>
/// 物料信息
/// </summary>
[Table("MaterialInfo")]
public class MaterialInfoEntity
{
    /// <summary>
    /// 物料编码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 规格型号
    /// </summary>
    /// <returns></returns>
    public string Model { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    /// <returns></returns>
    public string Unit { get; set; }

    /// <summary>
    /// 条码规则，多个以逗号分隔。可用于扫码校验。
    /// </summary>
    /// <returns></returns>
    public string BarCodeRule { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    /// <returns></returns>
    public string Remark { get; set; }
}
