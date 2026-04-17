using ASCwed.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        public IActionResult ServiceRequest()
        {
            return View();
        }
    }
}
