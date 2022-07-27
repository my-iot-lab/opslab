using Bootstrap.Security.Blazor;
using BootstrapAdmin.Web.Core;

namespace BootstrapAdmin.Web.Components;

[CascadingTypeParameter(nameof(TItem))]
public partial class AdminTable<TItem> where TItem : class, new()
{
    [Parameter]
    public IEnumerable<int>? PageItemsSource { get; set; }

    [Parameter]
    public int ExtendButtonColumnWidth { get; set; } = 130;

    [Parameter]
    public string? SortString { get; set; }

    [NotNull]
    [Parameter]
    public RenderFragment<TItem>? TableColumns { get; set; }

    [Parameter]
    public RenderFragment<TItem>? RowButtonTemplate { get; set; }

    [Parameter]
    public RenderFragment<ITableSearchModel>? CustomerSearchTemplate { get; set; }

    [Parameter]
    public RenderFragment<TItem>? EditTemplate { get; set; }

    [NotNull]
    [Parameter]
    public RenderFragment? TableToolbarTemplate { get; set; }

    [Parameter]
    public bool IsPagination { get; set; }

    [Parameter]
    public bool IsMultipleSelect { get; set; } = true;

    [Parameter]
    public bool IsFixedHeader { get; set; } = true;

    [Parameter]
    public bool IsTree { get; set; }

    [Parameter]
    public bool ShowToolbar { get; set; } = true;

    [Parameter]
    public bool ShowEmpty { get; set; } = true;

    [Parameter]
    public bool ShowLoading { get; set; } = false;

    [Parameter]
    public bool ShowSearch { get; set; } = true;

    [Parameter]
    public bool ShowAdvancedSearch { get; set; } = true;

    [Parameter]
    public bool ShowDefaultButtons { get; set; } = true;

    [Parameter]
    public bool ShowExtendButtons { get; set; } = true;

    [Parameter]
    public ITableSearchModel? CustomerSearchModel { get; set; }

    [Parameter]
    public Func<QueryPageOptions, Task<QueryData<TItem>>>? OnQueryAsync { get; set; }

    [Parameter]
    public Func<TItem, Task<IEnumerable<TableTreeNode<TItem>>>>? OnTreeExpand { get; set; }

    [Parameter]
    public Func<IEnumerable<TItem>, Task<IEnumerable<TableTreeNode<TItem>>>>? TreeNodeConverter { get; set; }

    [Parameter]
    public Func<TItem, ItemChangedType, Task<bool>>? OnSaveAsync { get; set; }

    [Parameter]
    public Func<IEnumerable<TItem>, Task<bool>>? OnDeleteAsync { get; set; }

    [Parameter]
    public List<TItem>? SelectedRows { get; set; } = new List<TItem>();

    [Parameter]
    public Func<TItem, bool>? ShowEditButtonCallback { get; set; }
    
    [Parameter]
    public Func<TItem, bool>? ShowDeleteButtonCallback { get; set; }

    [Parameter]
    public Func<TItem, TItem, bool>? ModelEqualityComparer { get; set; }

    [NotNull]
    private Table<TItem>? Instance { get; set; }

    public ValueTask ToggleLoading(bool v) => Instance.ToggleLoading(v);

    public Task QueryAsync() => Instance.QueryAsync();

    [Inject]
    [NotNull]
    private IBootstrapAdminService? AdminService { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    [Inject]
    [NotNull]
    private BootstrapAppContext? AppContext { get; set; }

    private bool AuthorizeButton(string operate)
    {
        var url = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        return AdminService.AuhorizingBlock(AppContext.UserName, url, operate);
    }
}
