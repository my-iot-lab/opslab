using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;

using Ops.Host.Common.Utils;

namespace Ops.Host.App.ViewModels;

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

    protected SinglePagedViewModelBase()
    {
        QueryCommand = new RelayCommand(() => DoSearch(1, PageSize));
        PageUpdatedCommand = new RelayCommand<FunctionEventArgs<int>>((e) => PageUpdated(e!));
        DownloadCommand = new RelayCommand<string>((path) => Download(path!));
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

    #endregion

    /// <summary>
    /// 查询数据。
    /// </summary>
    /// <param name="pageIndex">页数</param>
    /// <param name="pageSize">每页数量</param>
    protected abstract (IEnumerable<TDataSource> items, long pageCount) OnSearch(int pageIndex, int pageSize);

    /// <summary>
    /// 导出文件默认名称，默认为 "yyyyMMddHHmmss"。
    /// </summary>
    public virtual string DownloadFileName()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmss");
    }

    private void PageUpdated(FunctionEventArgs<int> e)
    {
        DoSearch(e.Info, PageSize);
    }

    private void Download(string path)
    {
        try
        {
            SaveFileDialog saveFile = new()
            {
                Filter = "导出文件 （*.xlsx）|*.xlsx",
                FilterIndex = 0,
                FileName = DownloadFileName(),
            };

            if (saveFile.ShowDialog() != true)
            {
                return;
            }

            var fileName = saveFile.FileName; 

            var (items, _) = OnSearch(1, short.MaxValue);
            ExcelHelper.Export(fileName, Path.GetFileNameWithoutExtension(fileName), items);
        }
        catch (Exception ex)
        {
            Growl.Error($"数据导出失败, 错误：{ex.Message}");
        }
    }

    private void DoSearch(int pageIndex, int pageSize)
    {
        var (items, count) = OnSearch(pageIndex, pageSize);

        PageCount = PageHelper.GetPageCount(count, PageSize);
        DataSourceList = new ObservableCollection<TDataSource>(items);
    }
}
