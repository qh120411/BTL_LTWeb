using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BTL_LTWEB.Controllers
{
    public class AccountController : Controller
    {
        // Danh sách toàn bộ 49 sinh viên lớp K65.CLCCNTT1 (CNTT Việt Anh 1) chuẩn từ CSDL
        private static readonly Dictionary<string, (string FullName, string FirstName)> Students = new(StringComparer.OrdinalIgnoreCase)
        {
            ["242630899"] = ("Nguyễn Văn An", "An"),
            ["242624495"] = ("Nguyễn Hoàng Anh", "Anh"),
            ["242630904"] = ("Phạm Tú Anh", "Anh"),
            ["242630905"] = ("Vương Duy Anh", "Anh"),
            ["242630906"] = ("Cao Đắc Bảo", "Bao"),
            ["242630909"] = ("Trương Đức Bình", "Binh"),
            ["242630910"] = ("Nguyễn Đức Cảnh", "Canh"),
            ["242630911"] = ("Dương Linh Chi", "Chi"),
            ["242630913"] = ("Nguyễn Duy Dũng", "Dung"),
            ["242630916"] = ("Hà Thế Duy", "Duy"),
            ["242630917"] = ("Nguyễn Thành Duy", "Duy"),
            ["242603895"] = ("Bạch Thành Dương", "Duong"),
            ["242630922"] = ("Nguyễn Viết Tùng Dương", "Duong"),
            ["242630924"] = ("Đinh Quang Đạt", "Dat"),
            ["242630923"] = ("Đỗ Văn Đạt", "Dat"),
            ["242630926"] = ("Lê Tiến Đạt", "Dat"),
            ["242630928"] = ("Như Mạnh Đạt", "Dat"),
            ["242603896"] = ("Đinh Việt Đăng", "Dang"),
            ["242630932"] = ("Bùi Văn Hữu Đức", "Duc"),
            ["242630933"] = ("Đào Mạnh Đức", "Duc"),
            ["242630934"] = ("Lê Minh Đức", "Duc"),
            ["242624497"] = ("Nguyễn Minh Đức", "Duc"),
            ["242630936"] = ("Phạm Minh Đức", "Duc"),
            ["242630941"] = ("Nguyễn Thị Hoài", "Hoai"),
            ["242630942"] = ("Đặng Việt Hoàn", "Hoan"),
            ["242630943"] = ("Nguyễn Văn Hùng", "Hung"),
            ["242630944"] = ("Trần Quang Huy", "Huy"),
            ["242630946"] = ("Vũ Phùng Quang Huy", "Huy"),
            ["242630954"] = ("Nguyễn Thu Kiều", "Kieu"),
            ["242630955"] = ("Nguyễn Thành Lâm", "Lam"),
            ["242630956"] = ("Trương Ngọc Lâm", "Lam"),
            ["242630957"] = ("Hà Doãn Linh", "Linh"),
            ["242630958"] = ("Phạm Ngọc Linh", "Linh"),
            ["242630964"] = ("Nguyễn Tiến Minh", "Minh"),
            ["242624500"] = ("Phạm Tiến Minh", "Minh"),
            ["242630967"] = ("Nguyễn Văn Phúc", "Phuc"),
            ["242630969"] = ("Vũ Trần Bảo Quân", "Quan"),
            ["242630970"] = ("Phăng Phú Sáng", "Sang"),
            ["242630971"] = ("Bùi Văn Sơn", "Son"),
            ["242624501"] = ("Trần Đức Tài", "Tai"),
            ["242603897"] = ("Lê Đình Thành", "Thanh"),
            ["242630976"] = ("Nguyễn Quang Thắng", "Thang"),
            ["242630978"] = ("Trần Minh Tiến", "Tien"),
            ["242630979"] = ("Đỗ Kiếm Tiêu", "Tieu"),
            ["242630983"] = ("Đặng Thanh Tùng", "Tung"),
            ["242630984"] = ("Hoàng Văn Tùng", "Tung"),
            ["242630985"] = ("Nguyễn Thị Ánh Tuyết", "Tuyet"),
            ["242630988"] = ("Lê Anh Việt", "Viet"),
            ["242630991"] = ("Nguyễn Quốc Vương", "Vuong"),
        };

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("Thuthu"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home", new { area = "Reader" });
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return View();
            }

            var cleanUser = username.Trim();
            var cleanPwd = password.Trim();

            // 1. KIỂM TRA ĐĂNG NHẬP ADMIN:
            // Admin duy nhất: tài khoản 'admin', mật khẩu 'Admin@123'
            if (cleanUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                if (cleanPwd != "Admin@123")
                {
                    ViewBag.Error = "Mật khẩu Quản trị viên không chính xác!";
                    return View();
                }

                var adminClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "admin"),
                    new Claim(ClaimTypes.Name, "Quản trị viên Hệ thống"),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(ClaimTypes.Email, "admin@utc.edu.vn")
                };

                var identity = new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.Contains("/Admin"))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            // 2. KIỂM TRA ĐĂNG NHẬP SINH VIÊN (USER/READER):
            // Tài khoản: Mã sinh viên (VD: 242630923)
            // Mật khẩu: Tên + MSV (VD: Đỗ Văn Đạt -> Dat242630923)
            if (!Students.TryGetValue(cleanUser, out var studentInfo))
            {
                ViewBag.Error = $"Mã sinh viên '{cleanUser}' không tồn tại trong danh sách bạn đọc thư viện!";
                return View();
            }

            // Mật khẩu mong đợi theo chuẩn: Tên không dấu viết hoa chữ đầu + MSV (ví dụ: Dat242630923)
            string expectedPwd = $"{studentInfo.FirstName}{cleanUser}";
            string lastWord = studentInfo.FullName.Trim().Split(' ').Last();
            string expectedPwdWithAccent = $"{lastWord}{cleanUser}";

            // Cho phép so khớp linh hoạt: Dat242630923, dat242630923 hoặc Đạt242630923
            bool isPasswordCorrect = cleanPwd.Equals(expectedPwd, StringComparison.OrdinalIgnoreCase) ||
                                     cleanPwd.Equals(expectedPwdWithAccent, StringComparison.OrdinalIgnoreCase);

            if (!isPasswordCorrect)
            {
                ViewBag.Error = $"Mật khẩu không chính xác! (Mật khẩu của bạn là: {expectedPwd})";
                return View();
            }

            // Đăng nhập thành công với vai trò Reader
            var readerClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, cleanUser),
                new Claim(ClaimTypes.Name, studentInfo.FullName),
                new Claim(ClaimTypes.Role, "Reader"),
                new Claim(ClaimTypes.Email, $"{cleanUser}@utc.edu.vn"),
                new Claim("Class", "K65.CLCCNTT1")
            };

            var readerIdentity = new ClaimsIdentity(readerClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(readerIdentity));

            // Nếu là User: Tuyệt đối không cho phép chuyển hướng sang bất kỳ trang Admin nào!
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("/Admin"))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home", new { area = "Reader" });
        }

        // Đăng nhập nhanh SSO Microsoft / Email UTC (Mặc định tài khoản Đỗ Văn Đạt - 242630923)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithMicrosoft()
        {
            var studentId = "242630923";
            var fullName = "Đỗ Văn Đạt";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, studentId),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, "Reader"),
                new Claim(ClaimTypes.Email, $"{studentId}@utc.edu.vn"),
                new Claim("Class", "K65.CLCCNTT1")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return RedirectToAction("Index", "Home", new { area = "Reader" });
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}