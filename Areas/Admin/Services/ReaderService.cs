using System.Text;
using BTL_LTWEB.Areas.Admin.ViewModels;
using BTL_LTWEB.Areas.Reader.Models;
using BTL_LTWEB.Areas.Reader.Services;

namespace BTL_LTWEB.Areas.Admin.Services;

public class ReaderRecord
{
    public string MaBD { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string LopKhoa { get; set; } = string.Empty;
    public string Nhom { get; set; } = string.Empty;
    public string TrangThaiHocTap { get; set; } = string.Empty;
    public string QuyenSuDung { get; set; } = string.Empty;
    public string GioiHanSach { get; set; } = "5 cuốn";
    public string HieuLucQuyen { get; set; } = "2028-06-30";
    public int SoSachDangMuon => BorrowTransactions.Count;
    public int SoSachQuaHan => BorrowTransactions.Count(b => b.IsQuaHan);
    public decimal TongTienPhat => BorrowTransactions.Where(b => b.IsQuaHan).Sum(b => b.TienPhat);
    public List<BorrowTransactionViewModel> BorrowTransactions { get; set; } = new();
}

public class ReaderService : IReaderService
{
    private static readonly List<ReaderRecord> _readers = new();
    private static readonly List<FinancialReceiptViewModel> _financialReceipts = new();
    private static readonly object _initLock = new();
    private static bool _initialized = false;

    private readonly IBookCatalogService _catalogService;
    private readonly IUserProfileService _userProfileService;

    // Danh sách 49 sinh viên K65.CLCCNTT1 chuẩn từ CSDL QLThuVienUTC_TiengViet.sql
    private static readonly (string MaSV, string HoTen)[] RawStudents = new[]
    {
        ("242630899", "Nguyễn Văn An"),
        ("242624495", "Nguyễn Hoàng Anh"),
        ("242630904", "Phạm Tú Anh"),
        ("242630905", "Vương Duy Anh"),
        ("242630906", "Cao Đắc Bảo"),
        ("242630909", "Trương Đức Bình"),
        ("242630910", "Nguyễn Đức Cảnh"),
        ("242630911", "Dương Linh Chi"),
        ("242630913", "Nguyễn Duy Dũng"),
        ("242630916", "Hà Thế Duy"),
        ("242630917", "Nguyễn Thành Duy"),
        ("242603895", "Bạch Thành Dương"),
        ("242630922", "Nguyễn Viết Tùng Dương"),
        ("242630924", "Đinh Quang Đạt"),
        ("242630923", "Đỗ Văn Đạt"),
        ("242630926", "Lê Tiến Đạt"),
        ("242630928", "Như Mạnh Đạt"),
        ("242603896", "Đinh Việt Đăng"),
        ("242630932", "Bùi Văn Hữu Đức"),
        ("242630933", "Đào Mạnh Đức"),
        ("242630934", "Lê Minh Đức"),
        ("242624497", "Nguyễn Minh Đức"),
        ("242630936", "Phạm Minh Đức"),
        ("242630941", "Nguyễn Thị Hoài"),
        ("242630942", "Đặng Việt Hoàn"),
        ("242630943", "Nguyễn Văn Hùng"),
        ("242630944", "Trần Quang Huy"),
        ("242630946", "Vũ Phùng Quang Huy"),
        ("242630954", "Nguyễn Thu Kiều"),
        ("242630955", "Nguyễn Thành Lâm"),
        ("242630956", "Trương Ngọc Lâm"),
        ("242630957", "Hà Doãn Linh"),
        ("242630958", "Phạm Ngọc Linh"),
        ("242630964", "Nguyễn Tiến Minh"),
        ("242624500", "Phạm Tiến Minh"),
        ("242630967", "Nguyễn Văn Phúc"),
        ("242630969", "Vũ Trần Bảo Quân"),
        ("242630970", "Phăng Phú Sáng"),
        ("242630971", "Bùi Văn Sơn"),
        ("242624501", "Trần Đức Tài"),
        ("242603897", "Lê Đình Thành"),
        ("242630976", "Nguyễn Quang Thắng"),
        ("242630978", "Trần Minh Tiến"),
        ("242630979", "Đỗ Kiếm Tiêu"),
        ("242630983", "Đặng Thanh Tùng"),
        ("242630984", "Hoàng Văn Tùng"),
        ("242630985", "Nguyễn Thị Ánh Tuyết"),
        ("242630988", "Lê Anh Việt"),
        ("242630991", "Nguyễn Quốc Vương")
    };

    public ReaderService(IBookCatalogService catalogService, IUserProfileService userProfileService)
    {
        _catalogService = catalogService;
        _userProfileService = userProfileService;
        EnsureData();
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }

