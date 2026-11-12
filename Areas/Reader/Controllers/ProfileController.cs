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
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var studentName = User.Identity?.Name;

            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var profile = _profileService.GetProfile(studentId, studentName ?? "");
            return View(profile);
        }

        // [POST] /Reader/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(UserProfileViewModel model)
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var studentName = User.Identity?.Name;

            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            model.MaSinhVien = studentId;
            if (string.IsNullOrWhiteSpace(model.HoTen) && !string.IsNullOrWhiteSpace(studentName))
            {
                model.HoTen = studentName;
            }

            _profileService.UpdateProfile(model);

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân và ngân hàng thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
