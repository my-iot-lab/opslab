using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Ops.Host.Common.IO;

public sealed class ExcelSettings
{
    /// <summary>
    /// 全局设置的时间显示格式，默认为 "yyyy-MM-dd HH:mm:ss";
    /// </summary>
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 小数类型显示格式，默认为 "0.00"。
    /// </summary>
    public string RealFormat { get; set; } = "0.00";

    /// <summary>
    /// 单元格是否有边框, 默认为 true。
    /// </summary>
    public bool IstBorderStyleTine { get; set; } = true;

    /// <summary>
    /// 要排除的列名，列名与类型的属性名一致，若 includes 不为 null，会在其基础上再进行排除。
    /// </summary>
    public string[]? Excludes { get; set; }

    /// <summary>
    /// 要包含的列名，列名与类型的属性名一致，若不为 null，会优先使用该选项进行筛选。
    /// </summary>
    public string[]? Includes { get; set; }
}

public sealed class RowCustom
{
    /// <summary>
    /// 与顶部的间隔行。
    /// </summary>
    public int PaddingTop { get; set; }

    /// <summary>
    /// 与左边的区域间隔。
    /// </summary>
    public int PaddingLeft { get; set; }

    /// <summary>
    /// 与底部的间隔行。
    /// </summary>
    public int PaddingButtom { get; set; }

    /// <summary>
    /// 行跨度，默认为 1。
    /// </summary>
    public int RowSpan { get; set; } = 1;

    /// <summary>
    /// 列跨度，默认为 1。
    /// </summary>
    public int ColunmSpan { get; set; } = 1;

    /// <summary>
    /// 文本内容。
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 文本水平方向对齐方式。
    /// </summary>
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
}

/// <summary>
/// 文本水平方向对齐方式。
/// </summary>
public enum HorizontalAlignment
{
    Left,
    Center,
    Right,
}

/// <summary>
/// 要导出的数据。
/// </summary>
public sealed class ExcelExportData<T>
{
    public List<RowCustom>? Header { get; set; }

    /// <summary>
    /// 注数据
    /// </summary>
    [NotNull]
    public List<T>? Body { get; set; }

    public List<RowCustom>? Footer { get; set; }
}

