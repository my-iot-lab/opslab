using System.ComponentModel.DataAnnotations.Schema;
using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.Model;

/// <summary>
/// 设备信息
/// </summary>
[Table("EquipmentInfo")]
public class EquipmentInfoEntity : BasePoco
{
    /// <summary>
    /// 产线
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// 产线名称
    /// </summary>
    public string LineName { get; set; }

    /// <summary>
    /// 工站
    /// </summary>
    public string Station { get; set; }

    /// <summary>
    /// 工站名称
    /// </summary>
    public string StationName { get; set; }
}
