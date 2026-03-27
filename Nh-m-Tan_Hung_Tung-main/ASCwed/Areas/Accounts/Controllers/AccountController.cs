using ASCwed.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    public class AccountController : BaseController
    {
        [Authorize(Roles = "Admin")]
        public IActionResult Customers()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ServiceEngineers()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }
    }
}
