using System.Security.Claims;
using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class MyLibraryController : ReaderBaseController
    {
        private readonly IUserProfileService _profileService;

        public MyLibraryController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        // [Figma Reader 07] Thư viện của tôi: Sách đang mượn & hạn trả
        public IActionResult Index()
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "242630923";
            var studentName = User.Identity?.Name ?? "Đỗ Văn Đạt";
            var profile = _profileService.GetProfile(studentId, studentName);

            return View(profile);
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

        // [Figma Reader 10] Thông tin bạn đọc & thẻ thư viện -> chuyển hướng sang Profile
        public IActionResult Profile()
        {
            return RedirectToAction("Index", "Profile");
        }
    }
}
