using Microsoft.AspNetCore.Mvc;

namespace News_Portal_UI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }
        public IActionResult Messages()
        {
            return View();
        }
    }
}