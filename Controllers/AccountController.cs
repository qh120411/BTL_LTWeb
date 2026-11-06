using System.Security.Claims;
using BTL_LTWEB.Common;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Controllers;
public class AccountController(QLThuVienUtcContext db) : Controller
{
    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Nhập tên đăng nhập và mật khẩu.";
            return View();
        }

        var name = username.Trim();
        var account = await db.TaiKhoans.Include(t => t.MaVaiTros).Include(t => t.MaBanDocNavigation).Include(t => t.MaNhanVienNavigation).FirstOrDefaultAsync(t => t.TenDangNhap == name || t.EmailDangNhap == name);
        if (account == null || account.TrangThai != "Hoạt động" || !PasswordHelper.Verify(password, account.MatKhauHash, out var upgrade))
        {
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.";
            return View();
        }

        var roles = account.MaVaiTros.Select(r => r.MaVaiTro.Trim().ToUpperInvariant()).ToHashSet();
        var staff = roles.Overlaps(new[] { "ADMIN", "QUANLY_BANDOC", "QUANLY_TAILIEU", "THUTHU" });
        if (!staff && account.MaBanDocNavigation == null)
        {
            ViewBag.Error = "Tài khoản chưa được liên kết với hồ sơ bạn đọc.";
            return View();
        }

        if (upgrade)
            account.MatKhauHash = PasswordHelper.HashPassword(password);
        account.LanDangNhapCuoi = DateTime.Now;
        await db.SaveChangesAsync();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.TenDangNhap),
            new(ClaimTypes.Name, account.MaBanDocNavigation?.HoTen ?? account.MaNhanVienNavigation?.HoTen ?? account.TenDangNhap),
            new("AccountId", account.MaTaiKhoan.ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        if (staff)
            claims.Add(new(ClaimTypes.Role, "AdminStaff"));
        if (roles.Contains("ADMIN"))
            claims.Add(new(ClaimTypes.Role, "Admin"));
        if (!staff)
            claims.Add(new(ClaimTypes.Role, "Reader"));
        if (account.MaNhanVien.HasValue)
            claims.Add(new("StaffId", account.MaNhanVien.Value.ToString()));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        if (staff)
            return RedirectToAction("Index", "Home", new { area = "Reader" });
        if (Url.IsLocalUrl(returnUrl) && !returnUrl!.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return LocalRedirect(returnUrl!);
        return RedirectToAction("Index", "Home", new { area = "Reader" });
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home", new { area = "Reader" });
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = 403;
        return View();
    }
}
