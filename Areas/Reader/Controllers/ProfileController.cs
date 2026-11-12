using System.Security.Claims;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class ProfileController : ReaderBaseController
    {
        private readonly IUserProfileService _profileService;

        public ProfileController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        // [GET] /Reader/Profile
        [HttpGet]
        public IActionResult Index()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(studentId)) return Unauthorized("Phiên đăng nhập thiếu định danh. Vui lòng đăng nhập lại.");
            try { return View(_profileService.GetProfile(studentId)); }
            catch (KeyNotFoundException) { return NotFound("Không tìm thấy hồ sơ bạn đọc của tài khoản đang đăng nhập."); }
        }

        // [POST] /Reader/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ProfileContactInputModel model)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(studentId)) return Unauthorized("Phiên đăng nhập thiếu định danh. Vui lòng đăng nhập lại.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Only contact/bank fields belong to this form. Identity and loans stay server-owned.
            UserProfileViewModel profile;
            try { profile = _profileService.GetProfile(studentId); }
            catch (KeyNotFoundException) { return NotFound("Không tìm thấy hồ sơ bạn đọc của tài khoản đang đăng nhập."); }
            profile.SoDienThoai = model.SoDienThoai;
            profile.DiaChiNha = model.DiaChiNha;
            profile.SoTaiKhoanNganHang = model.SoTaiKhoanNganHang;
            profile.TenNganHang = model.TenNganHang;
            profile.Email = model.Email;
            _profileService.UpdateProfile(profile);

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
