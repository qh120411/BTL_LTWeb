using System.Security.Claims;
using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles="Reader")]
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
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("Thiếu định danh");
            var studentName = User.Identity?.Name ?? "";
            var profile = _profileService.GetProfile(studentId, studentName);

            return View(profile);
        }

        // [Figma Reader 08] Lịch sử mượn trả
        public IActionResult BorrowHistory([FromServices] BTL_LTWEB.Models.Entities.QLThuVienUtcContext db) {
            var id=User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var rows=Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(db.ChiTietMuons,c=>c.MaBanSaoNavigation.MaTaiLieuNavigation)
                .Where(c=>c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDocNavigation.MaSinhVien==id&&c.NgayTra!=null).ToList();
            return View(rows);
        }

        // [Figma Reader 09] Thông báo của tôi
        public IActionResult Notifications()
        {
            return RedirectToAction("Index");
        }

        // [Figma Reader 10] Thông tin bạn đọc & thẻ thư viện -> chuyển hướng sang Profile
        public IActionResult Profile()
        {
            return RedirectToAction("Index", "Profile");
        }
    }
}
