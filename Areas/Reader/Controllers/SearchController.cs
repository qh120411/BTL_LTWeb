using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class SearchController : Controller
    {
        // [Figma Reader 02] Tra cứu học liệu OPAC đa tiêu chí
        public IActionResult Index(string? keyword, string? category)
        {
            ViewBag.Keyword = keyword;
            ViewBag.Category = category ?? "all";
            return View();
        }
    }
}
