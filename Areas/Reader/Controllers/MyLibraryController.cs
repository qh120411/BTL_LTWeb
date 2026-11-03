using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class MyLibraryController : Controller
    {
        // [Figma Reader 07] Thư viện của tôi: Sách đang mượn & hạn trả
        public IActionResult Index()
        {
            return View();
        }

        // [Figma Reader 08] Lịch sử mượn trả
        public IActionResult BorrowHistory()
        {
            return View();
        }

        // [Figma Reader 09] Thông báo của tôi
        public IActionResult Notifications()
        {
            return View();
        }

        // [Figma Reader 10] Thông tin bạn đọc & thẻ thư viện
        public IActionResult Profile()
        {
            return View();
        }
    }
}
