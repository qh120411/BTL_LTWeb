using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Reader.Services;
public interface IUserProfileService
{
    UserProfileViewModel GetProfile(string studentId, string? fallbackName = null);
    void UpdateProfile(UserProfileViewModel model);
}

public class UserProfileService(QLThuVienUtcContext db) : IUserProfileService
{
    public UserProfileViewModel GetProfile(string id, string? fallbackName = null)
    {
        var b = db.BanDocs.AsNoTracking().Include(b => b.MaLopNavigation).FirstOrDefault(b => b.MaSinhVien == id);
        if (b == null)
            return new UserProfileViewModel
            {
                MaSinhVien = id,
                HoTen = fallbackName ?? "",
                NganhHoc = "",
                LopQuanLy = ""
            };
        var loans = db.ChiTietMuons.AsNoTracking().Include(c => c.MaPhieuMuonNavigation).Include(c => c.MaBanSaoNavigation).ThenInclude(b => b.MaTaiLieuNavigation).Where(c => c.MaPhieuMuonNavigation.MaTheBanDocNavigation.MaBanDoc == b.MaBanDoc && c.NgayTra == null).ToList();
        var today = DateOnly.FromDateTime(DateTime.Today);
        return new UserProfileViewModel
        {
            MaSinhVien = id,
            HoTen = b.HoTen,
            NgaySinh = b.NgaySinh?.ToString("dd/MM/yyyy") ?? "",
            Email = b.Email,
            SoDienThoai = b.SoDienThoai,
            DiaChiNha = b.DiaChi,
            NganhHoc = "",
            LopQuanLy = b.MaLopNavigation?.TenLop ?? "",
            BorrowedBooks = loans.Select(c => new BorrowedBookItem { MaGD = c.MaChiTietMuon.ToString(), MaVach = c.MaBanSaoNavigation.MaVach, TenSach = c.MaBanSaoNavigation.MaTaiLieuNavigation.TenTaiLieu, NgayMuon = c.MaPhieuMuonNavigation.NgayMuon, HanTra = c.HanTra.ToDateTime(TimeOnly.MinValue), IsQuaHan = c.HanTra < today, SoNgayQuaHan = Math.Max(0, today.DayNumber - c.HanTra.DayNumber), TrangThaiText = c.HanTra < today ? "Quá hạn" : c.TrangThai }).ToList()
        };
    }

    public void UpdateProfile(UserProfileViewModel m)
    {
        var b = db.BanDocs.Single(b => b.MaSinhVien == m.MaSinhVien);
        b.Email = m.Email?.Trim();
        b.SoDienThoai = m.SoDienThoai?.Trim();
        b.DiaChi = m.DiaChiNha?.Trim();
        db.SaveChanges();
    }
}
