using System;
using System.IO.Packaging;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows.Threading;

namespace Ops.Host.App.Components;

/// <summary>
/// 打印组件
/// </summary>
public partial class PrintPreviewWindow : Window
{
    private delegate void LoadXpsMethod();

    private readonly FlowDocument _doc;

    public PrintPreviewWindow(string strTmplName, object? data, IDocumentRenderer? renderer = null)
    {
        InitializeComponent();

        _doc = LoadDocument(strTmplName, data, renderer);
        Dispatcher.BeginInvoke(new LoadXpsMethod(LoadXps), DispatcherPriority.ApplicationIdle);
    }

    /// <summary>
    /// 加载 FlowDocument。
    /// </summary>
    /// <param name="strTmplName">模板路径</param>
    /// <param name="data">模板数据</param>
    /// <returns></returns>
    public static FlowDocument LoadDocument(string strTmplName, object? data, IDocumentRenderer? renderer = null)
    {
        var doc = (FlowDocument)Application.LoadComponent(new Uri(strTmplName, UriKind.RelativeOrAbsolute));
        doc.PagePadding = new Thickness(50);
        if (data != null)
        {
            doc.DataContext = data;
        }
        if (renderer != null)
        {
            renderer.Render(doc, data);
        }
        return doc;
    }

    private void LoadXps()
    {
        // 构造一个基于内存的xps document
        MemoryStream ms = new();
        Package package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
        Uri DocumentUri = new("pack://InMemoryDocument.xps");
        PackageStore.RemovePackage(DocumentUri);
        PackageStore.AddPackage(DocumentUri, package);
        XpsDocument xpsDocument = new(package, CompressionOption.Fast, DocumentUri.AbsoluteUri);

        // 将flow document写入基于内存的xps document中去
        XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
        writer.Write(((IDocumentPaginatorSource)_doc).DocumentPaginator);

        // 获取这个基于内存的xps document的fixed document
        docViewer.Document = xpsDocument.GetFixedDocumentSequence();

        // 关闭基于内存的xps document
        xpsDocument.Close();
    }
}
