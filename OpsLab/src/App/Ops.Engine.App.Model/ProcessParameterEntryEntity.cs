using System;
using System.ComponentModel.DataAnnotations.Schema;
using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.Model;

/// <summary>
/// 工艺参数条码
/// </summary>
[Table("ProcessParameter")]
public class ProcessParameterEntryEntity : BasePoco
{
    public Guid ProcessParameterId { get; set; }

    /// <summary>
    /// 工参的编号
    /// </summary>
    public string No { get; set; }

    /// <summary>
    /// 工参的值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 工参数据类型
    /// </summary>
    public int DataType { get; set; }

    /// <summary>
    /// 工参数据长度
    /// </summary>
    public int DataLength { get; set; }

    /// <summary>
    /// 工参描述
    /// </summary>
    public string Desc { get; set; }
}