public sealed class Excel
{
    static Excel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 非商业版
    }

    /// <summary>
    /// 导出 Excel。
    /// <para>导出 Excel 的 Header 优先使用导出类型的 <see cref="DisplayAttribute"/> 名称，若没有会使用类型的属性名。Excel 列顺序与属性顺序一致。</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">路径</param>
    /// <param name="sheetName">sheet 名称</param>
    /// <param name="data">要导出的数据</param>
    /// <param name="settings">设置</param>
    public static void Export<T>(string path, string sheetName, IEnumerable<T> data, ExcelSettings? settings = default)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var package = new ExcelPackage(path);
        var sheet = package.Workbook.Worksheets.Add(sheetName);
        ExportToSheet(sheet, data, 1, settings);
        package.Save();
    }

    public static async Task ExportAsync<T>(string path, string sheetName, IEnumerable<T> data, ExcelSettings? settings = default)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var package = new ExcelPackage(path);
        var sheet = package.Workbook.Worksheets.Add(sheetName);
        ExportToSheet(sheet, data, 1, settings);
        await package.SaveAsync();
    }

    /// <summary>
    /// 导出 Excel。
    /// <para>导出 Excel 的 Header 优先使用导出类型的 <see cref="DisplayAttribute"/> 名称，若没有会使用类型的属性名。Excel 列顺序与属性顺序一致。</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileStream">文件流</param>
    /// <param name="sheetName">sheet 名称</param>
    /// <param name="data">要导出的数据</param>
    /// <param name="settings">设置</param>
    public static void Export<T>(Stream fileStream, string sheetName, IEnumerable<T> data, ExcelSettings? settings = default)
    {
        using var package = new ExcelPackage(fileStream);
        var sheet = package.Workbook.Worksheets.Add(sheetName);
        ExportToSheet(sheet, data, 1, settings);
        package.Save();
    }

    /// <summary>
    /// 导出 Excel。
    /// <para>导出 Excel 的 Header 优先使用导出类型的 <see cref="DisplayAttribute"/> 名称，若没有会使用类型的属性名。Excel 列顺序与属性顺序一致。</para>
    /// </summary>
    public static async Task ExportAsync<T>(Stream fileStream, string sheetName, IEnumerable<T> data, ExcelSettings? settings = default)
    {
        using var package = new ExcelPackage(fileStream);
        var sheet = package.Workbook.Worksheets.Add(sheetName);
        ExportToSheet(sheet, data, 1, settings);
        await package.SaveAsync();
    }

    /// <summary>
    /// 导出 Excel。
    /// <para>导出 Excel 的 Header 优先使用导出类型的 <see cref="DisplayAttribute"/> 名称，若没有会使用类型的属性名。Excel 列顺序与属性顺序一致。</para>
    /// </summary>
    public static void Export<T>(string path, string sheetName, ExcelExportData<T> data, ExcelSettings? settings = default)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var package = new ExcelPackage(path);
        var sheet = package.Workbook.Worksheets.Add(sheetName);
        SetMainData(sheet, data, settings);
        package.Save();
    }

    /// <summary>
    /// 导出 Excel。
    /// <para>导出 Excel 的 Header 优先使用导出类型的 <see cref="DisplayAttribute"/> 名称，若没有会使用类型的属性名。Excel 列顺序与属性顺序一致。</para>
    /// </summary>
    public static async Task ExportAsync<T>(string path, string sheetName, ExcelExportData<T> data, ExcelSettings? settings = default)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        using var package = new ExcelPackage(path);
        var sheet = package.Workbook.Worksheets.Add(sheetName);
        SetMainData(sheet, data, settings);
        await package.SaveAsync();
    }

    private static void SetMainData<T>(ExcelWorksheet sheet, ExcelExportData<T> data, ExcelSettings? settings = default)
    {
        int rowIndex = 0; // Row/Col 起始为 1。

        // 设置 Header
        if (data.Header != null)
        {
            rowIndex += SetCustomRow(sheet, rowIndex, data.Header);
        }

        // 设置主体数据
        rowIndex++;
        ExportToSheet(sheet, data.Body!, rowIndex, settings);

        rowIndex += data.Body!.Count; // 重置行索引位置

        // 设置 Footer
        if (data.Footer != null)
        {
            SetCustomRow(sheet, rowIndex, data.Footer);
        }
    }

    private static int SetCustomRow(ExcelWorksheet sheet, int rowIndex, IEnumerable<RowCustom> rows)
    {
        foreach (var row in rows)
        {
            rowIndex += row.PaddingTop + 1;

            int fromRow = rowIndex;
            int fromCol = 1 + row.PaddingLeft;
            int toRow = fromRow + (row.RowSpan - 1);
            int toCol = fromCol + (row.ColunmSpan - 1);

            var cell = sheet.Cells[fromRow, fromCol, toRow, toCol];
            if (row.RowSpan > 1 || row.ColunmSpan > 1)
            {
                cell.Merge = true;
            }

            cell.Value = row.Text;

            cell.Style.HorizontalAlignment = row.HorizontalAlignment switch
            {
                HorizontalAlignment.Left => ExcelHorizontalAlignment.Left,
                HorizontalAlignment.Center => ExcelHorizontalAlignment.Center,
                HorizontalAlignment.Right => ExcelHorizontalAlignment.Right,
                _ => ExcelHorizontalAlignment.Left,
            };

            rowIndex += (row.RowSpan - 1) + row.PaddingButtom;
        }

        return rowIndex;
    }

    private static void ExportToSheet<T>(ExcelWorksheet sheet, IEnumerable<T> data, int startRow, ExcelSettings? settings = default)
    {
        settings ??= new ExcelSettings();

        // 考虑导出功能使用频率低，因此不使用缓存机制。
        List<(string DisplayName, PropertyInfo PropInfo)> columns = new();
        var props = typeof(T).GetProperties();
        if (settings.Includes?.Any() == true)
        {
            props = props.Where(s => settings.Includes.Contains(s.Name)).ToArray();
        }

        if (settings.Excludes?.Any() == true)
        {
            props = props.Where(s => !settings.Excludes.Contains(s.Name)).ToArray();
        }

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<DisplayAttribute>();
            columns.Add((attr?.Name ?? prop.Name, prop));
        }

        // 构建头
        for (int i = 0; i < columns.Count; i++)
        {
            var cell = sheet.Cells[startRow, i + 1];
            cell.Value = columns[i].DisplayName;
            cell.Style.Font.Bold = true;
            if (settings.IstBorderStyleTine)
            {
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        int n = startRow + 1;
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
                    cell.Style.Numberformat.Format = settings.DateTimeFormat;
                }
                else if (propInfo!.PropertyType == typeof(decimal) || propInfo!.PropertyType == typeof(decimal?)
                    || propInfo!.PropertyType == typeof(double) || propInfo!.PropertyType == typeof(double?)
                    || propInfo!.PropertyType == typeof(float?) || propInfo!.PropertyType == typeof(float?))
                {
                    cell.Style.Numberformat.Format = settings.RealFormat;
                }

                if (settings.IstBorderStyleTine)
                {
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                cell.AutoFitColumns();
            }

            n++;
        }
    }
}
