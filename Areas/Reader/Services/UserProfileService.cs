using System.Collections.Concurrent;
using System.Text.Json;
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
        private readonly ConcurrentDictionary<string, UserProfileViewModel> _profiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _storageFilePath;
        private readonly object _fileLock = new();
        private readonly QLThuVienUtcContext? _dbContext;

        public UserProfileService(IWebHostEnvironment env, QLThuVienUtcContext? dbContext = null)
        {
            _dbContext = dbContext;

            var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _storageFilePath = Path.Combine(dataDir, "student_profiles.json");

            LoadProfilesFromFile();
        }

        public UserProfileViewModel GetProfile(string studentId, string? fallbackName = null)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                studentId = "242630923";
            }

            // 1. ƯU TIÊN TRUY VẤN TRỰC TIẾP TỪ CSDL SQL SERVER QLThuVienUTC
            if (_dbContext != null)
            {
                try
                {
                    var banDoc = _dbContext.BanDocs
                        .Include(b => b.MaLopNavigation)
                        .FirstOrDefault(b => b.MaSinhVien == studentId);

                    if (banDoc != null)
                    {
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
                            SoDienThoai = banDoc.SoDienThoai ?? "0986123456",
                            DiaChiNha = banDoc.DiaChi ?? "Số 3 Phố Cầu Giấy, Láng Thượng, Đống Đa, Hà Nội",
                            SoTaiKhoanNganHang = "1028374659",
                            TenNganHang = "Vietcombank",
                            Email = banDoc.Email ?? $"{studentId}@utc.edu.vn",
                            AvatarUrl = "/images/default-avatar.png",
                            BorrowedBooks = new List<BorrowedBookItem>()
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

                            profile.BorrowedBooks.Add(new BorrowedBookItem
                            {
                                MaGD = $"GD{ctm.MaPhieuMuonNavigation.NgayMuon:yyMMdd}-{ctm.MaChiTietMuon:D3}",
                                MaVach = ctm.MaBanSaoNavigation?.MaVach ?? $"BS-{ctm.MaBanSao:D5}",
                                TenSach = book?.TenTaiLieu ?? "Học liệu UTC",
                                TacGia = authors,
                                CoverImage = coverPath,
                                NgayMuon = ctm.MaPhieuMuonNavigation.NgayMuon,
                                HanTra = hanTraDate,
                                IsQuaHan = isOverdue,
                                SoNgayQuaHan = daysOverdue,
                                SoNgayConLai = daysLeft,
                                TrangThaiText = isOverdue ? "Đang quá hạn" : "Đang mượn (Còn hạn)",
                                TienPhat = isOverdue ? 100_000m : 0m
                            });
                        }

                        // Cập nhật lại cache bộ nhớ
                        _profiles[studentId] = profile;
                        return profile;
                    }
                }
                catch
                {
                    // Fallback sang memory nếu lỗi kết nối DB tạm thời
                }
            }

            // 2. FALLBACK NẾU CHƯA CÓ DB KẾT NỐI
            if (!_profiles.TryGetValue(studentId, out var cachedProfile))
            {
                cachedProfile = CreateDefaultStudentProfile(studentId, fallbackName);
                _profiles[studentId] = cachedProfile;
                SaveProfilesToFile();
            }

            RefreshLoanStatuses(cachedProfile);
            return cachedProfile;
        }

        public void UpdateProfile(UserProfileViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MaSinhVien))
            {
                return;
            }

            // Cập nhật vào SQL Server nếu có DbContext
            if (_dbContext != null)
            {
                try
                {
                    var banDoc = _dbContext.BanDocs.FirstOrDefault(b => b.MaSinhVien == model.MaSinhVien);
                    if (banDoc != null)
                    {
                        if (!string.IsNullOrWhiteSpace(model.SoDienThoai))
                            banDoc.SoDienThoai = model.SoDienThoai.Trim();
                        if (!string.IsNullOrWhiteSpace(model.DiaChiNha))
                            banDoc.DiaChi = model.DiaChiNha.Trim();
                        if (!string.IsNullOrWhiteSpace(model.Email))
                            banDoc.Email = model.Email.Trim();

                        _dbContext.SaveChanges();
                    }
                }
                catch
                {
                    // Bỏ qua lỗi DB
                }
            }

            if (_profiles.TryGetValue(model.MaSinhVien, out var existing))
            {
                existing.SoDienThoai = model.SoDienThoai?.Trim();
                existing.DiaChiNha = model.DiaChiNha?.Trim();
                existing.SoTaiKhoanNganHang = model.SoTaiKhoanNganHang?.Trim();
                existing.TenNganHang = model.TenNganHang?.Trim();
                existing.Email = model.Email?.Trim();
                if (!string.IsNullOrWhiteSpace(model.AvatarUrl))
                {
                    existing.AvatarUrl = model.AvatarUrl;
                }
                if (model.BorrowedBooks != null)
                {
                    existing.BorrowedBooks = model.BorrowedBooks;
                    RefreshLoanStatuses(existing);
                }
            }
            else
            {
                RefreshLoanStatuses(model);
                _profiles[model.MaSinhVien] = model;
            }

            SaveProfilesToFile();
        }

        private UserProfileViewModel CreateDefaultStudentProfile(string studentId, string? fallbackName)
        {
            var profile = new UserProfileViewModel
            {
                MaSinhVien = studentId,
                HoTen = fallbackName ?? "Sinh viên UTC",
                NgaySinh = "15/09/2006",
                SoCCCD = $"03320600{studentId.Substring(Math.Max(0, studentId.Length - 4))}",
                NganhHoc = "CLC Công nghệ thông tin Việt - Anh",
                LopQuanLy = "CLC Công nghệ thông tin - Anh 1 K65",
                SoDienThoai = "0986123456",
                DiaChiNha = "Số 3 Phố Cầu Giấy, Láng Thượng, Đống Đa, Hà Nội",
                SoTaiKhoanNganHang = "1028374659",
                TenNganHang = "Vietcombank",
                Email = $"{studentId}@utc.edu.vn",
                BorrowedBooks = new List<BorrowedBookItem>()
            };

            RefreshLoanStatuses(profile);
            return profile;
        }

        private void RefreshLoanStatuses(UserProfileViewModel profile)
        {
            var today = DateTime.Today;

            foreach (var item in profile.BorrowedBooks)
            {
                if (today > item.HanTra)
                {
                    item.IsQuaHan = true;
                    item.SoNgayQuaHan = Math.Max(1, (int)(today - item.HanTra).TotalDays);
                    item.SoNgayConLai = 0;
                    item.TrangThaiText = "Đang quá hạn";
                    item.TienPhat = 100_000m;
                }
                else
                {
                    item.IsQuaHan = false;
                    item.SoNgayQuaHan = 0;
                    item.SoNgayConLai = (int)(item.HanTra - today).TotalDays;
                    item.TrangThaiText = "Đang mượn (Còn hạn)";
                    item.TienPhat = 0m;
                }
            }
        }

        private void LoadProfilesFromFile()
        {
            try
            {
                lock (_fileLock)
                {
                    if (File.Exists(_storageFilePath))
                    {
                        var json = File.ReadAllText(_storageFilePath);
                        var list = JsonSerializer.Deserialize<List<UserProfileViewModel>>(json);
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (!string.IsNullOrWhiteSpace(item.MaSinhVien))
                                {
                                    RefreshLoanStatuses(item);
                                    _profiles[item.MaSinhVien] = item;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback nếu có lỗi đọc file
            }
        }

        private void SaveProfilesToFile()
        {
            try
            {
                lock (_fileLock)
                {
                    var list = _profiles.Values.ToList();
                    var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_storageFilePath, json);
                }
            }
            catch
            {
                // Bỏ qua lỗi ghi disk nếu môi trường giới hạn
            }
        }
    }
}
