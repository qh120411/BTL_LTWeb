using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class DocumentController : Controller
    {
        // [Figma Reader 03] Chi tiết học liệu & Giới thiệu sách
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Search");
        }

        public IActionResult Details(int? id)
        {
            ViewBag.DocumentId = id ?? 1;
            return View();
        }
    }
}
