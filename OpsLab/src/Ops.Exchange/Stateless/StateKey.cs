namespace Ops.Exchange.Stateless;

/// <summary>
/// 状态 Key 值
/// </summary>
/// <param name="Group">分组名称。注：该值与 <see cref="StateTable.Name"/> 相对应</param>
/// <param name="Tag">标签名称</param>
public record class StateKey(string Group, string Tag)
{

}
