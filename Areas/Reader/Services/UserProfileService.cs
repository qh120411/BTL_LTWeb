using System.Collections.Concurrent;
using System.Text.Json;
using BTL_LTWEB.Areas.Reader.Models;

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

        // Danh mục mẫu các đầu sách thật để cấp phát mượn ngẫu nhiên cho sinh viên
        private static readonly List<(string Title, string Authors, string Cover, string Barcode)> AvailableCatalog = new()
        {
            ("Cấu trúc dữ liệu và giải thuật (Python)", "Bùi Việt Hà", "/images/covers/sach_01.jpg", "BS-00001"),
            ("Giáo trình Cơ sở dữ liệu quan hệ", "Phạm Minh Chuẩn, Trịnh Thị Nhị", "/images/covers/sach_02.jpg", "BS-00005"),
            ("Giáo trình Mạng máy tính", "Nguyễn Thế Lộc, Nguyễn Hồng Phương", "/images/covers/sach_03.jpg", "BS-00009"),
            ("Giáo trình Cơ sở An toàn thông tin", "HV Công nghệ Bưu chính Viễn thông", "/images/covers/sach_04.jpg", "BS-00013"),
            ("Giáo trình Kỹ thuật lập trình C", "Phạm Văn Ất, Nguyễn Hiếu Cường", "/images/covers/sach_05.jpg", "BS-00017"),
            ("Giáo trình C++ và Lập trình hướng đối tượng", "Phạm Văn Ất, Lê Trường Thông", "/images/covers/sach_06.jpg", "BS-00021"),
            ("Giáo trình Trí tuệ nhân tạo, Học máy và Học sâu", "Nguyễn Tuấn Đăng", "/images/covers/sach_07.jpg", "BS-00025"),
            ("Giáo trình Kiến trúc máy tính và Hợp ngữ", "Phạm Văn Cường", "/images/covers/sach_08.jpg", "BS-00029"),
            ("Giáo trình Kỹ nghệ phần mềm", "Đoàn Văn Ban", "/images/covers/sach_09.jpg", "BS-00033"),
            ("Giáo trình Lập trình Python cơ bản và ứng dụng", "Đậu Hải Long", "/images/covers/sach_10.jpg", "BS-00037"),
            ("Giáo trình Hệ quản trị cơ sở dữ liệu SQL Server", "Nguyễn Gia Tuấn Anh", "/images/covers/sach_11.jpg", "BS-00041"),
            ("Phát triển ứng dụng Web với ASP.NET Core MVC", "Microsoft Press Vietnam", "/images/covers/sach_12.jpg", "BS-00045"),
            ("Giáo trình Toán cao cấp cho kỹ sư - Tập 1", "Nguyễn Đình Trí, Trần Việt Dũng", "/images/covers/sach_21.jpg", "BS-00081"),
            ("Giáo trình Vật lý đại cương - Cơ và Nhiệt", "Lương Duyên Bình", "/images/covers/sach_23.jpg", "BS-00089"),
            ("Xác suất thống kê và ứng dụng", "Đào Hữu Hồ", "/images/covers/sach_25.jpg", "BS-00097")
        };

        public UserProfileService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _storageFilePath = Path.Combine(dataDir, "student_profiles.json");

            LoadProfilesFromFile();
            EnsureDefaultProfiles();
        }

        public UserProfileViewModel GetProfile(string studentId, string? fallbackName = null)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                studentId = "242630923";
            }

            if (!_profiles.TryGetValue(studentId, out var profile))
            {
                profile = CreateDefaultStudentProfile(studentId, fallbackName);
                _profiles[studentId] = profile;
                SaveProfilesToFile();
            }

            // Luôn cập nhật và tính toán lại hạn trả (1 năm), trạng thái quá hạn và tiền phạt (100k/quyển)
            RefreshLoanStatuses(profile);

            return profile;
        }

        public void UpdateProfile(UserProfileViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MaSinhVien))
            {
                return;
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
            }
            else
            {
                RefreshLoanStatuses(model);
                _profiles[model.MaSinhVien] = model;
            }

            SaveProfilesToFile();
        }

        private void EnsureDefaultProfiles()
        {
            // Thiết lập chuẩn cho sinh viên Đỗ Văn Đạt (242630923):
            // Mượn đúng 5 cuốn sách: 2 cuốn còn hạn và 3 cuốn quá hạn (phạt 100k/quyển -> tổng phạt 300k)
            var datProfile = new UserProfileViewModel
            {
                MaSinhVien = "242630923",
                HoTen = "Đỗ Văn Đạt",
                NgaySinh = "18/05/2006",
                SoCCCD = "033206009004",
                NganhHoc = "CLC Công nghệ thông tin Việt - Anh",
                LopQuanLy = "CLC Công nghệ thông tin - Anh 1 K65",
                SoDienThoai = "0986123456",
                DiaChiNha = "Số 3 Phố Cầu Giấy, Phường Láng Thượng, Quận Đống Đa, Hà Nội",
                SoTaiKhoanNganHang = "1028374659",
                TenNganHang = "Vietcombank",
                Email = "242630923@utc.edu.vn",
                BorrowedBooks = new List<BorrowedBookItem>
                {
                    // 1. Cuốn 1: CÒN HẠN
                    new BorrowedBookItem
                    {
                        MaGD = "GD260315-001",
                        MaVach = "BS-00001",
                        TenSach = "Cấu trúc dữ liệu và giải thuật (Python)",
                        TacGia = "Bùi Việt Hà",
                        CoverImage = "/images/covers/sach_01.jpg",
                        NgayMuon = new DateTime(2026, 3, 15),
                        HanTra = new DateTime(2027, 3, 15) // Quy định 1 năm
                    },
                    // 2. Cuốn 2: CÒN HẠN
                    new BorrowedBookItem
                    {
                        MaGD = "GD260520-002",
                        MaVach = "BS-00037",
                        TenSach = "Giáo trình Lập trình Python cơ bản và ứng dụng",
                        TacGia = "Đậu Hải Long",
                        CoverImage = "/images/covers/sach_10.jpg",
                        NgayMuon = new DateTime(2026, 5, 20),
                        HanTra = new DateTime(2027, 5, 20) // Quy định 1 năm
                    },
                    // 3. Cuốn 3: ĐANG QUÁ HẠN (Hạn trả 10/10/2026) -> Phạt 100k
                    new BorrowedBookItem
                    {
                        MaGD = "GD251010-003",
                        MaVach = "BS-00005",
                        TenSach = "Giáo trình Cơ sở dữ liệu quan hệ",
                        TacGia = "Phạm Minh Chuẩn, Trịnh Thị Nhị",
                        CoverImage = "/images/covers/sach_02.jpg",
                        NgayMuon = new DateTime(2025, 10, 10),
                        HanTra = new DateTime(2026, 10, 10) // Quá hạn 26 ngày
                    },
                    // 4. Cuốn 4: ĐANG QUÁ HẠN (Hạn trả 25/08/2026) -> Phạt 100k
                    new BorrowedBookItem
                    {
                        MaGD = "GD250825-004",
                        MaVach = "BS-00021",
                        TenSach = "Giáo trình C++ và Lập trình hướng đối tượng",
                        TacGia = "Phạm Văn Ất, Lê Trường Thông",
                        CoverImage = "/images/covers/sach_06.jpg",
                        NgayMuon = new DateTime(2025, 8, 25),
                        HanTra = new DateTime(2026, 8, 25) // Quá hạn 72 ngày
                    },
                    // 5. Cuốn 5: ĐANG QUÁ HẠN (Hạn trả 01/11/2026) -> Phạt 100k
                    new BorrowedBookItem
                    {
                        MaGD = "GD251101-005",
                        MaVach = "BS-00033",
                        TenSach = "Giáo trình Kỹ nghệ phần mềm",
                        TacGia = "Đoàn Văn Ban",
                        CoverImage = "/images/covers/sach_09.jpg",
                        NgayMuon = new DateTime(2025, 11, 1),
                        HanTra = new DateTime(2026, 11, 1) // Quá hạn 4 ngày
                    }
                }
            };

            RefreshLoanStatuses(datProfile);
            _profiles["242630923"] = datProfile;

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
                SoDienThoai = "",
                DiaChiNha = "",
                SoTaiKhoanNganHang = "",
                TenNganHang = "",
                Email = $"{studentId}@utc.edu.vn",
                BorrowedBooks = GenerateRandomLoansForStudent(studentId)
            };

            RefreshLoanStatuses(profile);
            return profile;
        }

        private List<BorrowedBookItem> GenerateRandomLoansForStudent(string studentId)
        {
            var result = new List<BorrowedBookItem>();
            int hash = Math.Abs(studentId.GetHashCode());
            int count = (hash % 4) + 1; // 1 đến 4 cuốn ngẫu nhiên
            var rnd = new Random(hash);

            for (int i = 0; i < count; i++)
            {
                var book = AvailableCatalog[(hash + i * 3) % AvailableCatalog.Count];
                bool isOverdue = (i % 2 == 1); // Xen kẽ cuốn còn hạn và quá hạn

                DateTime ngayMuon;
                if (isOverdue)
                {
                    // Mượn từ năm ngoái (2025) -> Hạn trả năm 2026 đã quá hạn
                    int month = (rnd.Next(1, 10));
                    int day = (rnd.Next(1, 28));
                    ngayMuon = new DateTime(2025, month, day);
                }
                else
                {
                    // Mượn trong năm 2026 -> Hạn trả năm 2027 vẫn còn hạn
                    int month = (rnd.Next(1, 10));
                    int day = (rnd.Next(1, 28));
                    ngayMuon = new DateTime(2026, month, day);
                }

                result.Add(new BorrowedBookItem
                {
                    MaGD = $"GD26{studentId.Substring(Math.Max(0, studentId.Length - 4))}-{i + 1:D3}",
                    MaVach = $"{book.Barcode}-{studentId.Substring(Math.Max(0, studentId.Length - 2))}",
                    TenSach = book.Title,
                    TacGia = book.Authors,
                    CoverImage = book.Cover,
                    NgayMuon = ngayMuon,
                    HanTra = ngayMuon.AddYears(1) // Quy định thời gian mượn luôn là 1 năm
                });
            }

            return result;
        }

        private void RefreshLoanStatuses(UserProfileViewModel profile)
        {
            var today = new DateTime(2026, 11, 5); // Ngày hệ thống thực tế

            foreach (var item in profile.BorrowedBooks)
            {
                // Quy tắc mượn luôn là 1 năm
                item.HanTra = item.NgayMuon.AddYears(1);

                if (today > item.HanTra)
                {
                    item.IsQuaHan = true;
                    item.SoNgayQuaHan = (int)(today - item.HanTra).TotalDays;
                    item.SoNgayConLai = 0;
                    item.TrangThaiText = "Đang quá hạn";
                    item.TienPhat = 100_000m; // Quá hạn tiền nộp luôn là 100k/quyển
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
