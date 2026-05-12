using Microsoft.AspNetCore.Mvc;

namespace News_Portal_UI.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult Update(int id)
        {
            ViewBag.CategoryId = id;
            return View();
        }
    }
}