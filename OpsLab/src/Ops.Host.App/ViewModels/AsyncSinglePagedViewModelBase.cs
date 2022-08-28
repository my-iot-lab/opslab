using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Ops.Host.Common.Utils;
using Ops.Host.App.Components;
using Ops.Host.Common.IO;

namespace Ops.Host.App.ViewModels;

/// <summary>
/// 单数据源分页 ViewModel 基类。
/// </summary>
/// <typeparam name="TDataSource">数据源类型</typeparam>
public abstract class AsyncSinglePagedViewModelBase<TDataSource> : AsyncSinglePagedViewModelBase<TDataSource, NullQueryFilter>
    where TDataSource : class, new()
{

}

/// <summary>
/// 基于异步的单数据源分页 ViewModel 基类。
/// </summary>
/// <typeparam name="TDataSource">数据源类型</typeparam>
/// <typeparam name="TQueryFilter">数据查询筛选类型</typeparam>
public abstract class AsyncSinglePagedViewModelBase<TDataSource, TQueryFilter> : ObservableObject
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

    protected AsyncSinglePagedViewModelBase()
    {
        QueryCommand = new AsyncRelayCommand(() => DoSearchAsync(1, PageSize));
        PageUpdatedCommand = new AsyncRelayCommand<FunctionEventArgs<int>>(PageUpdatedAsync!);
        DownloadCommand = new AsyncRelayCommand(DownloadAsync!);
        PrintCommand = new AsyncRelayCommand(PrintAsync);
    }

    /// <summary>
    /// 初始化查询。
    /// </summary>
    /// <returns></returns>
    protected async Task InitSearchAsync()
    {
        await DoSearchAsync(1, PageSize);
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
    protected abstract Task<(List<TDataSource> items, long pageCount)> OnSearchAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Excel 下载参数设置。
    /// </summary>
    /// <param name="builder"></param>
    protected virtual Task OnExcelCreatingAsync(ExcelModelBuilder builder)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 打印参数设置。
    /// </summary>
    protected virtual Task OnPrintCreatingAsync(PrintModelBuilder builder)
    {
        return Task.CompletedTask;
    }

    private async Task PageUpdatedAsync(FunctionEventArgs<int> e)
    {
        await DoSearchAsync(e.Info, PageSize);
    }

    private async Task DownloadAsync()
    {
        try
        {
            await DoSearchedMaxDataAsync();

            ExcelModelBuilder builder = new();
            await OnExcelCreatingAsync(builder);
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
            await Excel.ExportAsync(fileName, builder.SheetName, exportData, builder.Settings);
        }
        catch (Exception ex)
        {
            Growl.Error($"数据导出失败, 错误：{ex.Message}");
        }
    }

    private async Task PrintAsync()
    {
        PrintModelBuilder builder = new();

        try
        {
            await DoSearchedMaxDataAsync();
            await OnPrintCreatingAsync(builder);

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

    private async Task DoSearchedMaxDataAsync()
    {
        var (items, _) = await OnSearchAsync(1, short.MaxValue);
        SearchedAllData = items;
    }

    private async Task DoSearchAsync(int pageIndex, int pageSize)
    {
        var (items, count) = await OnSearchAsync(pageIndex, pageSize);

        PageCount = PageHelper.GetPageCount(count, PageSize);
        DataSourceList = new ObservableCollection<TDataSource>(items);
    }
}
