using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace BTL_LTWEB.Areas.Reader.Services;

public interface IUserProfileService
{
    UserProfileViewModel GetProfile(string studentId);
    void UpdateProfile(UserProfileViewModel model);
}
public class UserProfileService : IUserProfileService
{
    private readonly QLThuVienUtcContext _dbContext;
    public UserProfileService(QLThuVienUtcContext dbContext) => _dbContext = dbContext;
    public UserProfileViewModel GetProfile(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId)) throw new ArgumentException("Thiếu mã bạn đọc.", nameof(studentId));
        var banDoc = _dbContext.BanDocs.AsNoTracking().Include(b => b.MaLopNavigation).SingleOrDefault(b => b.MaSinhVien == studentId)
            ?? throw new KeyNotFoundException("Không tìm thấy hồ sơ bạn đọc trong SQL Server.");
        var profile = new UserProfileViewModel
        {
            MaSinhVien = banDoc.MaSinhVien,
            HoTen = banDoc.HoTen,
            NgaySinh = banDoc.NgaySinh?.ToString("dd/MM/yyyy") ?? "",
            SoCCCD = banDoc.SoCCCD ?? "",
            NganhHoc = banDoc.NganhHoc ?? "",
            LopQuanLy = banDoc.MaLopNavigation?.TenLop ?? "",
            SoDienThoai = banDoc.SoDienThoai,
            DiaChiNha = banDoc.DiaChi,
            Email = banDoc.Email,
            SoTaiKhoanNganHang = banDoc.SoTaiKhoanNganHang,
            TenNganHang = banDoc.TenNganHang,
            AvatarUrl = banDoc.AvatarUrl
        };
        // Đọc danh sách sách đang mượn từ bảng ChiTietMuon (chỉ lấy sách chưa trả: NgayTra == null)
        var activeLoans = _dbContext.ChiTietMuons
            .Include(c => c.MaPhieuMuonNavigation)
            .Include(c => c.MaBanSaoNavigation)
                .ThenInclude(b => b.MaTaiLieuNavigation)
                    .ThenInclude(t => t.TaiLieuTacGia)
                        .ThenInclude(tg => tg.MaTacGiaNavigation)
            .Where(c => c.MaPhieuMuonNavigation.MaBanDoc == banDoc.MaBanDoc
                     && c.NgayTra == null
                     && c.TrangThai != "Đã trả"
                     && c.TrangThai != "Đã đền sách mới"
                     && c.TrangThai != "Đã đền tiền")
            .OrderBy(c => c.HanTra)
            .ToList();

        var today = DateTime.Today;
        foreach (var ctm in activeLoans)
        {
            var book = ctm.MaBanSaoNavigation?.MaTaiLieuNavigation;
            var hanTraDate = ctm.HanTra.ToDateTime(TimeOnly.MinValue);
            var isOverdue = today > hanTraDate || ctm.TrangThai == "Quá hạn";
            var daysOverdue = isOverdue ? Math.Max(1, (int)(today - hanTraDate).TotalDays) : 0;
            var daysLeft = isOverdue ? 0 : Math.Max(0, (int)(hanTraDate - today).TotalDays);

            var authors = (book != null && book.TaiLieuTacGia.Any())
                ? string.Join(", ", book.TaiLieuTacGia.Select(tg => tg.MaTacGiaNavigation.HoTen))
                : "Nhiều tác giả";

            string coverPath = !string.IsNullOrWhiteSpace(book?.AnhBia)
                ? (book.AnhBia.StartsWith("/") ? book.AnhBia : "/" + book.AnhBia)
                : "/images/covers/cover-placeholder.png";

            var loanDate = ctm.MaPhieuMuonNavigation != null ? ctm.MaPhieuMuonNavigation.NgayMuon : DateTime.Now;

            profile.BorrowedBooks.Add(new BorrowedBookItem
            {
                MaGD = $"GD{loanDate:yyMMdd}-{ctm.MaChiTietMuon:D3}",
                MaVach = ctm.MaBanSaoNavigation?.MaVach ?? $"BS-{ctm.MaBanSao:D5}",
                TenSach = book?.TenTaiLieu ?? "Học liệu UTC",
                TacGia = authors,
                CoverImage = coverPath,
                NgayMuon = loanDate,
                HanTra = hanTraDate,
                IsQuaHan = isOverdue,
                SoNgayQuaHan = daysOverdue,
                SoNgayConLai = daysLeft,
                TrangThaiText = isOverdue ? "Đang quá hạn" : "Đang mượn (Còn hạn)",
                TienPhat = isOverdue ? 100_000m : 0m
            });
        }


        return profile;
    }
    public void UpdateProfile(UserProfileViewModel model)
    {
        var b = _dbContext.BanDocs.SingleOrDefault(b => b.MaSinhVien == model.MaSinhVien) ?? throw new KeyNotFoundException("Không tìm thấy bạn đọc.");
        b.SoDienThoai = model.SoDienThoai?.Trim(); b.DiaChi = model.DiaChiNha?.Trim(); b.Email = model.Email?.Trim();
        b.SoTaiKhoanNganHang = model.SoTaiKhoanNganHang?.Trim(); b.TenNganHang = model.TenNganHang?.Trim();
        _dbContext.SaveChanges();
    }
}
