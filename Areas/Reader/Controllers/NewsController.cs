using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class NewsController : Controller
    {
        // [Figma Reader 05] Tin tức & thông báo hoạt động
        public IActionResult Index()
        {
            return View();
        }
    }
}
