using System.Security.Claims;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles="Reader")]
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
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("Thiếu định danh");
            var studentName = User.Identity?.Name ?? "";

            var profile = _profileService.GetProfile(studentId, studentName);
            return View(profile);
        }

        // [POST] /Reader/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(UserProfileViewModel model)
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("Thiếu định danh");
            var studentName = User.Identity?.Name ?? "";

            model.MaSinhVien = studentId;
            if (string.IsNullOrWhiteSpace(model.HoTen))
            {
                model.HoTen = studentName;
            }

            if ((model.Email?.Length??0)>150 || (!string.IsNullOrWhiteSpace(model.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(model.Email)) || (model.SoDienThoai?.Length??0)>20 || (model.DiaChiNha?.Length??0)>250) {
                ModelState.AddModelError("", "Kiểm tra email, số điện thoại và địa chỉ.");return View(model);
            }
            _profileService.UpdateProfile(model);

            TempData["SuccessMessage"] = "Đã cập nhật thông tin liên hệ.";
            return RedirectToAction(nameof(Index));
        }
    }
}
