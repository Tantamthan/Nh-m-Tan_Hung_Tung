using ASCwed.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        public IActionResult MasterKeys()
        {
            return View();
        }

        public IActionResult MasterValues()
        {
            return View();
        }
    }
}
