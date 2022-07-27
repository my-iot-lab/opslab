using Bootstrap.Security.Blazor;
using BootstrapAdmin.Web.Core;

namespace BootstrapAdmin.Web.Components;

public partial class AdminCard
{
    [Parameter]
    public string? AuthorizeKey { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public string? HeaderText { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject]
    [NotNull]
    private IBootstrapAdminService? AdminService { get; set; }

    [Inject]
    [NotNull]
    private BootstrapAppContext? AppContext { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    private Task<bool> OnQueryCondition(string name)
    {
        var url = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        return Task.FromResult(AdminService.AuhorizingBlock(AppContext.UserName, url, name));
    }
}
