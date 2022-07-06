using System.ComponentModel.DataAnnotations.Schema;
using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.Model;

/// <summary>
/// 工艺参数实体类。
/// </summary>
/// <remarks>工艺参数是跟随产品的。</remarks>
[Table("ProcessParameter")]
public class ProcessParameterEntity : BasePoco
{
    /// <summary>
    /// 产线
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// 工站
    /// </summary>
    public string Station { get; set; }

    /// <summary>
    /// 产品编号
    /// </summary>
    /// <returns></returns>
    public string ProductionCode { get; set; }
}
