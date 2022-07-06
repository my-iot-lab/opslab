using System.ComponentModel.DataAnnotations.Schema;
using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.Model;

/// <summary>
/// 警报信息
/// </summary>
[Table("AlarmInfo")]
public class AlarmInfoEntity : BasePoco
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
    /// 报警来源，如设备、机器人、夹具等。
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// 警报代码
    /// </summary>
    public int AlarmCode { get; set; }

    /// <summary>
    /// 警报描述
    /// </summary>
    public string AlarmDesc { get; set; }
}
