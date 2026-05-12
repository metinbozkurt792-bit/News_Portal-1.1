using Microsoft.AspNetCore.Mvc;

namespace News_Portal_1._1.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}