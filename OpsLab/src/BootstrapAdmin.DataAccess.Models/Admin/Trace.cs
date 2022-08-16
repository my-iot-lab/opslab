using System.ComponentModel.DataAnnotations;

namespace BootstrapAdmin.DataAccess.Models;

/// <summary>
/// 操作记录
/// </summary>
public class Trace
{
    /// <summary>
    /// Id
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 登录用户
    /// </summary>
    [Display(Name = "登录用户")]
    public string? UserName { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    [Display(Name = "操作时间")]
    public DateTime LogTime { get; set; }

    /// <summary>
    /// 登录主机
    /// </summary>
    [Display(Name = "登录主机")]
    public string? Ip { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    [Display(Name = "浏览器")]
    public string? Browser { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    [Display(Name = "操作系统")]
    public string? OS { get; set; }

    /// <summary>
    /// 操作地点
    /// </summary>
    [Display(Name = "操作地点")]
    public string? City { get; set; }

    /// <summary>
    /// 操作页面
    /// </summary>
    [Display(Name = "操作页面")]
    public string? RequestUrl { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    public string? Referer { get; set; }
}
