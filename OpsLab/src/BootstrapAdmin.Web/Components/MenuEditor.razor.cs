using BootstrapAdmin.DataAccess.Models;

namespace BootstrapAdmin.Web.Components;

/// <summary>
/// 
/// </summary>
public partial class MenuEditor
{
    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    [EditorRequired]
    [NotNull]
    public Navigation? Value { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<SelectedItem>? ParementMenus { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<SelectedItem>? Targets { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<SelectedItem>? Apps { get; set; }
}
