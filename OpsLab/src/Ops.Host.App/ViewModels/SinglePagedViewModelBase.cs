using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows.Input;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Ops.Host.Common.Utils;
using Ops.Host.App.Components;
using Ops.Host.Common.IO;

namespace Ops.Host.App.ViewModels;

public sealed class ExcelModelBuilder
{
    /// <summary>
    /// 保存的 Excel 默认名称。
    /// </summary>
    public string? ExcelName { get; set; }

    /// <summary>
    /// Excel Sheet 名称。
    /// </summary>
    public string? SheetName { get; set; }

    public ExcelSettings Settings { get; } = new();

    public List<RowCustom>? Header { get; set; }

    public List<RowCustom>? Footer { get; set; }
}

public delegate void DoPrintDelegate(PrintDialog pdlg, DocumentPaginator paginator);

public sealed class PrintModelBuilder
{
    /// <summary>
    /// 打印模式。
    /// </summary>
    public PrintMode Mode { get; set; } = PrintMode.Preview;

    /// <summary>
    /// 要打印的模板路径。
    /// </summary>
    public string? TemplateUrl { get; set; }

    /// <summary>
    /// 文档描述
    /// </summary>
    public string DocumentDescription { get; set; } = "Document";

    /// <summary>
    /// 数据上下文
    /// </summary>
    public object? DataContext { get; set; }

    /// <summary>
    /// 文档呈现器。
    /// </summary>
    public IDocumentRenderer? Render { get; set; }

    public enum PrintMode
    {
        /// <summary>
        /// 打印预览。
        /// </summary>
        Preview,

        /// <summary>
        /// 弹出打印框。
        /// </summary>
        Dialog,

        /// <summary>
        /// 直接打印。
        /// </summary>
        Direct,
    }
}

/// <summary>
/// 空查询筛选器。
/// </summary>
public class NullQueryFilter { }

/// <summary>
/// 单数据源分页 ViewModel 基类。
/// </summary>
/// <typeparam name="TDataSource">数据源类型</typeparam>
public abstract class SinglePagedViewModelBase<TDataSource> : SinglePagedViewModelBase<TDataSource, NullQueryFilter>
    where TDataSource : class, new()
{

}

