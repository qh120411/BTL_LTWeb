using System.Security.Claims;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Areas.Reader.Controllers
{
    [Area("Reader")]
    public class HomeController : ReaderBaseController
    {
        private readonly IUserProfileService _profileService;

        public HomeController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        // [Figma Reader 01] Trang chủ Thư viện UTC
        public IActionResult Index()
        {
            UserProfileViewModel? profile = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var studentName = User.Identity?.Name;
                if (!string.IsNullOrEmpty(studentId))
                {
                    profile = _profileService.GetProfile(studentId, studentName ?? "Bạn đọc UTC");
                }
            }

            return View("~/Views/Home/Reader/Index.cshtml", profile);
        }
    }
}
