using System.Security.Claims;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Common;
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

    private static bool IsSuperAdmin(string? username)
    {
        return !string.IsNullOrWhiteSpace(username) &&
               string.Equals(username.Trim(), "admin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IActionResult> Index(string? keyword, string? role)
    {
        ViewBag.ActiveMenu = "User";

        var currentUsername = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "";
        bool isCurrentUserSuperAdmin = IsSuperAdmin(currentUsername);

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

            bool isCurrent = string.Equals(t.TenDangNhap, currentUsername, StringComparison.OrdinalIgnoreCase);
            bool isSuperAcc = IsSuperAdmin(t.TenDangNhap);

            // Quy tắc: Chỉ admin tối cao mới sửa quyền được và không tự sửa được chính mình
            bool canEdit = isCurrentUserSuperAdmin && !isCurrent && !isSuperAcc;

            return new StaffAccountItemViewModel
            {
                MaTaiKhoan = t.MaTaiKhoan,
                MaNV = maNv,
                HoTen = hoTen,
                TenDangNhap = t.TenDangNhap,
                Email = t.EmailDangNhap ?? "canbo@utc.edu.vn",
                VaiTroCode = t.VaiTro ?? "ADMIN",
                VaiTroText = roleText,
                TrangThai = t.TrangThai ?? "Hoạt động",
                IsCurrentLoggedInUser = isCurrent,
                IsSuperAdminAccount = isSuperAcc,
                CanEdit = canEdit
            };
        }).OrderBy(s => s.IsSuperAdminAccount ? 0 : 1).ThenBy(s => s.MaNV).ToList();

        var model = new UserManagementIndexViewModel
        {
            StaffList = staffList,
            Keyword = keyword,
            SelectedRole = role,
            IsCurrentUserSuperAdmin = isCurrentUserSuperAdmin,
            CurrentUsername = currentUsername
        };

        return View("~/Views/User/Admin/Index.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRole(int maTaiKhoan, string vaiTro)
    {
        var currentUsername = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "";
        bool isCurrentUserSuperAdmin = IsSuperAdmin(currentUsername);

        // 1. Chỉ Admin tối cao mới có quyền sửa phân quyền
        if (!isCurrentUserSuperAdmin)
        {
            return Json(new
            {
                success = false,
                message = "Từ chối quyền truy cập: Chỉ Quản trị viên tối cao (tài khoản admin) mới có quyền chỉnh sửa phân quyền cán bộ!"
            });
        }

        var targetAccount = await _dbContext.TaiKhoans
            .Include(t => t.MaNhanVienNavigation)
            .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);

        if (targetAccount == null)
        {
            return Json(new { success = false, message = "Không tìm thấy tài khoản cán bộ trong hệ thống!" });
        }

        // 2. Không được tự sửa quyền của chính mình
        if (string.Equals(targetAccount.TenDangNhap, currentUsername, StringComparison.OrdinalIgnoreCase))
        {
            return Json(new
            {
                success = false,
                message = "Bạn không thể tự chỉnh sửa quyền hạn của chính mình!"
            });
        }

        // 3. Không được sửa tài khoản Quản trị viên tối cao
        if (IsSuperAdmin(targetAccount.TenDangNhap))
        {
            return Json(new
            {
                success = false,
                message = "Không được phép thay đổi quyền hạn của tài khoản Quản trị viên tối cao!"
            });
        }

        var validRoles = new[] { "ADMIN", "QUANLY_BANDOC", "QUANLY_TAILIEU" };
        if (!validRoles.Contains(vaiTro))
        {
            return Json(new { success = false, message = "Vai trò phân quyền không hợp lệ!" });
        }

        targetAccount.VaiTro = vaiTro;
        await _dbContext.SaveChangesAsync();

        string staffName = targetAccount.MaNhanVienNavigation?.HoTen ?? targetAccount.TenDangNhap;
        string roleDisplay = vaiTro switch
        {
            "ADMIN" => "Quản trị viên (QTV)",
            "QUANLY_BANDOC" => "Quản lý bạn đọc (QLB)",
            "QUANLY_TAILIEU" => "Thủ thư mượn trả (TTM)",
            _ => vaiTro
        };

        return Json(new
        {
            success = true,
            message = $"Đã cập nhật vai trò của cán bộ '{staffName}' thành '{roleDisplay}' thành công vào CSDL SQL Server!",
            roleCode = vaiTro,
            roleText = roleDisplay
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddStaff(string maNv, string hoTen, string email, string vaiTro)
    {
        var currentUsername = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "";
        bool isCurrentUserSuperAdmin = IsSuperAdmin(currentUsername);

        if (!isCurrentUserSuperAdmin)
        {
            return Json(new
            {
                success = false,
                message = "Từ chối quyền: Chỉ Quản trị viên tối cao (tài khoản admin) mới có quyền thêm tài khoản cán bộ!"
            });
        }

        if (string.IsNullOrWhiteSpace(maNv) || string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email))
        {
            return Json(new { success = false, message = "Vui lòng nhập đầy đủ Mã NV, Họ tên và Email của cán bộ!" });
        }

        maNv = maNv.Trim().ToUpper();
        hoTen = hoTen.Trim();
        email = email.Trim().ToLower();

        if (await _dbContext.TaiKhoans.AnyAsync(t => t.TenDangNhap == maNv || t.EmailDangNhap == email))
        {
            return Json(new { success = false, message = $"Mã cán bộ '{maNv}' hoặc Email '{email}' đã tồn tại trong hệ thống!" });
        }

        // Tạo cán bộ NhanVien
        var nv = new NhanVien
        {
            MaSoNhanVien = maNv,
            HoTen = hoTen,
            ChucVu = vaiTro switch
            {
                "ADMIN" => "Quản trị viên hệ thống",
                "QUANLY_BANDOC" => "Cán bộ quản lý bạn đọc",
                _ => "Cán bộ quản lý học liệu"
            },
            Email = email,
            TrangThai = "Đang làm việc"
        };
        _dbContext.NhanViens.Add(nv);
        await _dbContext.SaveChangesAsync();

        // Mật khẩu mặc định: Tên + Mã NV (VD: LanNV26-04)
        string lastName = hoTen.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
        string defaultPassword = $"{lastName}{maNv}";

        var acc = new TaiKhoan
        {
            TenDangNhap = maNv,
            EmailDangNhap = email,
            MatKhauHash = PasswordHelper.HashPassword(defaultPassword),
            VaiTro = vaiTro,
            TrangThai = "Hoạt động",
            MaNhanVien = nv.MaNhanVien,
            NgayTao = DateTime.Now
        };
        _dbContext.TaiKhoans.Add(acc);
        await _dbContext.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = $"Đã cấp tài khoản cán bộ thành công! Mật khẩu khởi tạo: {defaultPassword}"
        });
    }
}
