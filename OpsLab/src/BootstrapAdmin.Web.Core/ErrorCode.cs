using System.ComponentModel;
using System.Reflection;

namespace BootstrapAdmin.DataAccess.Models;

/// <summary>
/// 业务异常代码
/// </summary>
public sealed class ErrorCode
{
    private ErrorCode()
    { }

    /// <summary>
    /// 成功
    /// </summary>
    public const int Success = 1;

    /// <summary>
    /// 程序异常
    /// </summary>
    [Description("MES 程序异常")]
    public const int Error = 2;

    /// <summary>
    /// SN 为空错误
    /// </summary>
    [Description("SN 不能为空")]
    public const int ErrEmptyOfSn = 401;

    /// <summary>
    /// 获取描述
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <returns></returns>
    public static string GetDescription(int code)
    {
        var field = typeof(ErrorCode).GetFields(BindingFlags.Public)
            .FirstOrDefault(f => f.FieldType == typeof(int) && (int?)f.GetRawConstantValue() == code);
        if (field != null)
        {
            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
            {
                return attr.Description;
            }
        }

        return string.Empty;
    }
}
