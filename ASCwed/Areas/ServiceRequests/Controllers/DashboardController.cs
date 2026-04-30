using ASCwed.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
