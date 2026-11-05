using BTL_LTWEB.Models;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Demo logic xác thực (chuẩn bị kết nối CSDL sau này)
            // Phân luồng tài khoản theo kiến trúc dự án:
            // - Tài khoản quản trị (bắt đầu bằng "admin" hoặc "nv"): chuyển hướng sang Area Admin
            // - Tài khoản bạn đọc / sinh viên: chuyển hướng sang Area Reader
            if (model.Username.StartsWith("admin", StringComparison.OrdinalIgnoreCase) ||
                model.Username.StartsWith("nv", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Quản trị viên/Nhân viên đăng nhập: {Username}", model.Username);
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            _logger.LogInformation("Bạn đọc đăng nhập: {Username}", model.Username);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "Reader" });
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Xử lý đăng xuất phiên làm việc
            return RedirectToAction("Index", "Home", new { area = "Reader" });
        }
    }
}

