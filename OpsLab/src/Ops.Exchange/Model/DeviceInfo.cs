namespace Ops.Exchange.Model;

/// <summary>
/// 设备信息
/// </summary>
public class DeviceInfo
{
    public long Id { get; set; }

    /// <summary>
    /// 设备 Schema 基础信息。
    /// </summary>
    public DeviceSchema Schema { get; }

    /// <summary>
    /// 设备包含的地址变量集合。
    /// </summary>
    public List<DeviceVariable> Variables { get; } = new List<DeviceVariable>(0);

    public DeviceInfo(DeviceSchema schema)
    {
        Schema = schema;
    }

    /// <summary>
    /// 获取设备变量
    /// </summary>
    /// <param name="tag">Tag 标签</param>
    /// <returns></returns>
    public DeviceVariable? GetVariable(string tag)
    {
        return Variables.FirstOrDefault(s => string.Equals(s.Tag, tag, StringComparison.OrdinalIgnoreCase));
    }

    public void AddVariable(DeviceVariable variable)
    {
        // 校验
        Variables.Add(variable);
    }

    public void AddVariables(IEnumerable<DeviceVariable> variables)
    {
        // 校验
        Variables.AddRange(variables);
    }
}
