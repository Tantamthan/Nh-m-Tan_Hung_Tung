using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASCwed.Controllers
{
    [AllowAnonymous]
    public abstract class AnonymousController : Controller
    {
    }
}
