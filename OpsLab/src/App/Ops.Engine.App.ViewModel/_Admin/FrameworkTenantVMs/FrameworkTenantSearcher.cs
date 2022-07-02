using System.ComponentModel.DataAnnotations;
using WalkingTec.Mvvm.Core;

namespace WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkTenantVMs;

public partial class FrameworkTenantSearcher : BaseSearcher
{
    [Display(Name = "_Admin.TenantCode")]
    public string TCode { get; set; }

    [Display(Name = "_Admin.TenantName")]
    public string TName { get; set; }

    [Display(Name = "_Admin.TenantDomain")]
    public string TDomain { get; set; }

    protected override void InitVM()
    {
    }
}
