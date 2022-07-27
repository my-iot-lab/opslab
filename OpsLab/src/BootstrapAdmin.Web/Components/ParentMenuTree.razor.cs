using BootstrapAdmin.Web.Core;
using BootstrapAdmin.Web.Extensions;

namespace BootstrapAdmin.Web.Components;

public partial class ParentMenuTree
{
    [Parameter]
    [EditorRequired]
    [NotNull]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [NotNull]
    private List<TreeItem>? InternalItems { get; set; }

    [Inject]
    [NotNull]
    private INavigation? NavigationService { get; set; }

    [Inject]
    [NotNull]
    private IDict? DictService { get; set; }

    [Inject]
    [NotNull]
    private BootstrapAppContext? Context { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var items = NavigationService.GetAllMenus(Context.UserName);
        InternalItems = items.ToTreeItemList(new List<string> { Value }, RenderTreeItem);
    }

    private async Task OnTreeItemChecked(List<TreeItem> items)
    {
        Value = items.First().Key?.ToString();
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(Value);
        }
    }

    private string GetApp(string? app) => DictService.GetApps().FirstOrDefault(i => i.Key == app).Value ?? "未设置";
}
