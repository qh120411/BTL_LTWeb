using System.Security.Claims;
using BTL_LTWEB.Common;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly QLThuVienUtcContext _dbContext;

        public AccountController(QLThuVienUtcContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("ADMIN") || User.IsInRole("QUANLY_BANDOC") || User.IsInRole("QUANLY_TAILIEU") || User.IsInRole("Thuthu"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home", new { area = "Reader" });
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return View("~/Views/Account/Login.cshtml");
            }

            var cleanUser = username.Trim();
            var cleanPwd = password;

            // 1. TRUY VẤN TÀI KHOẢN TRỰC TIẾP TỪ BẢNG TaiKhoan TRONG CSDL SQL SERVER
            var account = await _dbContext.TaiKhoans
                .Include(t => t.MaBanDocNavigation)
                .Include(t => t.MaNhanVienNavigation)
                .FirstOrDefaultAsync(t => t.TenDangNhap == cleanUser || t.EmailDangNhap == cleanUser);

            if (account == null)
            {
                string userPrefix = cleanUser.Contains("@") ? cleanUser.Split('@')[0] : cleanUser;
                account = await _dbContext.TaiKhoans
                    .Include(t => t.MaBanDocNavigation)
                    .Include(t => t.MaNhanVienNavigation)
                    .FirstOrDefaultAsync(t => t.TenDangNhap == userPrefix);
            }

            if (account == null)
            {
                ViewBag.Error = $"Tài khoản '{cleanUser}' không tồn tại trong hệ thống Thư viện UTC!";
                return View("~/Views/Account/Login.cshtml");
            }

            // 2. XÁC THỰC MẬT KHẨU THÔNG QUA HÀM BĂM SHA-256 (BẢO VỆ AN TOÀN TUYỆT ĐỐI)
            bool isPasswordCorrect = PasswordHelper.VerifyPassword(cleanPwd, account.MatKhauHash);

            if (!isPasswordCorrect)
            {
                ViewBag.Error = "Mật khẩu không chính xác! Vui lòng kiểm tra lại.";
                return View("~/Views/Account/Login.cshtml");
            }

            if (!AccountStatus.IsActive(account.TrangThai))
            {
                ViewBag.Error = "Tài khoản đang bị khóa. Vui lòng liên hệ thư viện.";
                return View("~/Views/Account/Login.cshtml");
            }

            var roleCode = account.VaiTro?.Trim().ToUpperInvariant();
            var supportedRoles = new[] { "ADMIN", "QUANLY_BANDOC", "QUANLY_TAILIEU", "THUTHU", "READER", "BANDOC" };
            if (string.IsNullOrWhiteSpace(roleCode) || !supportedRoles.Contains(roleCode))
            {
                ViewBag.Error = "Vai trò tài khoản chưa được cấu hình hợp lệ. Vui lòng liên hệ thư viện.";
                return View("~/Views/Account/Login.cshtml");
            }
            var isReader = roleCode == "READER" || roleCode == "BANDOC";
            var fullName = isReader ? account.MaBanDocNavigation?.HoTen : account.MaNhanVienNavigation?.HoTen;
            var identifier = isReader ? account.MaBanDocNavigation?.MaSinhVien : account.TenDangNhap;
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(identifier))
            {
                ViewBag.Error = "Tài khoản chưa có hồ sơ hợp lệ. Vui lòng liên hệ thư viện.";
                return View("~/Views/Account/Login.cshtml");
            }

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, identifier),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, roleCode)
            };
            if (!string.IsNullOrWhiteSpace(account.EmailDangNhap)) claims.Add(new Claim(ClaimTypes.Email, account.EmailDangNhap));
            if (isReader)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Reader"));
                claims.Add(new Claim(ClaimTypes.Role, "READER"));
                if (!string.IsNullOrWhiteSpace(account.MaBanDocNavigation?.MaLop))
                    claims.Add(new Claim("Class", account.MaBanDocNavigation.MaLop));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "AdminStaff"));
                if (roleCode == "ADMIN") claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                if (roleCode == "THUTHU") claims.Add(new Claim(ClaimTypes.Role, "Thuthu"));
            }
            account.LanDangNhapCuoi = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProps = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) &&
                (isReader ? !returnUrl.Contains("/Admin", StringComparison.OrdinalIgnoreCase) : returnUrl.Contains("/Admin", StringComparison.OrdinalIgnoreCase)))
                return Redirect(returnUrl);
            return isReader ? RedirectToAction("Index", "Home", new { area = "Reader" }) : RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