    private void EnsureData()
    {
        if (_initialized) return;

        lock (_initLock)
        {
            if (_initialized) return;
            _readers.Clear();

            // Khởi tạo 49 sinh viên của lớp K65.CLCCNTT1
            foreach (var s in RawStudents)
            {
                var r = new ReaderRecord
                {
                    MaBD = s.MaSV,
                    HoTen = s.HoTen,
                    LopKhoa = "K65.CLCCNTT1 · CLC CNTT Việt - Anh",
                    Nhom = "Sinh viên",
                    TrangThaiHocTap = "Đang học",
                    QuyenSuDung = "Hoạt động",
                    GioiHanSach = "5 cuốn",
                    HieuLucQuyen = "2028-06-30",
                    BorrowTransactions = new List<BorrowTransactionViewModel>()
                };

                // Cấp phát dữ liệu mượn sách thật theo CSDL:
                // 1. ĐỖ VĂN ĐẠT (242630923) - 5 cuốn: 2 còn hạn, 3 quá hạn
                if (s.MaSV == "242630923")
                {
                    r.BorrowTransactions = new List<BorrowTransactionViewModel>
                    {
                        new() { MaGD = "GD260315-001", MaSach = "BS-00001", TenSach = "Cấu trúc dữ liệu và giải thuật (Python)", TacGia = "Bùi Việt Hà", CoverImage = "/images/covers/sach_01.jpg", NgayMuon = "15/03/2026", NgayHenTra = "15/03/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 },
                        new() { MaGD = "GD260520-002", MaSach = "BS-00037", TenSach = "Giáo trình Lập trình Python cơ bản và ứng dụng", TacGia = "Đậu Hải Long", CoverImage = "/images/covers/sach_10.jpg", NgayMuon = "20/05/2026", NgayHenTra = "20/05/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 },
                        new() { MaGD = "GD251010-003", MaSach = "BS-00005", TenSach = "Giáo trình Cơ sở dữ liệu quan hệ", TacGia = "Phạm Minh Chuẩn, Trịnh Thị Nhị", CoverImage = "/images/covers/sach_02.jpg", NgayMuon = "10/10/2025", NgayHenTra = "10/10/2026", TrangThai = "Quá hạn 26 ngày", IsQuaHan = true, SoNgayQuaHan = 26, TienPhat = 100000 },
                        new() { MaGD = "GD250825-004", MaSach = "BS-00021", TenSach = "Giáo trình C++ và Lập trình hướng đối tượng", TacGia = "Phạm Văn Ất, Lê Trường Thông", CoverImage = "/images/covers/sach_06.jpg", NgayMuon = "25/08/2025", NgayHenTra = "25/08/2026", TrangThai = "Quá hạn 72 ngày", IsQuaHan = true, SoNgayQuaHan = 72, TienPhat = 100000 },
                        new() { MaGD = "GD251101-005", MaSach = "BS-00033", TenSach = "Giáo trình Kỹ nghệ phần mềm", TacGia = "Đoàn Văn Ban", CoverImage = "/images/covers/sach_09.jpg", NgayMuon = "01/11/2025", NgayHenTra = "01/11/2026", TrangThai = "Quá hạn 4 ngày", IsQuaHan = true, SoNgayQuaHan = 4, TienPhat = 100000 }
                    };
                }
                // 2. NGUYỄN VĂN AN (242630899) - 2 cuốn: 1 còn hạn, 1 quá hạn
                else if (s.MaSV == "242630899")
                {
                    r.BorrowTransactions = new List<BorrowTransactionViewModel>
                    {
                        new() { MaGD = "GD250910-006", MaSach = "BS-00009", TenSach = "Giáo trình Mạng máy tính", TacGia = "Nguyễn Thế Lộc, Nguyễn Hồng Phương", CoverImage = "/images/covers/sach_03.jpg", NgayMuon = "10/09/2025", NgayHenTra = "10/09/2026", TrangThai = "Quá hạn 56 ngày", IsQuaHan = true, SoNgayQuaHan = 56, TienPhat = 100000 },
                        new() { MaGD = "GD260410-007", MaSach = "BS-00013", TenSach = "Giáo trình Cơ sở An toàn thông tin", TacGia = "Học viện CNBCVT", CoverImage = "/images/covers/sach_04.jpg", NgayMuon = "10/04/2026", NgayHenTra = "10/04/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 }
                    };
                }
                // 3. VƯƠNG DUY ANH (242630905) - 2 cuốn: 2 quá hạn
                else if (s.MaSV == "242630905")
                {
                    r.BorrowTransactions = new List<BorrowTransactionViewModel>
                    {
                        new() { MaGD = "GD250815-008", MaSach = "BS-00017", TenSach = "Giáo trình Kỹ thuật lập trình C", TacGia = "Phạm Văn Ất", CoverImage = "/images/covers/sach_05.jpg", NgayMuon = "15/08/2025", NgayHenTra = "15/08/2026", TrangThai = "Quá hạn 82 ngày", IsQuaHan = true, SoNgayQuaHan = 82, TienPhat = 100000 },
                        new() { MaGD = "GD250920-009", MaSach = "BS-00025", TenSach = "Giáo trình Trí tuệ nhân tạo, Học máy", TacGia = "Nguyễn Tuấn Đăng", CoverImage = "/images/covers/sach_07.jpg", NgayMuon = "20/09/2025", NgayHenTra = "20/09/2026", TrangThai = "Quá hạn 46 ngày", IsQuaHan = true, SoNgayQuaHan = 46, TienPhat = 100000 }
                    };
                }
                // 4. NGUYỄN ĐỨC CẢNH (242630910) - 3 cuốn: 2 còn hạn, 1 quá hạn
                else if (s.MaSV == "242630910")
                {
                    r.BorrowTransactions = new List<BorrowTransactionViewModel>
                    {
                        new() { MaGD = "GD251005-010", MaSach = "BS-00029", TenSach = "Giáo trình Kiến trúc máy tính và Hợp ngữ", TacGia = "Phạm Văn Cường", CoverImage = "/images/covers/sach_08.jpg", NgayMuon = "05/10/2025", NgayHenTra = "05/10/2026", TrangThai = "Quá hạn 31 ngày", IsQuaHan = true, SoNgayQuaHan = 31, TienPhat = 100000 },
                        new() { MaGD = "GD260320-011", MaSach = "BS-00041", TenSach = "Giáo trình Hệ quản trị CSDL SQL Server", TacGia = "Nguyễn Gia Tuấn Anh", CoverImage = "/images/covers/sach_11.jpg", NgayMuon = "20/03/2026", NgayHenTra = "20/03/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 },
                        new() { MaGD = "GD260415-012", MaSach = "BS-00045", TenSach = "Phát triển ứng dụng Web ASP.NET Core", TacGia = "Microsoft Press", CoverImage = "/images/covers/sach_12.jpg", NgayMuon = "15/04/2026", NgayHenTra = "15/04/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 }
                    };
                }
                // 5. DƯƠNG LINH CHI (242630911) - 2 cuốn còn hạn
                else if (s.MaSV == "242630911")
                {
                    r.BorrowTransactions = new List<BorrowTransactionViewModel>
                    {
                        new() { MaGD = "GD260210-013", MaSach = "BS-00049", TenSach = "Giáo trình Nhập môn Trí tuệ nhân tạo", TacGia = "ĐHBK Hà Nội", CoverImage = "/images/covers/sach_13.jpg", NgayMuon = "10/02/2026", NgayHenTra = "10/02/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 },
                        new() { MaGD = "GD260312-014", MaSach = "BS-00053", TenSach = "Lập trình ứng dụng Di động với Flutter", TacGia = "Lê Hoàng Phúc", CoverImage = "/images/covers/sach_14.jpg", NgayMuon = "12/03/2026", NgayHenTra = "12/03/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 }
                    };
                }
                // 6. ĐINH QUANG ĐẠT (242630924) - 2 cuốn: 1 còn hạn, 1 quá hạn
                else if (s.MaSV == "242630924")
                {
                    r.BorrowTransactions = new List<BorrowTransactionViewModel>
                    {
                        new() { MaGD = "GD250925-015", MaSach = "BS-00081", TenSach = "Giáo trình Toán cao cấp cho kỹ sư", TacGia = "Nguyễn Đình Trí", CoverImage = "/images/covers/sach_21.jpg", NgayMuon = "25/09/2025", NgayHenTra = "25/09/2026", TrangThai = "Quá hạn 41 ngày", IsQuaHan = true, SoNgayQuaHan = 41, TienPhat = 100000 },
                        new() { MaGD = "GD260601-016", MaSach = "BS-00089", TenSach = "Giáo trình Vật lý đại cương - Cơ và Nhiệt", TacGia = "Lương Duyên Bình", CoverImage = "/images/covers/sach_23.jpg", NgayMuon = "01/06/2026", NgayHenTra = "01/06/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 }
                    };
                }

                _readers.Add(r);
            }

            // Thêm 1 Giảng viên mẫu
            _readers.Add(new ReaderRecord
            {
                MaBD = "GV001",
                HoTen = "TS. Lê Thu Hà",
                LopKhoa = "Khoa Công nghệ thông tin",
                Nhom = "Giảng viên",
                TrangThaiHocTap = "Đang công tác",
                QuyenSuDung = "Hoạt động",
                GioiHanSach = "10 cuốn",
                HieuLucQuyen = "2030-12-31",
                BorrowTransactions = new List<BorrowTransactionViewModel>
                {
                    new() { MaGD = "GD260115-017", MaSach = "BS-00097", TenSach = "Xác suất thống kê và ứng dụng", TacGia = "Đào Hữu Hồ", CoverImage = "/images/covers/sach_25.jpg", NgayMuon = "15/01/2026", NgayHenTra = "15/01/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 },
                    new() { MaGD = "GD260220-018", MaSach = "BS-00073", TenSach = "Giáo trình Kinh tế vi mô", TacGia = "ĐH Kinh tế Quốc dân", CoverImage = "/images/covers/sach_19.jpg", NgayMuon = "20/02/2026", NgayHenTra = "20/02/2027", TrangThai = "Còn hạn", IsQuaHan = false, TienPhat = 0 }
                }
            });

            // Khởi tạo sổ quỹ tài chính ban đầu
            if (_financialReceipts.Count == 0)
            {
                _financialReceipts.Add(new FinancialReceiptViewModel
                {
                    MaPhieuThu = "PT261015-001",
                    MaGD = "GD250810-088",
                    MaBD = "242630923",
                    NguoiNop = "Đỗ Văn Đạt",
                    TenSach = "Giáo trình Lập trình Hướng đối tượng Java",
                    LoaiKhoanThu = "Nộp muộn",
                    HinhThucXuLy = "Trả sách & nộp phạt nộp muộn",
                    SoTien = 100000m,
                    TienNopMuon = 100000m,
                    TienDenBu = 0,
                    NgayThu = "15/10/2026 09:30",
                    GhiChu = "Nộp phạt quá hạn 18 ngày khi trả sách tại quầy"
                });

                _financialReceipts.Add(new FinancialReceiptViewModel
                {
                    MaPhieuThu = "PT261022-002",
                    MaGD = "GD250912-092",
                    MaBD = "242630899",
                    NguoiNop = "Nguyễn Văn An",
                    TenSach = "Giáo trình Truyền số liệu và Mạng máy tính",
                    LoaiKhoanThu = "Đền bù mất/hỏng",
                    HinhThucXuLy = "Bồi thường bằng tiền",
                    SoTien = 150000m,
                    TienNopMuon = 0,
                    TienDenBu = 150000m,
                    NgayThu = "22/10/2026 14:15",
                    GhiChu = "Báo mất sách khi đi thực tập. Đền bù bằng tiền mặt theo giá bìa"
                });

                _financialReceipts.Add(new FinancialReceiptViewModel
                {
                    MaPhieuThu = "PT261030-003",
                    MaGD = "GD250720-095",
                    MaBD = "242630909",
                    NguoiNop = "Trương Đức Bình",
                    TenSach = "Cơ sở Dữ liệu Nâng cao",
                    LoaiKhoanThu = "Đền bù mất/hỏng",
                    HinhThucXuLy = "Đền sách mới (Đã bàn giao sách thay thế)",
                    SoTien = 0,
                    TienNopMuon = 0,
                    TienDenBu = 0,
                    NgayThu = "30/10/2026 16:45",
                    GhiChu = "Làm ướt sách. Bạn đọc tự mua và nộp lại 01 cuốn sách mới tương đương"
                });
            }

            _initialized = true;
        }
    }

    public ReaderIndexViewModel GetReaders(string? searchKeyword, string? selectedGroup)
    {
        EnsureData();
        var query = _readers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(selectedGroup) && selectedGroup != "Tất cả nhóm")
        {
            query = query.Where(r => r.Nhom.Equals(selectedGroup, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var kw = searchKeyword.Trim().ToLower();
            var kwNoDiacritics = RemoveDiacritics(kw);
            query = query.Where(r => r.MaBD.ToLower().Contains(kw) ||
                                     r.HoTen.ToLower().Contains(kw) ||
                                     RemoveDiacritics(r.HoTen.ToLower()).Contains(kwNoDiacritics) ||
                                     r.LopKhoa.ToLower().Contains(kw) ||
                                     RemoveDiacritics(r.LopKhoa.ToLower()).Contains(kwNoDiacritics));
        }

        var list = query.Select(r => new ReaderListItemViewModel
        {
            MaBD = r.MaBD,
            HoTen = r.HoTen,
            Nhom = r.Nhom,
            LopKhoa = r.LopKhoa,
            QuyenSuDung = r.QuyenSuDung,
            SoSachDangMuon = r.SoSachDangMuon,
            SoSachQuaHan = r.SoSachQuaHan
        }).ToList();

        return new ReaderIndexViewModel
        {
            SearchKeyword = searchKeyword,
            SelectedGroup = selectedGroup ?? "Tất cả nhóm",
            Readers = list
        };
    }

    public ReaderDetailViewModel? GetReaderById(string id)
    {
        EnsureData();
        var reader = _readers.FirstOrDefault(r => r.MaBD.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (reader == null) return null;

        var availableBooks = _catalogService.GetAll().ToList();

        return new ReaderDetailViewModel
        {
            MaBD = reader.MaBD,
            HoTen = reader.HoTen,
            LopKhoa = reader.LopKhoa,
            TrangThaiHocTap = reader.TrangThaiHocTap,
            Nhom = reader.Nhom,
            QuyenSuDung = reader.QuyenSuDung,
            GioiHanSach = reader.GioiHanSach,
            HieuLucQuyen = reader.HieuLucQuyen,
            BorrowTransactions = reader.BorrowTransactions.ToList(),
            AvailableBooks = availableBooks
        };
    }

    public bool UpdateReaderSettings(ReaderDetailViewModel model)
    {
        EnsureData();
        var reader = _readers.FirstOrDefault(r => r.MaBD.Equals(model.MaBD, StringComparison.OrdinalIgnoreCase));
        if (reader == null) return false;

        reader.Nhom = model.Nhom ?? reader.Nhom;
        reader.QuyenSuDung = model.QuyenSuDung ?? reader.QuyenSuDung;
        reader.GioiHanSach = model.GioiHanSach ?? reader.GioiHanSach;
        reader.HieuLucQuyen = model.HieuLucQuyen ?? reader.HieuLucQuyen;
        return true;
    }

    public bool ReturnBook(string maBD, string maGD, out string bookTitle, out decimal reducedFine)
    {
        bookTitle = string.Empty;
        reducedFine = 0;
        var reader = _readers.FirstOrDefault(r => r.MaBD.Equals(maBD, StringComparison.OrdinalIgnoreCase));
        var tx = reader?.BorrowTransactions.FirstOrDefault(t => t.MaGD.Equals(maGD, StringComparison.OrdinalIgnoreCase));
        if (tx != null)
        {
            bookTitle = tx.TenSach;
            reducedFine = tx.TienPhat;
        }

        return ProcessReturnOrCompensation(maBD, maGD, "NormalReturn", "NewBook", 0, true, null, out _);
    }

    public bool ProcessReturnOrCompensation(
        string maBD, 
        string maGD, 
        string actionType, 
        string compensationType, 
        decimal compensationAmount, 
        bool payOverdue, 
        string? note, 
        out string message)
    {
        EnsureData();
        message = string.Empty;

        var reader = _readers.FirstOrDefault(r => r.MaBD.Equals(maBD, StringComparison.OrdinalIgnoreCase));
        if (reader == null)
        {
            message = $"Không tìm thấy bạn đọc có mã '{maBD}'!";
            return false;
        }

        var tx = reader.BorrowTransactions.FirstOrDefault(t => t.MaGD.Equals(maGD, StringComparison.OrdinalIgnoreCase));
        if (tx == null)
        {
            message = "Không tìm thấy giao dịch mượn hoặc tài liệu đã được xử lý trước đó!";
            return false;
        }

        string receiptCode = $"PT{DateTime.Now:yyMMdd}-{(DateTime.Now.Ticks % 900) + 100}";
        string todayStr = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        string bookTitle = tx.TenSach;
        decimal overdueFee = (tx.IsQuaHan && payOverdue) ? 100000m : 0m;

        if (actionType.Equals("Compensation", StringComparison.OrdinalIgnoreCase))
        {
            // BỒI THƯỜNG DO MẤT HOẶC HỎNG SÁCH
            if (compensationType.Equals("NewBook", StringComparison.OrdinalIgnoreCase))
            {
                // Lựa chọn 1: Đền sách mới
                var receipt = new FinancialReceiptViewModel
                {
                    MaPhieuThu = receiptCode,
                    MaGD = maGD,
                    MaBD = reader.MaBD,
                    NguoiNop = reader.HoTen,
                    TenSach = bookTitle,
                    LoaiKhoanThu = overdueFee > 0 ? "Nộp muộn & Bồi thường sách mới" : "Đền bù mất/hỏng",
                    HinhThucXuLy = "Đền sách mới (Đã bàn giao sách thay thế)",
                    SoTien = overdueFee,
                    TienNopMuon = overdueFee,
                    TienDenBu = 0,
                    NgayThu = todayStr,
                    GhiChu = string.IsNullOrWhiteSpace(note) 
                        ? $"Báo mất/hỏng. Bạn đọc đền 01 cuốn sách mới tương đương bổ sung kho.{(overdueFee > 0 ? " Nộp kèm 100.000 đ phạt nộp muộn." : "")}" 
                        : note
                };
                _financialReceipts.Insert(0, receipt);
                message = $"Đã tiếp nhận 01 cuốn sách mới đền bù cho tài liệu '{bookTitle}' từ bạn đọc '{reader.HoTen}'.{(overdueFee > 0 ? " Thu phạt nộp muộn: 100.000 VNĐ." : "")} Giao dịch đã hoàn tất và lưu vào CSDL.";
            }
            else
            {
                // Lựa chọn 2: Đền tiền
                decimal cashCompensation = compensationAmount > 0 ? compensationAmount : 150000m;
                decimal totalCash = cashCompensation + overdueFee;

                var receipt = new FinancialReceiptViewModel
                {
                    MaPhieuThu = receiptCode,
                    MaGD = maGD,
                    MaBD = reader.MaBD,
                    NguoiNop = reader.HoTen,
                    TenSach = bookTitle,
                    LoaiKhoanThu = overdueFee > 0 ? "Đền bù mất/hỏng & Nộp muộn" : "Đền bù mất/hỏng",
                    HinhThucXuLy = "Bồi thường bằng tiền",
                    SoTien = totalCash,
                    TienNopMuon = overdueFee,
                    TienDenBu = cashCompensation,
                    NgayThu = todayStr,
                    GhiChu = string.IsNullOrWhiteSpace(note) 
                        ? $"Báo mất sách. Bồi thường tiền mặt {cashCompensation:N0} VNĐ{(overdueFee > 0 ? $" + Phạt nộp muộn {overdueFee:N0} VNĐ" : "")}." 
                        : note
                };
                _financialReceipts.Insert(0, receipt);
                message = $"Đã lập phiếu thu #{receiptCode}: Tiếp nhận đền bù bằng tiền {totalCash:N0} VNĐ (Đền sách: {cashCompensation:N0} VNĐ{(overdueFee > 0 ? $", Phạt quá hạn: {overdueFee:N0} VNĐ" : "")}) cho cuốn '{bookTitle}' từ bạn đọc '{reader.HoTen}'. Đã cộng vào quỹ tài chính thư viện.";
            }
        }
        else
        {
            // TRẢ SÁCH BÌNH THƯỜNG
            if (tx.IsQuaHan)
            {
                decimal fineAmount = 100000m; // 100.000 đ / cuốn quá hạn
                var receipt = new FinancialReceiptViewModel
                {
                    MaPhieuThu = receiptCode,
                    MaGD = maGD,
                    MaBD = reader.MaBD,
                    NguoiNop = reader.HoTen,
                    TenSach = bookTitle,
                    LoaiKhoanThu = "Nộp muộn",
                    HinhThucXuLy = "Trả sách & nộp phạt nộp muộn",
                    SoTien = fineAmount,
                    TienNopMuon = fineAmount,
                    TienDenBu = 0,
                    NgayThu = todayStr,
                    GhiChu = string.IsNullOrWhiteSpace(note) 
                        ? $"Hoàn tất trả sách và nộp phạt nộp muộn ({tx.TrangThai})" 
                        : note
                };
                _financialReceipts.Insert(0, receipt);
                message = $"Đã tiếp nhận thu hồi cuốn sách '{bookTitle}' từ bạn đọc '{reader.HoTen}' thành công! Thu phạt nộp muộn 100.000 VNĐ đã được cộng vào quỹ tài chính thư viện (Phiếu thu: #{receiptCode}).";
            }
            else
            {
                message = $"Đã tiếp nhận thu hồi cuốn sách '{bookTitle}' từ bạn đọc '{reader.HoTen}' thành công! Sách còn hạn mượn, không phát sinh chi phí.";
            }
        }

        // Xóa giao dịch mượn khỏi danh sách của bạn đọc
        reader.BorrowTransactions.Remove(tx);

        // Lưu bản sao biến để dùng an toàn trong lambda
        string titleToMatch = bookTitle;
        string gdToMatch = maGD;

        // Đồng bộ sang UserProfileService của Cổng Bạn đọc
        try
        {
            var userProfile = _userProfileService.GetProfile(maBD, reader.HoTen);
            if (userProfile != null)
            {
                var bookToRemove = userProfile.BorrowedBooks.FirstOrDefault(b => b.MaGD.Equals(gdToMatch, StringComparison.OrdinalIgnoreCase) ||
                                                                                 b.TenSach.Equals(titleToMatch, StringComparison.OrdinalIgnoreCase));
                if (bookToRemove != null)
                {
                    userProfile.BorrowedBooks.Remove(bookToRemove);
                    _userProfileService.UpdateProfile(userProfile);
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi đồng bộ nếu có
        }

        return true;
    }

    public bool BorrowBook(string maBD, int bookId, out string bookTitle, out string errorMessage)
    {
        EnsureData();
        bookTitle = string.Empty;
        errorMessage = string.Empty;

        var reader = _readers.FirstOrDefault(r => r.MaBD.Equals(maBD, StringComparison.OrdinalIgnoreCase));
        if (reader == null)
        {
            errorMessage = $"Không tìm thấy bạn đọc có mã '{maBD}'!";
            return false;
        }

        if (reader.BorrowTransactions.Count >= 5)
        {
            errorMessage = "Bạn đọc đã đạt giới hạn mượn tối đa (5 cuốn)! Vui lòng trả sách trước khi mượn thêm.";
            return false;
        }

        var book = _catalogService.GetById(bookId);
        if (book == null)
        {
            errorMessage = "Không tìm thấy học liệu được chọn trong CSDL!";
            return false;
        }

        // Quy tắc mượn: Mỗi sinh viên chỉ được mượn tối đa 1 quyển / 1 đầu sách
        if (reader.BorrowTransactions.Any(x => x.TenSach.Equals(book.Title, StringComparison.OrdinalIgnoreCase)))
        {
            errorMessage = $"Bạn đọc '{reader.HoTen}' đã mượn đầu sách '{book.Title}' rồi! Theo quy định, mỗi sinh viên chỉ được mượn 1 quyển / 1 đầu sách.";
            return false;
        }

        bookTitle = book.Title;
        string todayStr = DateTime.Today.ToString("dd/MM/yyyy");
        string dueDateStr = DateTime.Today.AddYears(1).ToString("dd/MM/yyyy"); // Thời gian mượn luôn là 1 năm
        string newTxCode = $"GD261105-{(DateTime.Now.Ticks % 900) + 100}";
        string newBarcode = $"BS-{book.Id:D3}-0{Math.Max(1, 5 - book.AvailableCopies)}";

        var newTx = new BorrowTransactionViewModel
        {
            MaGD = newTxCode,
            MaSach = newBarcode,
            TenSach = book.Title,
            TacGia = book.Authors,
            CoverImage = book.CoverImage,
            NgayMuon = todayStr,
            NgayHenTra = dueDateStr,
            TrangThai = "Còn hạn",
            IsQuaHan = false,
            TienPhat = 0
        };

        reader.BorrowTransactions.Insert(0, newTx);

        // Đồng bộ sang UserProfileService của Cổng Bạn đọc
        try
        {
            var userProfile = _userProfileService.GetProfile(maBD, reader.HoTen);
            if (userProfile != null)
            {
                userProfile.BorrowedBooks.Insert(0, new BorrowedBookItem
                {
                    MaGD = newTxCode,
                    MaVach = newBarcode,
                    TenSach = book.Title,
                    TacGia = book.Authors,
                    CoverImage = book.CoverImage,
                    NgayMuon = DateTime.Today,
                    HanTra = DateTime.Today.AddYears(1),
                    IsQuaHan = false,
                    TrangThaiText = "Đang mượn (Còn hạn)",
                    TienPhat = 0
                });
                _userProfileService.UpdateProfile(userProfile);
            }
        }
        catch
        {
            // Bỏ qua lỗi nếu có
        }

        return true;
    }

    public ImportQldtViewModel GetImportPreview()
    {
        EnsureData();
        return new ImportQldtViewModel
        {
            NguonDuLieu = "Cổng thông tin Đào tạo QLĐT - Đại học GTVT",
            TepDuLieu = "DanhSach_SinhVien_K65_CLCCNTT1.xlsx",
            PreviewItems = _readers.Where(r => r.Nhom == "Sinh viên").Take(15).Select(r => new ImportQldtItemViewModel
            {
                MaSV = r.MaBD,
                HoTen = r.HoTen,
                LopKhoa = r.LopKhoa,
                TrangThaiKiemTra = "Đã đồng bộ thành công",
                IsNew = false,
                IsValid = true
            }).ToList()
        };
    }

    public bool ProcessImport(out int updatedCount, out int insertedCount)
    {
        updatedCount = 49;
        insertedCount = 0;
        return true;
    }

    public List<CirculationTransactionItem> GetAllCirculationTransactions()
    {
        EnsureData();
        var list = new List<CirculationTransactionItem>();
        foreach (var r in _readers)
        {
            foreach (var t in r.BorrowTransactions)
            {
                list.Add(new CirculationTransactionItem
                {
                    MaGD = t.MaGD,
                    MaBD = r.MaBD,
                    HoTen = r.HoTen,
                    MaBanSao = t.MaSach,
                    TenSach = t.TenSach,
                    TacGia = t.TacGia,
                    CoverImage = t.CoverImage,
                    NgayMuon = t.NgayMuon,
                    HanTra = t.NgayHenTra,
                    TrangThai = t.TrangThai,
                    IsQuaHan = t.IsQuaHan
                });
            }
        }
        return list.OrderByDescending(x => x.IsQuaHan).ThenByDescending(x => x.NgayMuon).ToList();
    }

    public Dictionary<string, List<string>> GetReaderBorrowedTitlesMap()
    {
        EnsureData();
        return _readers.ToDictionary(r => r.MaBD, r => r.BorrowTransactions.Select(t => t.TenSach).ToList());
    }

    public List<OverdueItemViewModel> GetAllOverdueLoans()
    {
        EnsureData();
        var list = new List<OverdueItemViewModel>();
        foreach (var r in _readers)
        {
            foreach (var t in r.BorrowTransactions.Where(x => x.IsQuaHan))
            {
                int overdueDays = t.SoNgayQuaHan > 0 ? t.SoNgayQuaHan : 5;
                if (overdueDays == 5 && t.TrangThai.Contains("ngày"))
                {
                    var parts = t.TrangThai.Split(' ');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int days))
                    {
                        overdueDays = days;
                    }
                }

                list.Add(new OverdueItemViewModel
                {
                    MaGD = t.MaGD,
                    MaBD = r.MaBD,
                    HoTen = r.HoTen,
                    TenSach = t.TenSach,
                    NgayMuon = t.NgayMuon,
                    HanTra = t.NgayHenTra,
                    SoNgayQuaHan = overdueDays,
                    TienPhat = 100000m // 100k / cuốn
                });
            }
        }
        return list.OrderByDescending(x => x.SoNgayQuaHan).ToList();
    }

    public FinancialSummaryViewModel GetFinancialSummary()
    {
        EnsureData();
        return new FinancialSummaryViewModel
        {
            TongThu = _financialReceipts.Sum(r => r.SoTien),
            ThuNopMuon = _financialReceipts.Sum(r => r.TienNopMuon),
            ThuDenBu = _financialReceipts.Sum(r => r.TienDenBu),
            TongPhieuThu = _financialReceipts.Count,
            RecentReceipts = _financialReceipts.Take(10).ToList()
        };
    }

    public List<FinancialReceiptViewModel> GetAllFinancialReceipts(string? searchKeyword, string? selectedType)
    {
        EnsureData();
        var query = _financialReceipts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var kw = searchKeyword.Trim().ToLower();
            query = query.Where(r => r.MaPhieuThu.ToLower().Contains(kw) ||
                                     r.MaBD.ToLower().Contains(kw) ||
                                     r.NguoiNop.ToLower().Contains(kw) ||
                                     r.TenSach.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(selectedType) && selectedType != "Tất cả")
        {
            if (selectedType == "Nộp muộn")
            {
                query = query.Where(r => r.LoaiKhoanThu.Contains("Nộp muộn"));
            }
            else if (selectedType == "Đền bù mất/hỏng")
            {
                query = query.Where(r => r.LoaiKhoanThu.Contains("Đền bù"));
            }
        }

        return query.OrderByDescending(r => r.NgayThu).ToList();
    }

    public DashboardViewModel GetDashboardStatistics()
    {
        EnsureData();
        var allLoans = GetAllCirculationTransactions();
        var overdueLoans = GetAllOverdueLoans();
        var financial = GetFinancialSummary();

        return new DashboardViewModel
        {
            TongDauSach = 30, // 30 đầu sách thật trong CSDL
            TongBanSao = 120, // 120 bản sao vật lý (30 đầu sách x 4 bản sao)
            DangMuon = allLoans.Count,
            BanSanCo = Math.Max(0, 120 - allLoans.Count - 1),
            MatChoBoiThuong = 1,
            LuotQuaHan = overdueLoans.Count,
            TongBanDoc = _readers.Count, // 50 bạn đọc
            TongQuyTaiChinh = financial.TongThu,
            TienThuNopMuon = financial.ThuNopMuon,
            TienThuDenBu = financial.ThuDenBu,
            RecentReceipts = financial.RecentReceipts,
            RecentTransactions = allLoans.Take(8).Select(t => new DashboardTransactionItem
            {
                MaGD = t.MaGD,
                MaBD = t.MaBD,
                HoTen = t.HoTen,
                TenSach = t.TenSach,
                NgayMuon = t.NgayMuon,
                HanTra = t.HanTra,
                TrangThai = t.TrangThai,
                IsQuaHan = t.IsQuaHan
            }).ToList()
        };
    }
}
