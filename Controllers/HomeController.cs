using System.Diagnostics;
using BTL_LTWEB.Models;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Mở ứng dụng: Nếu đã đăng nhập thì tự động vào ngay Admin hoặc Reader, nếu chưa thì hiển thị trang Đăng nhập
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Thuthu"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home", new { area = "Reader" });
            }
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Login(string? returnUrl)
        {
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
