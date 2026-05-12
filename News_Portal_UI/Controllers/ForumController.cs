using Microsoft.AspNetCore.Mvc;

namespace News_Portal_1._1.Controllers
{
    public class ForumController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Detail(int id)
        {
            if (id <= 0) return RedirectToAction("Index");
            ViewBag.TopicId = id;
            return View();
        }
        public IActionResult Update(int id)
        {
            if (id <= 0) return RedirectToAction("Index");
            ViewBag.TopicId = id;
            return View();
        }
    }
}