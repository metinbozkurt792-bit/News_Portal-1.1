using Microsoft.AspNetCore.Mvc;

namespace News_Portal_UI.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}