using System.ComponentModel.DataAnnotations.Schema;
using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.Model;

/// <summary>
/// 产品 BOM
/// </summary>
[Table("ProductBom")]
public class ProductBomEntity : BasePoco
{
    /// <summary>
    /// BOM编号
    /// </summary>
    /// <returns></returns>
    public string Code { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    /// <returns></returns>
    public string Remark { get; set; }
}
