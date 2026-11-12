using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Areas.Reader.Services
{
    public interface IUserProfileService
    {
        UserProfileViewModel GetProfile(string studentId, string? fallbackName = null);
        void UpdateProfile(UserProfileViewModel model);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly QLThuVienUtcContext _dbContext;

        public UserProfileService(QLThuVienUtcContext dbContext)
        {
            _dbContext = dbContext;
        }

        public UserProfileViewModel GetProfile(string studentId, string? fallbackName = null)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                return new UserProfileViewModel
                {
                    MaSinhVien = string.Empty,
                    HoTen = fallbackName ?? "Bạn đọc sinh viên",
                    AvatarUrl = "/images/default-avatar.png",
                    BorrowedBooks = new List<BorrowedBookItem>()
                };
            }

            var banDoc = _dbContext.BanDocs
                .Include(b => b.MaLopNavigation)
                .FirstOrDefault(b => b.MaSinhVien == studentId);

            if (banDoc == null)
            {
                return new UserProfileViewModel
                {
                    MaSinhVien = studentId,
                    HoTen = fallbackName ?? "Bạn đọc sinh viên",
                    AvatarUrl = "/images/default-avatar.png",
                    BorrowedBooks = new List<BorrowedBookItem>()
                };
            }

            var isLecturer = banDoc.MaSinhVien.StartsWith("GV", StringComparison.OrdinalIgnoreCase) ||
                             banDoc.MaSinhVien.StartsWith("CB", StringComparison.OrdinalIgnoreCase);

            var nganhHoc = isLecturer
                ? (banDoc.MaLopNavigation != null ? banDoc.MaLopNavigation.TenLop : "Khoa Công nghệ thông tin")
                : "CLC Công nghệ thông tin Việt - Anh";

            var lopQuanLy = isLecturer
                ? (banDoc.MaLopNavigation != null ? (banDoc.MaLopNavigation.KhoaHoc ?? "Cán bộ / Giảng viên") : "Cán bộ / Giảng viên")
                : (banDoc.MaLopNavigation != null ? banDoc.MaLopNavigation.TenLop : "K65.CLCCNTT1");

            var profile = new UserProfileViewModel
            {
                MaSinhVien = banDoc.MaSinhVien,
                HoTen = banDoc.HoTen,
                NgaySinh = banDoc.NgaySinh.HasValue ? banDoc.NgaySinh.Value.ToString("dd/MM/yyyy") : (isLecturer ? "15/01/1982" : "18/05/2006"),
                SoCCCD = $"03320600{studentId.Substring(Math.Max(0, studentId.Length - 4))}",
                NganhHoc = nganhHoc,
                LopQuanLy = lopQuanLy,
                SoDienThoai = banDoc.SoDienThoai ?? "",
                DiaChiNha = banDoc.DiaChi ?? "",
                SoTaiKhoanNganHang = banDoc.SoTaiKhoanNganHang ?? "",
                TenNganHang = banDoc.TenNganHang ?? "",
                Email = banDoc.Email ?? $"{studentId}@utc.edu.vn",
                AvatarUrl = "/images/default-avatar.png",
                BorrowedBooks = new List<BorrowedBookItem>()
            };

            // Lấy danh sách sách đang mượn từ bảng ChiTietMuon (NgayTra == null)
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
                var loanDate = (ctm.MaPhieuMuonNavigation?.NgayMuon ?? DateTime.Now).ToString("dd/MM/yyyy");
                var hanTraDate = ctm.HanTra.ToString("dd/MM/yyyy");

                var hanTraDt = ctm.HanTra.ToDateTime(TimeOnly.MinValue);
                bool isOverdue = today > hanTraDt || ctm.TrangThai == "Quá hạn";
                int daysOverdue = isOverdue ? Math.Max(1, (int)(today - hanTraDt).TotalDays) : 0;
                int daysLeft = !isOverdue ? Math.Max(0, (int)(hanTraDt - today).TotalDays) : 0;

                string authors = (book != null && book.TaiLieuTacGia.Any())
                    ? string.Join(", ", book.TaiLieuTacGia.Select(tg => tg.MaTacGiaNavigation?.HoTen).Where(h => !string.IsNullOrEmpty(h)))
                    : "Đang cập nhật";

                string coverPath = string.IsNullOrWhiteSpace(book?.AnhBia) ? "/images/covers/sach_01.jpg" : book.AnhBia;

                profile.BorrowedBooks.Add(new BorrowedBookItem
                {
                    MaGD = $"GD{(ctm.MaPhieuMuonNavigation != null ? ctm.MaPhieuMuonNavigation.NgayMuon : DateTime.Now):yyMMdd}-{ctm.MaChiTietMuon:D3}",
                    MaVach = ctm.MaBanSaoNavigation?.MaVach ?? $"BS-{ctm.MaBanSao:D5}",
                    TenSach = book?.TenTaiLieu ?? "Học liệu UTC",
                    TacGia = authors,
                    CoverImage = coverPath,
                    NgayMuon = ctm.MaPhieuMuonNavigation?.NgayMuon ?? DateTime.Now,
                    HanTra = hanTraDt,
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
            if (string.IsNullOrWhiteSpace(model.MaSinhVien))
            {
                return;
            }

            var banDoc = _dbContext.BanDocs.FirstOrDefault(b => b.MaSinhVien == model.MaSinhVien);
            if (banDoc != null)
            {
                if (!string.IsNullOrWhiteSpace(model.HoTen))
                    banDoc.HoTen = model.HoTen.Trim();
                if (!string.IsNullOrWhiteSpace(model.SoDienThoai))
                    banDoc.SoDienThoai = model.SoDienThoai.Trim();
                if (!string.IsNullOrWhiteSpace(model.DiaChiNha))
                    banDoc.DiaChi = model.DiaChiNha.Trim();
                if (!string.IsNullOrWhiteSpace(model.Email))
                    banDoc.Email = model.Email.Trim();
                if (!string.IsNullOrWhiteSpace(model.SoTaiKhoanNganHang))
                    banDoc.SoTaiKhoanNganHang = model.SoTaiKhoanNganHang.Trim();
                if (!string.IsNullOrWhiteSpace(model.TenNganHang))
                    banDoc.TenNganHang = model.TenNganHang.Trim();

                _dbContext.SaveChanges();
            }
        }
    }
}
