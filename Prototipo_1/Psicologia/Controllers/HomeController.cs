using Microsoft.AspNetCore.Mvc;

namespace Psicologia.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Specialty");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
