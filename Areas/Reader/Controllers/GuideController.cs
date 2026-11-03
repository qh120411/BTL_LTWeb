using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class GuideController : Controller
    {
        // [Figma Reader 04] Dịch vụ & hướng dẫn mượn trả
        public IActionResult Index()
        {
            return View();
        }
    }
}
