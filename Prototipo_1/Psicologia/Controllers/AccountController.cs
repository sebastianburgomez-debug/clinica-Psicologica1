using Microsoft.AspNetCore.Mvc;

namespace ClinicaPsicologia.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}