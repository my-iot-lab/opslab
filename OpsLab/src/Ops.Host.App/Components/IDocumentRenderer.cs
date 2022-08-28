using System.Windows.Documents;

namespace Ops.Host.App.Components;

/// <summary>
/// 文档呈现器。
/// </summary>
public interface IDocumentRenderer
{
    void Render(FlowDocument doc, object? data);
}
