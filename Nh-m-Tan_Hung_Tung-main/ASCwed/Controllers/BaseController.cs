using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
    }
}
