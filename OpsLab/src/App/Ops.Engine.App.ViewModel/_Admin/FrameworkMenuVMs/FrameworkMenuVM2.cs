// WTM默认页面 Wtm buidin page
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;

namespace WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkMenuVMs;

public class FrameworkMenuVM2 : BaseCRUDVM<FrameworkMenu>
{
    [Display(Name = "_Admin.Action")]
    public List<string> SelectedActionIDs { get; set; }

    [Display(Name = "_Admin.Module")]
    public string SelectedModule { get; set; }

    [Display(Name = "_Admin.AllowedRole")]
    public List<string> SelectedRolesCodes { get; set; }

    public FrameworkMenuVM2()
    {
        SelectedRolesCodes = new();
    }

    protected override void InitVM()
    {
        SelectedRolesCodes.AddRange(DC.Set<FunctionPrivilege>()
            .Where(x => x.MenuItemId == Entity.ID && x.RoleCode != null && x.Allowed == true)
            .Select(x => x.RoleCode)
            .ToList());

        var data = DC.Set<FrameworkMenu>().ToList();
        var topMenu = data.Where(x => x.ParentId == null).ToList().FlatTree(x => x.DisplayOrder);
        var modules = Wtm.GlobaInfo.AllModule;

        if (Entity.Url != null && Entity.IsInside == true)
        {
            SelectedModule = modules.Where(x => x.IsApi == true && (x.FullName == Entity.ClassName)).FirstOrDefault()?.FullName;
            if (SelectedModule != null)
            {
                var urls = modules.Where(x => x.FullName == SelectedModule && x.IsApi)
                    .SelectMany(x => x.Actions)
                    .Where(x => !x.IgnorePrivillege)
                    .Select(x => x.Url)
                    .ToList();
                SelectedActionIDs = DC.Set<FrameworkMenu>()
                    .Where(x => urls.Contains(x.Url) && x.IsInside == true && !x.FolderOnly)
                    .Select(x => x.MethodName)
                    .ToList();
            }
            else
            {
                SelectedModule = Entity.Url;
            }
        }
    }

    public override void Validate()
    {
        if (Entity.IsInside == true && !Entity.FolderOnly)
        {
            if (string.IsNullOrEmpty(SelectedModule))
            {
                MSD.AddModelError("SelectedModule", Localizer["Validate.{0}required", Localizer["_Admin.Module"]]);
            }
            else
            {
                var modules = Wtm.GlobaInfo.AllModule;
                var test = DC.Set<FrameworkMenu>().FirstOrDefault(x => x.Url != null && x.Url.ToLower() == Entity.Url.ToLower() && x.ID != Entity.ID);
                if (test != null)
                {
                    MSD.AddModelError(" error", Localizer["_Admin.ModuleHasSet"]);
                }
            }
        }
        base.Validate();
    }

    public override void DoEdit(bool updateAllFields = false)
    {
        if (Entity.IsInside == false)
        {
            if (Entity.Url != null && Entity.Url != "" && !Entity.Url.StartsWith("/"))
            {
                if (!Entity.Url.ToLower().StartsWith("http://") && !Entity.Url.ToLower().StartsWith("https://"))
                {
                    Entity.Url = "http://" + Entity.Url;
                }
            }

            if (Entity.Url != null)
            {
                Entity.Url = Entity.Url.TrimEnd('/');
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(SelectedModule) && !Entity.FolderOnly)
            {
                var modules = Wtm.GlobaInfo.AllModule;
                var ndc = DC.ReCreate();
                var actionsInDB = DC.Set<FrameworkMenu>().AsNoTracking().Where(x => x.ParentId == Entity.ID).ToList();
                var mo = modules.Where(x => x.FullName == SelectedModule && x.IsApi).FirstOrDefault();
                if (mo != null)
                {
                    Entity.ModuleName = mo.ModuleName;
                    Entity.ClassName = mo.FullName;
                    Entity.MethodName = null;

                    var otherActions = mo.Actions;
                    int order = 1;
                    Entity.Children = new List<FrameworkMenu>();
                    foreach (var action in otherActions)
                    {
                        if (SelectedActionIDs != null && SelectedActionIDs.Contains(action.MethodName))
                        {
                            Guid aid = action.ID;
                            var adb = actionsInDB.Where(x => x.Url.ToLower() == action.Url.ToLower()).FirstOrDefault();
                            if (adb != null)
                            {
                                aid = adb.ID;
                            }

                            FrameworkMenu menu = new()
                            {
                                FolderOnly = false,
                                IsPublic = Entity.IsPublic,
                                Parent = Entity,
                                ShowOnMenu = false,
                                DisplayOrder = order++,
                                IsInside = true,
                                Domain = Entity.Domain,
                                PageName = action.ActionDes?.Description ?? action.ActionName,
                                ModuleName = action.Module.ModuleName,
                                ActionName = action.ActionDes?.Description ?? action.ActionName,
                                Url = action.Url,
                                ClassName = action.Module.FullName,
                                MethodName = action.MethodName,
                                ID = aid
                            };
                            Entity.Children.Add(menu);
                        }
                    }
                }
                else
                {
                    Entity.ModuleName = "";
                    Entity.ClassName = "";
                    Entity.MethodName = "";
                }
            }
            else
            {
                Entity.Children = null;
                Entity.Url = null;
            }
        }

        if (!FC.ContainsKey("Entity.Children"))
        {
            FC.Add("Entity.Children", 0);
            FC.Add("Entity.Children[0].IsPublic", 0);
            FC.Add("Entity.Children[0].PageName", 0);
            FC.Add("Entity.Children[0].ModuleName", 0);
            FC.Add("Entity.Children[0].ActionName", 0);
            FC.Add("Entity.Children[0].ClassName", 0);
            FC.Add("Entity.Children[0].MethodName", 0);
            FC.Add("Entity.Children[0].Url", 0);
        }

        base.DoEdit(updateAllFields);

        List<Guid> guids = new()
        {
            Entity.ID
        };
        if (Entity.Children != null)
        {
            guids.AddRange(Entity.Children?.Select(x => x.ID).ToList());
        }
        AddPrivilege(guids);
    }

