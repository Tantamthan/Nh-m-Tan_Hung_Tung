using Lab8_PhamVanTung_2324801030079.Constants;
using Lab8_PhamVanTung_2324801030079.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab8_PhamVanTung_2324801030079.Areas.ServiceRequests.Controllers;

[Area("ServiceRequests")]
public sealed class MenuController : BaseController
{
    [Authorize(Roles = RoleConstants.Admin)]
    public IActionResult Customers()
    {
        return Placeholder("Customers", "Quản lý danh sách khách hàng dành cho Admin.");
    }

    [Authorize(Roles = RoleConstants.Admin)]
    public IActionResult ServiceEngineers()
    {
        return Placeholder("Service Engineers", "Quản lý đội ngũ kỹ sư dịch vụ dành cho Admin.");
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Engineer}")]
    public IActionResult NewServiceRequest()
    {
        return Placeholder("New Service Request", "Tạo phiếu yêu cầu dịch vụ mới cho Admin và Engineer.");
    }

    public IActionResult ServiceNotifications()
    {
        return Placeholder("Service Notifications", "Khu vực theo dõi các thông báo vận hành dịch vụ.");
    }

    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.User}")]
    public IActionResult Promotions()
    {
        return Placeholder("Promotions", "Khu vực quản trị chương trình khuyến mãi phù hợp với role hiện tại.");
    }

    [Authorize(Roles = RoleConstants.Admin)]
    public IActionResult MasterKeys()
    {
        return Placeholder("Master Keys", "Màn hình quản lý khóa danh mục gốc dành cho Admin.");
    }

    [Authorize(Roles = RoleConstants.Admin)]
    public IActionResult MasterValues()
    {
        return Placeholder("Master Values", "Màn hình quản lý giá trị danh mục gốc dành cho Admin.");
    }

    public IActionResult Profile()
    {
        return Placeholder("Profile", "Thông tin hồ sơ người dùng hiện hành.");
    }

    private ViewResult Placeholder(string title, string description)
    {
        ViewData["PageTitle"] = title;
        ViewData["PageDescription"] = description;
        return View("Placeholder");
    }
}
