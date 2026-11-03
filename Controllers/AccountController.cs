using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Controllers
{
    public class AccountController : Controller
    {
        // [Figma Reader 06] Đăng nhập & phân luồng (Admin / Bạn đọc)
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password, string role = "reader")
        {
            // Phân luồng đăng nhập
            if (role.ToLower() == "admin" || username.Contains("admin") || username.Contains("lan.demo"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home", new { area = "Reader" });
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Home", new { area = "Reader" });
        }
    }
}
