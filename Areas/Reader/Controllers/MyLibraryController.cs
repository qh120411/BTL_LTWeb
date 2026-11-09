using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class MyLibraryController : ReaderBaseController
    {
        // Chuyển hướng toàn bộ sang Hồ sơ sinh viên vì tất cả chức năng đã tích hợp tại Profile
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Profile");
        }

        public IActionResult BorrowHistory()
        {
            return RedirectToAction("Index", "Profile");
        }

        public IActionResult Notifications()
        {
            return RedirectToAction("Index", "Profile");
        }

        public IActionResult Profile()
        {
            return RedirectToAction("Index", "Profile");
        }
    }
}
