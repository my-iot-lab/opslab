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
        builder.ExcelName = "用户测试示例";
        builder.SheetName = "用户测试示例";
        builder.Settings.Excludes = new string[]
        {
            nameof(User.Id),
            nameof(User.Password),
        };

        //builder.Header = new()
        //{
        //    new Common.IO.RowCustom
        //    {
        //         PaddingTop = 1,
        //         ColunmSpan = 9,
        //         Text = "用户测试示例 Header1",
        //         HorizontalAlignment = Common.IO.HorizontalAlignment.Center,
        //    },
        //    new Common.IO.RowCustom
        //    {
        //         ColunmSpan = 9,
        //         Text = "用户测试示例 Header2",
        //         HorizontalAlignment = Common.IO.HorizontalAlignment.Center,
        //    },
        //};

        //builder.Footer = new()
        //{
        //    new Common.IO.RowCustom
        //    {
        //         PaddingTop = 2,
        //         ColunmSpan = 9,
        //         Text = "用户测试示例 Footer1",
        //         HorizontalAlignment = Common.IO.HorizontalAlignment.Right,
        //    },
        //    new Common.IO.RowCustom
        //    {
        //         ColunmSpan = 9,
        //         Text = "用户测试示例 Footer2",
        //         HorizontalAlignment = Common.IO.HorizontalAlignment.Right,
        //    },
        //};
    }

    protected override void OnPrintCreating(PrintModelBuilder builder)
    {
        builder.TemplateUrl = "./UserControls/Controls/UserDocument.xaml";
        builder.DataContext = new UserDocumentViewModel
        {
            Title = "用户测试示例",
            DataSourceList = SearchedAllData,
        };
        builder.Render = new UserDocumentRender();
    }

    protected override (List<User> items, long pageCount) OnSearch(int pageIndex, int pageSize)
    {
        return _userService.GetPaged(QueryFilter, pageIndex, pageSize);
    }
}
