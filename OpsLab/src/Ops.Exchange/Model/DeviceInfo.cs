namespace Ops.Exchange.Model;

/// <summary>
/// 设备信息
/// </summary>
public class DeviceInfo
{
    /// <summary>
    /// 设备唯一编号，建议以 "[产线]_[工站]" 方式命名。
    /// </summary>
    [NotNull]
    public string? Name { get; set; }

    /// <summary>
    /// 设备 Schema 基础信息。
    /// </summary>
    [NotNull]
    public DeviceSchema? Schema { get; set; }

    /// <summary>
    /// 设备包含的地址变量集合。
    /// </summary>
    public List<DeviceVariable> Variables { get; set; } = new(0);

    public DeviceInfo()
    {

    }

    public DeviceInfo(string name, DeviceSchema schema)
    {
        Name = name;
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

    /// <summary>
    /// 添加设备变量，若已存在，则不会再添加
    /// </summary>
    /// <param name="variable">要添加的变量</param>
    public void AddVariable(DeviceVariable variable)
    {
        if (Variables.Any(s => s == variable))
        {
            return;
        }

        Variables.Add(variable);
    }

    /// <summary>
    /// 添加设备变量集合，若已存在，则不会再添加
    /// </summary>
    /// <param name="variables">要添加的变量集合</param>
    public void AddVariables(IEnumerable<DeviceVariable> variables)
    {
        foreach (var variable in variables)
        {
            AddVariable(variable);
        }
    }
}
