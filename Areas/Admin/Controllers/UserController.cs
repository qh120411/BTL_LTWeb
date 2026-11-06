using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Admin.Controllers;

[Area("Admin")]
[BTL_LTWEB.Areas.Admin.Filters.AdminAuthorize(Roles = "ADMIN")]
public class UserController : AdminBaseController
{
    private readonly QLThuVienUtcContext _dbContext;

    public UserController(QLThuVienUtcContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(string? keyword, string? role)
    {
        ViewBag.ActiveMenu = "User";

        var query = _dbContext.TaiKhoans
            .Include(t => t.MaNhanVienNavigation)
            .Where(t => t.MaNhanVien != null || t.VaiTro == "ADMIN" || t.VaiTro == "QUANLY_BANDOC" || t.VaiTro == "QUANLY_TAILIEU")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim().ToLower();
            query = query.Where(t => t.TenDangNhap.ToLower().Contains(k) ||
                                     (t.EmailDangNhap != null && t.EmailDangNhap.ToLower().Contains(k)) ||
                                     (t.MaNhanVienNavigation != null && (t.MaNhanVienNavigation.HoTen.ToLower().Contains(k) || (t.MaNhanVienNavigation.MaSoNhanVien != null && t.MaNhanVienNavigation.MaSoNhanVien.ToLower().Contains(k)))));
        }

        if (!string.IsNullOrWhiteSpace(role) && role != "Tất cả vai trò")
        {
            query = query.Where(t => t.VaiTro == role);
        }

        var accounts = await query.ToListAsync();

        var staffList = accounts.Select(t =>
        {
            string maNv = t.MaNhanVienNavigation?.MaSoNhanVien ?? (t.TenDangNhap.StartsWith("NV", StringComparison.OrdinalIgnoreCase) ? t.TenDangNhap : "NV-QT01");
            string hoTen = t.MaNhanVienNavigation?.HoTen ?? "Quản trị viên Hệ thống";
            string roleText = t.VaiTro switch
            {
                "ADMIN" => "Quản trị viên (QTV)",
                "QUANLY_BANDOC" => "Quản lý bạn đọc (QLB)",
                "QUANLY_TAILIEU" => "Thủ thư mượn trả (TTM)",
                _ => t.VaiTro ?? "Nhân viên"
            };

            return new StaffAccountItemViewModel
            {
                MaTaiKhoan = t.MaTaiKhoan,
                MaNV = maNv,
                HoTen = hoTen,
                TenDangNhap = t.EmailDangNhap ?? t.TenDangNhap,
                Email = t.EmailDangNhap ?? "canbo@utc.edu.vn",
                VaiTroCode = t.VaiTro ?? "ADMIN",
                VaiTroText = roleText,
                TrangThai = t.TrangThai ?? "Hoạt động"
            };
        }).OrderBy(s => s.MaNV).ToList();

        var model = new UserManagementIndexViewModel
        {
            StaffList = staffList,
            Keyword = keyword,
            SelectedRole = role
        };

        return View("~/Views/User/Admin/Index.cshtml", model);
    }
}