    public override void DoAdd()
    {
        if (Entity.IsInside == false)
        {
            if (Entity.Url != null && Entity.Url != "" && !Entity.Url.StartsWith("/"))
            {
                if (!Entity.Url.ToLower().StartsWith("http://") && !Entity.Url.ToLower().StartsWith("https://"))
                {
                    Entity.Url = "http://" + Entity.Url;
                }
            }

            if (Entity.Url != null)
            {
                Entity.Url = Entity.Url.TrimEnd('/');
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(SelectedModule) && !Entity.FolderOnly)
            {
                var modules = Wtm.GlobaInfo.AllModule;
                var mo = modules.Where(x => x.FullName == SelectedModule && x.IsApi).FirstOrDefault();
                if (mo != null)
                {
                    Entity.ModuleName = mo.ModuleName;
                    Entity.ClassName = mo.FullName;
                    Entity.MethodName = null;

                    var otherActions = mo.Actions;
                    int order = 1;
                    Entity.Children = new List<FrameworkMenu>();
                    foreach (var action in otherActions)
                    {
                        if (SelectedActionIDs != null && SelectedActionIDs.Contains(action.MethodName))
                        {
                            FrameworkMenu menu = new()
                            {
                                FolderOnly = false,
                                IsPublic = Entity.IsPublic,
                                Parent = Entity,
                                ShowOnMenu = false,
                                DisplayOrder = order++,
                                IsInside = true,
                                Domain = Entity.Domain,
                                PageName = action.ActionDes?.Description ?? action.ActionName,
                                ModuleName = action.Module.ModuleName,
                                ActionName = action.ActionDes?.Description ?? action.ActionName,
                                Url = action.Url,
                                ClassName = action.Module.FullName,
                                MethodName = action.MethodName
                            };
                            Entity.Children.Add(menu);
                        }
                    }
                }
            }
            else
            {
                Entity.Children = null;
                Entity.Url = null;
            }

        }

        base.DoAdd();
        List<Guid> guids = new()
        {
            Entity.ID
        };
        if (Entity.Children != null)
        {
            guids.AddRange(Entity.Children?.Select(x => x.ID).ToList());
        }
        AddPrivilege(guids);
    }

    public void AddPrivilege(List<Guid> menuids)
    {
        var admin = DC.Set<FrameworkRole>().Where(x => x.RoleCode == "001").SingleOrDefault();
        if (admin != null && !SelectedRolesCodes.Contains(admin.RoleCode))
        {
            SelectedRolesCodes.Add(admin.RoleCode);
        }

        foreach (var menuid in menuids)
        {
            if (SelectedRolesCodes != null)
            {
                foreach (var code in SelectedRolesCodes)
                {
                    FunctionPrivilege fp = new()
                    {
                        MenuItemId = menuid,
                        RoleCode = code,
                        Allowed = true
                    };
                    DC.Set<FunctionPrivilege>().Add(fp);
                }
            }
        }

        DC.SaveChanges();
        Wtm.RemoveUserCacheByRole(SelectedRolesCodes.ToArray()).Wait();
    }

    public override void DoDelete()
    {
        try
        {
            DC.CascadeDelete(Entity);
            DC.SaveChanges();
        }
        catch
        { }
    }
}
