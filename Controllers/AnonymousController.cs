using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab8_PhamVanTung_2324801030079.Controllers;

[AllowAnonymous]
public abstract class AnonymousController : Controller
{
}
