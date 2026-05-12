using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace News_Portal_UI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
        [AllowAnonymous] 
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult AccessDeniedForAdmin()
        {
            return View();
        }
    }
}