using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;

namespace Ops.Host.Common.Utils;

/// <summary>
/// 处理 Excel
/// </summary>
public static class ExcelHelper
{
    /// <summary>
    /// 全局设置的时间显示格式，默认为 "yyyy-MM-dd HH:mm:ss";
    /// </summary>
    public static string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    static ExcelHelper()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 非商业版
    }

    /// <summary>
    /// 导出 Excel。
    /// <para>导出 Excel 的 Header 优先使用导出类型的 <see cref="DisplayAttribute"/> 名称，若没有会使用类型的属性名。Excel 列顺序与属性顺序一致。</para>
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="sheetName">sheet 名称</param>
    /// <param name="data">要导出的数据</param>
    /// <param name="excludes">要排除的列名，列名与类型的属性名一致，若 includes 不为 null，会在其基础上再进行排除</param>
    /// <param name="includes">要包含的列名，列名与类型的属性名一致，若不为 null，会优先使用该选项进行筛选</param>
    public static void Export<T>(Stream fileStream, string sheetName, IEnumerable<T> data, string[]? excludes = null, string[]? includes = null)
    {
        using var package = new ExcelPackage(fileStream);
        var sheet = package.Workbook.Worksheets.Add(sheetName);

        ExportToSheet(sheet, data, excludes, includes);

        package.Save();
    }

    /// <summary>
    /// 导出 Excel。
    /// <para>导出 Excel 的 Header 优先使用导出类型的 <see cref="DisplayAttribute"/> 名称，若没有会使用类型的属性名。Excel 列顺序与属性顺序一致。</para>
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="sheetName">sheet 名称</param>
    /// <param name="data">要导出的数据</param>
    /// <param name="excludes">要排除的列名，列名与类型的属性名一致，若 includes 不为 null，会在其基础上再进行排除</param>
    /// <param name="includes">要包含的列名，列名与类型的属性名一致，若不为 null，会优先使用该选项进行筛选</param>
    public static void Export<T>(string path, string sheetName, IEnumerable<T> data, string[]? excludes = null, string[]? includes = null)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var package = new ExcelPackage(path);
        var sheet = package.Workbook.Worksheets.Add(sheetName);

        ExportToSheet(sheet, data, excludes, includes);

        package.Save();
    }

    private static void ExportToSheet<T>(ExcelWorksheet sheet, IEnumerable<T> data, string[]? excludes, string[]? includes)
    {
        // 考虑导出功能使用频率低，因此不使用缓存机制。
        List<(string DisplayName, PropertyInfo PropInfo)> columns = new();
        var props = typeof(T).GetProperties();
        if (includes?.Any() == true)
        {
            props = props.Where(s => includes.Contains(s.Name)).ToArray();
        }

        if (excludes?.Any() == true)
        {
            props = props.Where(s => !excludes.Contains(s.Name)).ToArray();
        }

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<DisplayAttribute>();
            columns.Add((attr?.Name ?? prop.Name, prop));
        }

        // 构建头
        for (int i = 0; i < columns.Count; i++)
        {
            sheet.Cells[1, i + 1].Value = columns[i].DisplayName;
        }

        int n = 2; // 从第二行开始
        // 构建主体内容
        foreach (var item in data)
        {
            for (int j = 0; j < columns.Count; j++)
            {
                var propInfo = columns[j].PropInfo;
                var cell = sheet.Cells[n, j + 1];

                cell.Value = propInfo!.GetValue(item);

                if (propInfo!.PropertyType == typeof(DateTime) 
                    || propInfo!.PropertyType == typeof(DateTime?))
                {
                    cell.Style.Numberformat.Format = DateTimeFormat;
                }

                cell.AutoFitColumns();
            }

            n++;
        }
    }
}
