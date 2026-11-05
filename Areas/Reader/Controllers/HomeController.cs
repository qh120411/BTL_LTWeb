using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class HomeController : ReaderBaseController
    {
        // [Figma Reader 01] Trang chủ Thư viện UTC
        public IActionResult Index()
        {
            return View();
        }
    }
}