/// <summary>
/// 单数据源分页 ViewModel 基类。
/// </summary>
/// <typeparam name="TDataSource">数据源类型</typeparam>
/// <typeparam name="TQueryFilter">数据查询筛选类型</typeparam>
public abstract class SinglePagedViewModelBase<TDataSource, TQueryFilter> : ObservableObject
    where TDataSource : class, new()
    where TQueryFilter : class, new()
{
    private long _pageCount;
    private ObservableCollection<TDataSource>? _dataSourceList;
    public TQueryFilter _queryFilter = new();

    /// <summary>
    /// 每页数量，默认 20 条。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 获取或设置控件来源
    /// </summary>
    public Control? Owner { get; set; }

    /// <summary>
    /// 已根据筛选条件查询的所有数据。
    /// </summary>
    protected List<TDataSource> SearchedAllData { get; private set; } = new();

    protected SinglePagedViewModelBase()
    {
        QueryCommand = new RelayCommand(() => DoSearch(1, PageSize));
        PageUpdatedCommand = new RelayCommand<FunctionEventArgs<int>>((e) => PageUpdated(e!));
        DownloadCommand = new RelayCommand(Download);
        PrintCommand = new RelayCommand(Print);
    }

    /// <summary>
    /// 初始化查询。
    /// </summary>
    protected void InitSearch()
    {
        DoSearch(1, PageSize);
    }

    #region 绑定属性

    /// <summary>
    /// 总页数。
    /// </summary>
    public long PageCount
    {
        get => _pageCount;
        set => SetProperty(ref _pageCount, value);
    }

    /// <summary>
    /// 数据源。
    /// </summary>
    public ObservableCollection<TDataSource>? DataSourceList
    {
        get => _dataSourceList;
        set => SetProperty(ref _dataSourceList, value);
    }

    /// <summary>
    /// 查询筛选器。
    /// </summary>
    public TQueryFilter QueryFilter
    {
        get => _queryFilter;
        set => SetProperty(ref _queryFilter, value);
    }

    #endregion

    #region 绑定事件

    /// <summary>
    /// 数据查询事件。
    /// </summary>
    public ICommand QueryCommand { get; }

    /// <summary>
    /// 数据查询分页事件。
    /// </summary>
    public ICommand PageUpdatedCommand { get; }

    /// <summary>
    /// 导出查询的数据。
    /// </summary>
    public ICommand DownloadCommand { get; }

    /// <summary>
    /// 打印查询出的数据。
    /// </summary>
    public ICommand PrintCommand { get; }

    #endregion

    /// <summary>
    /// 查询数据。
    /// </summary>
    /// <param name="pageIndex">页数</param>
    /// <param name="pageSize">每页数量</param>
    protected abstract (List<TDataSource> items, long pageCount) OnSearch(int pageIndex, int pageSize);

    /// <summary>
    /// Excel 下载参数设置。
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void OnExcelCreating(ExcelModelBuilder builder)
    {

    }

    /// <summary>
    /// 打印参数设置。
    /// </summary>
    protected virtual void OnPrintCreating(PrintModelBuilder builder)
    {

    }

    private void PageUpdated(FunctionEventArgs<int> e)
    {
        DoSearch(e.Info, PageSize);
    }

    private void Download()
    {
        try
        {
            DoSearchedMaxData();

            ExcelModelBuilder builder = new();
            OnExcelCreating(builder);
            builder.ExcelName ??= DateTime.Now.ToString("yyyyMMddHHmmss");
            builder.SheetName ??= Path.GetFileNameWithoutExtension(builder.ExcelName);

            SaveFileDialog saveFile = new()
            {
                Filter = "导出文件 （*.xlsx）|*.xlsx",
                FilterIndex = 0,
                FileName = builder.ExcelName!,
            };

            if (saveFile.ShowDialog() != true)
            {
                return;
            }

            var fileName = saveFile.FileName; 
            ExcelExportData<TDataSource> exportData = new()
            {
                Header = builder.Header,
                Body = SearchedAllData,
                Footer = builder.Footer,
            };
            Excel.Export(fileName, builder.SheetName, exportData, builder.Settings);
        }
        catch (Exception ex)
        {
            Growl.Error($"数据导出失败, 错误：{ex.Message}");
        }
    }

    private void Print()
    {
        PrintModelBuilder builder = new();
        try
        {
            DoSearchedMaxData();
            OnPrintCreating(builder);

            if (builder.Mode == PrintModelBuilder.PrintMode.Preview)
            {
                // TODO：若是打开失败后，关闭主窗体应用程序依旧存在的 bug。
                PrintPreviewWindow? previewWnd = null;
                try
                {
                    previewWnd = new(builder.TemplateUrl!, builder.DataContext, builder.Render);
                    previewWnd.Owner = System.Windows.Application.Current.MainWindow;
                    previewWnd.ShowInTaskbar = false;
                    previewWnd.ShowDialog();
                }
                catch
                {
                    previewWnd?.Close();
                    throw;
                }
            }
            else if (builder.Mode == PrintModelBuilder.PrintMode.Dialog)
            {
                PrintDialog pdlg = new();
                if (pdlg.ShowDialog() == true)
                {
                    FlowDocument doc = PrintPreviewWindow.LoadDocument(builder.TemplateUrl!, builder.DataContext, builder.Render);
                    Owner?.Dispatcher.BeginInvoke(new DoPrintDelegate(DoPrint), DispatcherPriority.ApplicationIdle, pdlg, ((IDocumentPaginatorSource)doc).DocumentPaginator);
                }
            }
            else if (builder.Mode == PrintModelBuilder.PrintMode.Direct)
            {
                PrintDialog pdlg = new();
                FlowDocument doc = PrintPreviewWindow.LoadDocument(builder.TemplateUrl!, builder.DataContext, builder.Render);
                Owner?.Dispatcher.BeginInvoke(new DoPrintDelegate(DoPrint), DispatcherPriority.ApplicationIdle, pdlg, ((IDocumentPaginatorSource)doc).DocumentPaginator);
            }
        }
        catch (Exception ex)
        {
            Growl.Error($"数据打印失败, 错误：{ex.Message}");
        }

        void DoPrint(PrintDialog pdlg, DocumentPaginator paginator)
        {
            pdlg.PrintDocument(paginator, builder.DocumentDescription);
        }
    }

    private void DoSearchedMaxData()
    {
        var (items, _) = OnSearch(1, short.MaxValue);
        SearchedAllData = items;
    }

    private void DoSearch(int pageIndex, int pageSize)
    {
        var (items, count) = OnSearch(pageIndex, pageSize);

        PageCount = PageHelper.GetPageCount(count, PageSize);
        DataSourceList = new ObservableCollection<TDataSource>(items);
    }
}
