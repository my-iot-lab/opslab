using System.Collections.Generic;
using Ops.Host.Core.Models;
using Ops.Host.Core.Services;

namespace Ops.Host.App.ViewModels;

public sealed class UserViewModel : SinglePagedViewModelBase<User, UserFilter>
{
    private readonly IUserService _userService;

    public UserViewModel(IUserService userService)
    {
        _userService = userService;
    }

    protected override void OnExcelCreating(ExcelModelBuilder builder)
    {
        builder.ExcelName = "";
        builder.SheetName = "";
        builder.ExcludeColumns = new string[]
        {
            nameof(User.Id),
            nameof(User.Password),
        };
    }

    protected override (IEnumerable<User> items, long pageCount) OnSearch(int pageIndex, int pageSize)
    {
        return _userService.GetPaged(QueryFilter, pageIndex, pageSize);
    }
}
