using System;
using System.ComponentModel.DataAnnotations.Schema;
using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.Model;

/// <summary>
/// 产品 BOM 明细
/// </summary>
[Table("ProductBomEntry")]
public class ProductBomEntryEntity : BasePoco
{
    /// <summary>
    /// 产品 BOM Id
    /// </summary>
    public Guid ProductBomId { get; set; }
}
