using Lab8_PhamVanTung_2324801030079.Constants;
using Lab8_PhamVanTung_2324801030079.Controllers;
using Lab8_PhamVanTung_2324801030079.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Lab8_PhamVanTung_2324801030079.Areas.ServiceRequests.Controllers;

[Area("ServiceRequests")]
public sealed class DashboardController : BaseController
{
    public IActionResult Dashboard()
    {
        ViewData["CurrentUserName"] = User.GetEmailValue() ?? User.Identity?.Name ?? "Authenticated User";
        ViewData["CurrentUserRoles"] = string.Join(", ", User.GetRoleValues().DefaultIfEmpty(RoleConstants.User));
        return View();
    }
}
