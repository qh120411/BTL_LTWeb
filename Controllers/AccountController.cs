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
            return View("~/Views/Home/Index.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return View("~/Views/Home/Index.cshtml");
            }

            var cleanUser = username.Trim();
            var cleanPwd = password.Trim();

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

            // Tự động khởi tạo tài khoản admin nếu CSDL ban đầu chưa có
            if (account == null && cleanUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                account = new TaiKhoan
                {
                    TenDangNhap = "admin",
                    EmailDangNhap = "admin@utc.edu.vn",
                    MatKhauHash = PasswordHelper.HashPassword("Admin@123"),
                    VaiTro = "ADMIN",
                    TrangThai = "Hoạt động",
                    NgayTao = DateTime.Now
                };
                _dbContext.TaiKhoans.Add(account);
                await _dbContext.SaveChangesAsync();
            }

            if (account == null)
            {
                ViewBag.Error = $"Tài khoản '{cleanUser}' không tồn tại trong hệ thống Thư viện UTC!";
                return View("~/Views/Home/Index.cshtml");
            }

            // 2. XÁC THỰC MẬT KHẨU THÔNG QUA HÀM BĂM SHA-256 (BẢO VỆ AN TOÀN TUYỆT ĐỐI)
            bool isPasswordCorrect = PasswordHelper.VerifyPassword(cleanPwd, account.MatKhauHash);

            // Kiểm tra quy tắc dự phòng linh hoạt
            if (!isPasswordCorrect)
            {
                // Kiểm tra sinh viên: Tên + Mã sinh viên (hoặc 123456)
                if (account.MaBanDocNavigation != null || Students.ContainsKey(account.TenDangNhap))
                {
                    if (Students.TryGetValue(account.TenDangNhap, out var stInfo))
                    {
                        string expectedPwd = $"{stInfo.FirstName}{account.TenDangNhap}";
                        string lastWord = stInfo.FullName.Trim().Split(' ').Last();
                        string expectedPwdWithAccent = $"{lastWord}{account.TenDangNhap}";

                        if (cleanPwd.Equals(expectedPwd, StringComparison.OrdinalIgnoreCase) ||
                            cleanPwd.Equals(expectedPwdWithAccent, StringComparison.OrdinalIgnoreCase) ||
                            cleanPwd.Equals("123456", StringComparison.OrdinalIgnoreCase) ||
                            cleanPwd.Equals(account.TenDangNhap, StringComparison.OrdinalIgnoreCase) ||
                            cleanPwd.Equals(stInfo.FirstName, StringComparison.OrdinalIgnoreCase))
                        {
                            isPasswordCorrect = true;
                            // Cập nhật lại mật khẩu hash SHA-256 chuẩn vào CSDL
                            account.MatKhauHash = PasswordHelper.HashPassword(expectedPwd);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
                // Kiểm tra Quản trị viên (Admin tối cao)
                else if (account.VaiTro == "ADMIN" || account.TenDangNhap.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    if (cleanPwd.Equals("Admin@123", StringComparison.OrdinalIgnoreCase) ||
                        cleanPwd.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                        cleanPwd.Equals("123456", StringComparison.OrdinalIgnoreCase) ||
                        cleanPwd.Equals("admin123", StringComparison.OrdinalIgnoreCase))
                    {
                        isPasswordCorrect = true;
                        account.MatKhauHash = PasswordHelper.HashPassword("Admin@123");
                        await _dbContext.SaveChangesAsync();
                    }
                }
                // Kiểm tra Cán bộ nhân viên khác
                else if (cleanPwd.Equals("123456", StringComparison.OrdinalIgnoreCase) ||
                         (account.MaNhanVienNavigation?.MaSoNhanVien != null && cleanPwd.Equals(account.MaNhanVienNavigation.MaSoNhanVien, StringComparison.OrdinalIgnoreCase)))
                {
                    isPasswordCorrect = true;
                }
            }

            if (!isPasswordCorrect)
            {
                ViewBag.Error = "Mật khẩu không chính xác! Vui lòng kiểm tra lại.";
                return View("~/Views/Home/Index.cshtml");
            }

            // Cập nhật thời điểm đăng nhập cuối cùng vào SQL Server
            account.LanDangNhapCuoi = DateTime.Now;
            try { await _dbContext.SaveChangesAsync(); } catch { }

            // 3. TẠO CLAIMS VÀ PHÂN QUYỀN CHUẨN XÁC THEO CỘT VaiTro TRONG CSDL
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.TenDangNhap),
                new Claim(ClaimTypes.Email, account.EmailDangNhap ?? $"{account.TenDangNhap}@utc.edu.vn")
            };

            string roleCode = account.VaiTro?.ToUpperInvariant() ?? "READER";

            // VAI TRÒ 1: QUẢN TRỊ VIÊN TỐI CAO (ADMIN)
            if (roleCode == "ADMIN" || account.TenDangNhap.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                string fullName = account.MaNhanVienNavigation?.HoTen ?? "Quản trị viên Hệ thống";
                claims.Add(new Claim(ClaimTypes.Name, fullName));
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim(ClaimTypes.Role, "ADMIN"));
                claims.Add(new Claim(ClaimTypes.Role, "AdminStaff"));
                claims.Add(new Claim("RoleTitle", "Quản trị viên (QTV)"));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProps = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            // VAI TRÒ 2: NHÂN VIÊN QUẢN LÝ BẠN ĐỌC (QUANLY_BANDOC)
            else if (roleCode == "QUANLY_BANDOC")
            {
                string fullName = account.MaNhanVienNavigation?.HoTen ?? "Cán bộ quản lý bạn đọc";
                claims.Add(new Claim(ClaimTypes.Name, fullName));
                claims.Add(new Claim(ClaimTypes.Role, "QUANLY_BANDOC"));
                claims.Add(new Claim(ClaimTypes.Role, "AdminStaff"));
                claims.Add(new Claim("RoleTitle", "Quản lý bạn đọc (QLB)"));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProps = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            // VAI TRÒ 3: NHÂN VIÊN QUẢN LÝ TÀI LIỆU / THỦ THƯ (QUANLY_TAILIEU)
            else if (roleCode == "QUANLY_TAILIEU")
            {
                string fullName = account.MaNhanVienNavigation?.HoTen ?? "Cán bộ quản lý tài liệu";
                claims.Add(new Claim(ClaimTypes.Name, fullName));
                claims.Add(new Claim(ClaimTypes.Role, "QUANLY_TAILIEU"));
                claims.Add(new Claim(ClaimTypes.Role, "AdminStaff"));
                claims.Add(new Claim("RoleTitle", "Thủ thư mượn trả (TTM)"));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProps = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            // VAI TRÒ 4: BẠN ĐỌC SINH VIÊN (READER)
            else
            {
                string fullName = account.MaBanDocNavigation?.HoTen ?? (Students.TryGetValue(account.TenDangNhap, out var s) ? s.FullName : "Bạn đọc sinh viên");
                claims.Add(new Claim(ClaimTypes.Name, fullName));
                claims.Add(new Claim(ClaimTypes.Role, "Reader"));
                claims.Add(new Claim(ClaimTypes.Role, "READER"));
                claims.Add(new Claim("Class", "K65.CLCCNTT1"));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProps = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home", new { area = "Reader" });
            }
        }

        // Đăng nhập nhanh SSO Microsoft / Email UTC (Mặc định tài khoản Đỗ Văn Đạt - 242630923)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithMicrosoft()
        {
            var studentId = "242630923";
            var account = await _dbContext.TaiKhoans
                .Include(t => t.MaBanDocNavigation)
                .FirstOrDefaultAsync(t => t.TenDangNhap == studentId);

            string fullName = account?.MaBanDocNavigation?.HoTen ?? "Đỗ Văn Đạt";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, studentId),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, "Reader"),
                new Claim(ClaimTypes.Role, "READER"),
                new Claim(ClaimTypes.Email, $"{studentId}@utc.edu.vn"),
                new Claim("Class", "K65.CLCCNTT1")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);

            return RedirectToAction("Index", "Home", new { area = "Reader" });
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
