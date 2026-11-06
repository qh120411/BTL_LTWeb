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

<<<<<<< Updated upstream
        public IActionResult Index()
        {
            return View();
=======
        // Mở ứng dụng: Nếu đã đăng nhập thì tự động vào ngay Admin hoặc Reader, nếu chưa thì hiển thị trang Đăng nhập
        public IActionResult Index() => RedirectToAction("Index", "Home", new { area = "Reader" });

        public IActionResult Login(string? returnUrl)
        {
            return RedirectToAction("Login", "Account", new { returnUrl });
>>>>>>> Stashed changes
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
