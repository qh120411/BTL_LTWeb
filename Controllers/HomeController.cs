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

        // Trang chủ luôn mở công khai; người dùng chủ động vào đăng nhập hoặc quản trị.
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home", new { area = "Reader" });
        }

        public IActionResult Login(string? returnUrl)
        {
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
