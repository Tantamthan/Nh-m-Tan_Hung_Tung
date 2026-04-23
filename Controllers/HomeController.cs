using System.Diagnostics;
using Lab8_PhamVanTung_2324801030079.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lab8_PhamVanTung_2324801030079.Controllers;

public class HomeController : AnonymousController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
