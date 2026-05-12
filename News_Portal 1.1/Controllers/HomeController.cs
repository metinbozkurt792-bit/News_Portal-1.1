using Microsoft.AspNetCore.Mvc;

namespace News_Portal_1._1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult NewsDetail(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Index");
            }

            ViewBag.NewsId = id;

            return View();
        }
    }
}