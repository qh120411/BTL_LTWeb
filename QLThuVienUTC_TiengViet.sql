/* =========================================================================================
   CSDL QUẢN LÝ THƯ VIỆN ĐẠI HỌC GIAO THÔNG VẬN TẢI (QLThuVienUTC)
   Cấu trúc chuẩn hóa: 16 BẢNG (Chuẩn Entity Framework Core 8 / ASP.NET Core)
   Tương thích 100% với Codebase BTL_LTWEB
   Tài khoản quản trị mặc định: admin / 123456
   Tài khoản thủ thư mặc định:  thuthu / 123456
   ========================================================================================= */

IF DB_ID(N'QLThuVienUTC') IS NULL
BEGIN
    CREATE DATABASE QLThuVienUTC;
END
GO

USE QLThuVienUTC;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* 1. XÓA CÁC BẢNG CŨ THEO THỨ TỰ RÀNG BUỘC KHÓA NGOẠI */
DROP TABLE IF EXISTS dbo.ChiTietThanhToan;
DROP TABLE IF EXISTS dbo.TienPhat;
DROP TABLE IF EXISTS dbo.LichSuDatTruoc;
DROP TABLE IF EXISTS dbo.TaiKhoan_VaiTro;
DROP TABLE IF EXISTS dbo.VaiTro_Quyen;
DROP TABLE IF EXISTS dbo.Quyen;
DROP TABLE IF EXISTS dbo.VaiTro;
DROP TABLE IF EXISTS dbo.TheBanDoc;
DROP TABLE IF EXISTS dbo.QuocGia;
DROP TABLE IF EXISTS dbo.NgonNgu;
DROP TABLE IF EXISTS dbo.DinhDang;

DROP TABLE IF EXISTS dbo.ThanhToan;
DROP TABLE IF EXISTS dbo.ViPham;
DROP TABLE IF EXISTS dbo.DatTruoc;
DROP TABLE IF EXISTS dbo.GiaHan;
DROP TABLE IF EXISTS dbo.ChiTietMuon;
DROP TABLE IF EXISTS dbo.PhieuMuon;
DROP TABLE IF EXISTS dbo.BanSao;
DROP TABLE IF EXISTS dbo.TaiLieu_TacGia;
DROP TABLE IF EXISTS dbo.TaiLieu;
DROP TABLE IF EXISTS dbo.TaiKhoan;
DROP TABLE IF EXISTS dbo.BanDoc;
DROP TABLE IF EXISTS dbo.NhanVien;
DROP TABLE IF EXISTS dbo.TacGia;
DROP TABLE IF EXISTS dbo.TheLoai;
DROP TABLE IF EXISTS dbo.NhaXuatBan;
DROP TABLE IF EXISTS dbo.Lop;
GO

/* =========================================================================================
   2. TẠO 16 BẢNG CHUẨN
   ========================================================================================= */

-- Bảng 1: Lop
CREATE TABLE dbo.Lop (
    MaLop    VARCHAR(20) NOT NULL PRIMARY KEY,
    TenLop   NVARCHAR(100) NOT NULL,
    KhoaHoc  NVARCHAR(30) NULL,
    GhiChu   NVARCHAR(255) NULL
);
GO

-- Bảng 2: NhaXuatBan
CREATE TABLE dbo.NhaXuatBan (
    MaNhaXuatBan INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenNhaXuatBan NVARCHAR(150) NOT NULL,
    DiaChi       NVARCHAR(255) NULL,
    SoDienThoai  VARCHAR(20) NULL,
    QuocGia      NVARCHAR(100) NULL
);
GO

-- Bảng 3: TheLoai
CREATE TABLE dbo.TheLoai (
    MaTheLoai    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenTheLoai   NVARCHAR(100) NOT NULL,
    MaTheLoaiCha INT NULL FOREIGN KEY REFERENCES dbo.TheLoai(MaTheLoai)
);
GO

-- Bảng 4: TacGia
CREATE TABLE dbo.TacGia (
    MaTacGia INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HoTen    NVARCHAR(100) NOT NULL,
    QuocTich NVARCHAR(100) NULL
);
GO

-- Bảng 5: NhanVien
CREATE TABLE dbo.NhanVien (
    MaNhanVien   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaSoNhanVien VARCHAR(20) NOT NULL UNIQUE,
    HoTen        NVARCHAR(100) NOT NULL,
    ChucVu       NVARCHAR(50) NULL,
    Email        VARCHAR(150) NULL,
    SoDienThoai  VARCHAR(20) NULL,
    GioiTinh     NVARCHAR(10) NULL,
    TrangThai    NVARCHAR(30) NOT NULL DEFAULT N'Hoạt động'
);
GO

-- Bảng 6: BanDoc (Đã tích hợp Thẻ bạn đọc)
CREATE TABLE dbo.BanDoc (
    MaBanDoc      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaSinhVien    VARCHAR(15) NOT NULL UNIQUE,
    HoTen         NVARCHAR(100) NOT NULL,
    NgaySinh      DATE NULL,
    GioiTinh      NVARCHAR(10) NULL,
    DiaChi        NVARCHAR(255) NULL,
    SoDienThoai   VARCHAR(20) NULL,
    Email         VARCHAR(150) NULL,
    MaLop         VARCHAR(20) NULL FOREIGN KEY REFERENCES dbo.Lop(MaLop),
    TrangThai     NVARCHAR(30) NOT NULL DEFAULT N'Hoạt động',
    NgayTao       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    SoThe         NVARCHAR(50) NULL,
    NgayCapThe    DATE NULL,
    NgayHetHanThe DATE NULL,
    TrangThaiThe  NVARCHAR(60) NULL
);
CREATE INDEX IX_BanDoc_HoTen ON dbo.BanDoc(HoTen);
CREATE INDEX IX_BanDoc_MaLop ON dbo.BanDoc(MaLop);
GO

-- Bảng 7: TaiKhoan (Tích hợp cột VaiTro phân quyền trực tiếp)
CREATE TABLE dbo.TaiKhoan (
    MaTaiKhoan      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenDangNhap     VARCHAR(50) NOT NULL UNIQUE,
    EmailDangNhap   VARCHAR(150) NULL,
    MatKhauHash     VARCHAR(255) NOT NULL,
    MaBanDoc        INT NULL UNIQUE FOREIGN KEY REFERENCES dbo.BanDoc(MaBanDoc),
    MaNhanVien      INT NULL UNIQUE FOREIGN KEY REFERENCES dbo.NhanVien(MaNhanVien),
    TrangThai       NVARCHAR(30) NOT NULL DEFAULT N'Hoạt động',
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    LanDangNhapCuoi DATETIME2 NULL,
    VaiTro          NVARCHAR(50) NULL
);
GO

-- Bảng 8: TaiLieu
CREATE TABLE dbo.TaiLieu (
    MaTaiLieu    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ISBN         VARCHAR(20) NULL,
    TenTaiLieu   NVARCHAR(255) NOT NULL,
    MaNhaXuatBan INT NULL FOREIGN KEY REFERENCES dbo.NhaXuatBan(MaNhaXuatBan),
    MaTheLoai    INT NULL FOREIGN KEY REFERENCES dbo.TheLoai(MaTheLoai),
    NamXuatBan   SMALLINT NULL,
    LanXuatBan   INT NOT NULL DEFAULT 1,
    SoTrang      INT NULL,
    MoTa         NVARCHAR(MAX) NULL,
    AnhBia       NVARCHAR(255) NULL,
    NgonNgu      NVARCHAR(100) NULL,
    DinhDang     NVARCHAR(100) NULL,
    NgayTao      DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE INDEX IX_TaiLieu_Ten ON dbo.TaiLieu(TenTaiLieu);
GO

-- Bảng 9: TaiLieu_TacGia
CREATE TABLE dbo.TaiLieu_TacGia (
    MaTaiLieu    INT NOT NULL FOREIGN KEY REFERENCES dbo.TaiLieu(MaTaiLieu),
    MaTacGia     INT NOT NULL FOREIGN KEY REFERENCES dbo.TacGia(MaTacGia),
    VaiTroTacGia NVARCHAR(50) NOT NULL DEFAULT N'Tác giả',
    PRIMARY KEY (MaTaiLieu, MaTacGia)
);
GO

-- Bảng 10: BanSao
CREATE TABLE dbo.BanSao (
    MaBanSao  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaTaiLieu INT NOT NULL FOREIGN KEY REFERENCES dbo.TaiLieu(MaTaiLieu),
    MaVach    VARCHAR(50) NOT NULL UNIQUE,
    ViTriKe   NVARCHAR(100) NULL,
    TinhTrang NVARCHAR(30) NOT NULL DEFAULT N'Tốt',
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'Có sẵn',
    NgayNhap  DATE NULL
);
CREATE INDEX IX_BanSao_MaTaiLieu ON dbo.BanSao(MaTaiLieu);
GO

-- Bảng 11: PhieuMuon (Liên kết trực tiếp tới BanDoc)
CREATE TABLE dbo.PhieuMuon (
    MaPhieuMuon INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaBanDoc    INT NULL FOREIGN KEY REFERENCES dbo.BanDoc(MaBanDoc),
    MaNhanVien  INT NOT NULL FOREIGN KEY REFERENCES dbo.NhanVien(MaNhanVien),
    NgayMuon    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TrangThai   NVARCHAR(30) NOT NULL DEFAULT N'Đang mượn',
    GhiChu      NVARCHAR(255) NULL
);
CREATE INDEX IX_PhieuMuon_BanDoc ON dbo.PhieuMuon(MaBanDoc);
GO

-- Bảng 12: ChiTietMuon
CREATE TABLE dbo.ChiTietMuon (
    MaChiTietMuon   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaPhieuMuon     INT NOT NULL FOREIGN KEY REFERENCES dbo.PhieuMuon(MaPhieuMuon),
    MaBanSao        INT NOT NULL FOREIGN KEY REFERENCES dbo.BanSao(MaBanSao),
    HanTra          DATE NOT NULL,
    NgayTra         DATETIME2 NULL,
    TinhTrangKhiTra NVARCHAR(100) NULL,
    TrangThai       NVARCHAR(30) NOT NULL DEFAULT N'Đang mượn'
);
CREATE INDEX IX_ChiTietMuon_BanSao ON dbo.ChiTietMuon(MaBanSao);
GO

-- Bảng 13: GiaHan
CREATE TABLE dbo.GiaHan (
    MaGiaHan         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaChiTietMuon    INT NOT NULL FOREIGN KEY REFERENCES dbo.ChiTietMuon(MaChiTietMuon),
    HanTraCu         DATE NOT NULL,
    HanTraMoi        DATE NOT NULL,
    MaNhanVienDuyet  INT NOT NULL FOREIGN KEY REFERENCES dbo.NhanVien(MaNhanVien),
    NgayGiaHan       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    LyDo             NVARCHAR(255) NULL
);
GO

-- Bảng 14: DatTruoc
CREATE TABLE dbo.DatTruoc (
    MaDatTruoc INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaBanDoc   INT NOT NULL FOREIGN KEY REFERENCES dbo.BanDoc(MaBanDoc),
    MaTaiLieu  INT NOT NULL FOREIGN KEY REFERENCES dbo.TaiLieu(MaTaiLieu),
    NgayDat    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    HanNhan    DATETIME2 NULL,
    TrangThai  NVARCHAR(30) NOT NULL DEFAULT N'Chờ xử lý'
);
GO

-- Bảng 15: ViPham (Tích hợp tiền phạt & trạng thái nộp)
CREATE TABLE dbo.ViPham (
    MaViPham        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaBanDoc        INT NOT NULL FOREIGN KEY REFERENCES dbo.BanDoc(MaBanDoc),
    MaChiTietMuon   INT NULL FOREIGN KEY REFERENCES dbo.ChiTietMuon(MaChiTietMuon),
    LoaiViPham      NVARCHAR(50) NOT NULL,
    MoTa            NVARCHAR(255) NULL,
    NgayViPham      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    SoTienPhat      DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThaiNop    NVARCHAR(60) NOT NULL DEFAULT N'Chưa nộp',
    NgayNop         DATETIME2 NULL,
    MaNhanVienThu   INT NULL FOREIGN KEY REFERENCES dbo.NhanVien(MaNhanVien)
);
GO

-- Bảng 16: ThanhToan
CREATE TABLE dbo.ThanhToan (
    MaThanhToan    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaBanDoc       INT NOT NULL FOREIGN KEY REFERENCES dbo.BanDoc(MaBanDoc),
    MaNhanVien     INT NOT NULL FOREIGN KEY REFERENCES dbo.NhanVien(MaNhanVien),
    NgayThanhToan  DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TongTien       DECIMAL(18,2) NOT NULL,
    PhuongThuc     NVARCHAR(50) NOT NULL DEFAULT N'Tiền mặt',
    TrangThai      NVARCHAR(30) NOT NULL DEFAULT N'Thành công',
    NoiDung        NVARCHAR(500) NULL
);
GO


/* =========================================================================================
   3. NẠP DỮ LIỆU MẪU (SEED DATA CHUẨN 100%)
   ========================================================================================= */

-- Dữ liệu bảng [Lop] (7 bản ghi)
INSERT INTO dbo.[Lop] ([MaLop], [TenLop], [KhoaHoc], [GhiChu]) VALUES
(N'K65.CLCCNTT1', N'K65 Chất lượng cao Công nghệ thông tin 1', N'K65', N'Lớp lấy từ danh sách thi người dùng cung cấp'),
(N'KHOA.CK', N'Khoa Cơ khí', N'Cán bộ / Giảng viên', N'Khoa Cơ khí - ĐH Giao thông Vận tải'),
(N'KHOA.CNTT', N'Khoa Công nghệ thông tin', N'Cán bộ / Giảng viên', N'Khoa Công nghệ thông tin - ĐH Giao thông Vận tải'),
(N'KHOA.CT', N'Khoa Công trình', N'Cán bộ / Giảng viên', N'Khoa Công trình - ĐH Giao thông Vận tải'),
(N'KHOA.DDT', N'Khoa Điện - Điện tử', N'Cán bộ / Giảng viên', N'Khoa Điện - Điện tử - ĐH Giao thông Vận tải'),
(N'KHOA.KHCB', N'Khoa Khoa học Cơ bản', N'Cán bộ / Giảng viên', N'Khoa Khoa học Cơ bản - ĐH Giao thông Vận tải'),
(N'KHOA.VTKT', N'Khoa Vận tải - Kinh tế', N'Cán bộ / Giảng viên', N'Khoa Vận tải - Kinh tế - ĐH Giao thông Vận tải');
GO

-- Dữ liệu bảng [NhaXuatBan] (11 bản ghi)
SET IDENTITY_INSERT dbo.[NhaXuatBan] ON;
INSERT INTO dbo.[NhaXuatBan] ([MaNhaXuatBan], [TenNhaXuatBan], [DiaChi], [SoDienThoai], [QuocGia]) VALUES
(1, N'NXB Bách Khoa Hà Nội', NULL, NULL, N'Việt Nam'),
(2, N'NXB Chính Trị Quốc Gia Sự Thật', NULL, NULL, N'Việt Nam'),
(3, N'NXB Giao Thông Vận Tải', NULL, NULL, N'Việt Nam'),
(4, N'NXB Giáo Dục Việt Nam', NULL, NULL, N'Việt Nam'),
(5, N'NXB Khoa Học Và Kỹ Thuật', NULL, NULL, N'Việt Nam'),
(6, N'NXB Lao Động', NULL, NULL, N'Việt Nam'),
(7, N'NXB Thông Tin Và Truyền Thông', NULL, NULL, N'Việt Nam'),
(8, N'NXB Tài Chính', NULL, NULL, N'Việt Nam'),
(9, N'NXB Xây Dựng', NULL, NULL, N'Việt Nam'),
(10, N'NXB Đại Học Quốc Gia Hà Nội', NULL, NULL, N'Việt Nam'),
(11, N'NXB Đại Học Sư Phạm', NULL, NULL, N'Việt Nam');
SET IDENTITY_INSERT dbo.[NhaXuatBan] OFF;
GO

-- Dữ liệu bảng [TheLoai] (5 bản ghi)
SET IDENTITY_INSERT dbo.[TheLoai] ON;
INSERT INTO dbo.[TheLoai] ([MaTheLoai], [TenTheLoai], [MaTheLoaiCha]) VALUES
(1, N'Công nghệ thông tin', NULL),
(2, N'Khoa học cơ bản & Toán', NULL),
(3, N'Kỹ thuật công trình & Giao thông', NULL),
(4, N'Kinh tế & Logistics', NULL),
(5, N'Lý luận chính trị & Pháp luật', NULL);
SET IDENTITY_INSERT dbo.[TheLoai] OFF;
GO

-- Dữ liệu bảng [TacGia] (37 bản ghi)
SET IDENTITY_INSERT dbo.[TacGia] ON;
INSERT INTO dbo.[TacGia] ([MaTacGia], [HoTen], [NamSinh], [QuocTich]) VALUES
(1, N'Bùi Việt Hà', NULL, N'Việt Nam'),
(2, N'Phạm Minh Chuẩn', NULL, N'Việt Nam'),
(3, N'Trịnh Thị Nhị', NULL, N'Việt Nam'),
(4, N'Nguyễn Văn Quyết', NULL, N'Việt Nam'),
(5, N'Nguyễn Thế Lộc', NULL, N'Việt Nam'),
(6, N'Nguyễn Hồng Phương', NULL, N'Việt Nam'),
(7, N'Học viện Công nghệ Bưu chính Viễn thông', NULL, N'Việt Nam'),
(8, N'Phạm Văn Ất', NULL, N'Việt Nam'),
(9, N'Nguyễn Hiếu Cường', NULL, N'Việt Nam'),
(10, N'Lê Trường Thông', NULL, N'Việt Nam'),
(11, N'Nguyễn Tuấn Đăng', NULL, N'Việt Nam'),
(12, N'Phạm Văn Cường', NULL, N'Việt Nam'),
(13, N'Đoàn Văn Ban', NULL, N'Việt Nam'),
(14, N'Đậu Hải Long', NULL, N'Việt Nam'),
(15, N'Nguyễn Đình Trí', NULL, N'Việt Nam'),
(16, N'Trần Việt Dũng', NULL, N'Việt Nam'),
(17, N'Trần Xuân Hiển', NULL, N'Việt Nam'),
(18, N'Tống Đình Quỳ', NULL, N'Việt Nam'),
(19, N'Lương Duyên Bình', NULL, N'Việt Nam'),
(20, N'Nguyễn Văn Vượng', NULL, N'Việt Nam'),
(21, N'Lê Ngọc Hồng', NULL, N'Việt Nam'),
(22, N'Đỗ Sanh', NULL, N'Việt Nam'),
(23, N'Vũ Công Ngữ', NULL, N'Việt Nam'),
(24, N'Nguyễn Văn Quảng', NULL, N'Việt Nam'),
(25, N'Dương Học Hải', NULL, N'Việt Nam'),
(26, N'Nguyễn Quang Chiêu', NULL, N'Việt Nam'),
(27, N'Nguyễn Đình Cống', NULL, N'Việt Nam'),
(28, N'Bùi Hải', NULL, N'Việt Nam'),
(29, N'Trần Thế Sơn', NULL, N'Việt Nam'),
(30, N'Lê Cung', NULL, N'Việt Nam'),
(31, N'Michael H. Hugos', NULL, N'Việt Nam'),
(32, N'Đặng Đình Đào', NULL, N'Việt Nam'),
(33, N'Vũ Trọng Phong', NULL, N'Việt Nam'),
(34, N'Trần Thị Hoa', NULL, N'Việt Nam'),
(35, N'Nguyễn Văn Công', NULL, N'Việt Nam'),
(36, N'Nguyễn Thị Loan', NULL, N'Việt Nam'),
(37, N'Bộ Giáo Dục Và Đào Tạo', NULL, N'Việt Nam');
SET IDENTITY_INSERT dbo.[TacGia] OFF;
GO

-- Dữ liệu bảng [NhanVien] (5 bản ghi)
SET IDENTITY_INSERT dbo.[NhanVien] ON;
INSERT INTO dbo.[NhanVien] ([MaNhanVien], [MaSoNhanVien], [HoTen], [NgaySinh], [GioiTinh], [SoDienThoai], [Email], [ChucVu], [TrangThai]) VALUES
(1, N'NV001', N'Trần Văn Quản Trị', NULL, NULL, NULL, NULL, N'Quản trị viên hệ thống', N'Đang làm việc'),
(2, N'NV002', N'Lê Thị Thủ Thư', NULL, NULL, NULL, NULL, N'Cán bộ nghiệp vụ lưu thông', N'Đang làm việc'),
(3, N'NV26-01', N'Nguyễn Thị Lan', NULL, NULL, NULL, N'lan.demo@utc.edu.vn', N'Quản trị viên hệ thống', N'Đang làm việc'),
(4, N'NV26-02', N'Trần Văn Minh', NULL, NULL, NULL, N'bandoc.demo@utc.edu.vn', N'Cán bộ quản lý bạn đọc', N'Đang làm việc'),
(5, N'NV26-03', N'Lê Thị Hà', NULL, NULL, NULL, N'thuthu.demo@utc.edu.vn', N'Cán bộ quản lý học liệu', N'Đang làm việc');
SET IDENTITY_INSERT dbo.[NhanVien] OFF;
GO

-- Dữ liệu bảng [BanDoc] (68 bản ghi)
SET IDENTITY_INSERT dbo.[BanDoc] ON;
INSERT INTO dbo.[BanDoc] ([MaBanDoc], [MaSinhVien], [HoTen], [NgaySinh], [GioiTinh], [DiaChi], [SoDienThoai], [Email], [MaLop], [TrangThai], [NgayTao], [SoThe], [NgayCapThe], [NgayHetHanThe], [TrangThaiThe]) VALUES
(1, N'242630899', N'Nguyễn Văn An', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630899', '2024-09-01', '2028-06-30', N'Hoạt động'),
(2, N'242624495', N'Nguyễn Hoàng Anh', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242624495', '2024-09-01', '2028-06-30', N'Hoạt động'),
(3, N'242630904', N'Phạm Tú Anh', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630904', '2024-09-01', '2028-06-30', N'Hoạt động'),
(4, N'242630905', N'Vương Duy Anh', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630905', '2024-09-01', '2028-06-30', N'Hoạt động'),
(5, N'242630906', N'Cao Đắc Bảo', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630906', '2024-09-01', '2028-06-30', N'Hoạt động'),
(6, N'242630909', N'Trương Đức Bình', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630909', '2024-09-01', '2028-06-30', N'Hoạt động'),
(7, N'242630910', N'Nguyễn Đức Cảnh', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630910', '2024-09-01', '2028-06-30', N'Hoạt động'),
(8, N'242630911', N'Dương Linh Chi', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630911', '2024-09-01', '2028-06-30', N'Hoạt động'),
(9, N'242630913', N'Nguyễn Duy Dũng', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630913', '2024-09-01', '2028-06-30', N'Hoạt động'),
(10, N'242630916', N'Hà Thế Duy', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630916', '2024-09-01', '2028-06-30', N'Hoạt động'),
(11, N'242630917', N'Nguyễn Thành Duy', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630917', '2024-09-01', '2028-06-30', N'Hoạt động'),
(12, N'242603895', N'Bạch Thành Dương', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242603895', '2024-09-01', '2028-06-30', N'Hoạt động'),
(13, N'242630922', N'Nguyễn Viết Tùng Dương', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630922', '2024-09-01', '2028-06-30', N'Hoạt động'),
(14, N'242630924', N'Đinh Quang Đạt', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630924', '2024-09-01', '2028-06-30', N'Hoạt động'),
(15, N'242630923', N'Đỗ Văn Đạt', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630923', '2024-09-01', '2028-06-30', N'Hoạt động'),
(16, N'242630926', N'Lê Tiến Đạt', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630926', '2024-09-01', '2028-06-30', N'Hoạt động'),
(17, N'242630928', N'Như Mạnh Đạt', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630928', '2024-09-01', '2028-06-30', N'Hoạt động'),
(18, N'242603896', N'Đinh Việt Đăng', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242603896', '2024-09-01', '2028-06-30', N'Hoạt động'),
(19, N'242630932', N'Bùi Văn Hữu Đức', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630932', '2024-09-01', '2028-06-30', N'Hoạt động'),
(20, N'242630933', N'Đào Mạnh Đức', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630933', '2024-09-01', '2028-06-30', N'Hoạt động'),
(21, N'242630934', N'Lê Minh Đức', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630934', '2024-09-01', '2028-06-30', N'Hoạt động'),
(22, N'242624497', N'Nguyễn Minh Đức', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242624497', '2024-09-01', '2028-06-30', N'Hoạt động'),
(23, N'242630936', N'Phạm Minh Đức', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630936', '2024-09-01', '2028-06-30', N'Hoạt động'),
(24, N'242630941', N'Nguyễn Thị Hoài', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630941', '2024-09-01', '2028-06-30', N'Hoạt động'),
(25, N'242630942', N'Đặng Việt Hoàn', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630942', '2024-09-01', '2028-06-30', N'Hoạt động'),
(26, N'242630943', N'Nguyễn Văn Hùng', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630943', '2024-09-01', '2028-06-30', N'Hoạt động'),
(27, N'242630944', N'Trần Quang Huy', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630944', '2024-09-01', '2028-06-30', N'Hoạt động'),
(28, N'242630946', N'Vũ Phùng Quang Huy', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630946', '2024-09-01', '2028-06-30', N'Hoạt động'),
(29, N'242630954', N'Nguyễn Thu Kiều', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630954', '2024-09-01', '2028-06-30', N'Hoạt động'),
(30, N'242630955', N'Nguyễn Thành Lâm', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630955', '2024-09-01', '2028-06-30', N'Hoạt động'),
(31, N'242630956', N'Trương Ngọc Lâm', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630956', '2024-09-01', '2028-06-30', N'Hoạt động'),
(32, N'242630957', N'Hà Doãn Linh', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630957', '2024-09-01', '2028-06-30', N'Hoạt động'),
(33, N'242630958', N'Phạm Ngọc Linh', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630958', '2024-09-01', '2028-06-30', N'Hoạt động'),
(34, N'242630964', N'Nguyễn Tiến Minh', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630964', '2024-09-01', '2028-06-30', N'Hoạt động'),
(35, N'242624500', N'Phạm Tiến Minh', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242624500', '2024-09-01', '2028-06-30', N'Hoạt động'),
(36, N'242630967', N'Nguyễn Văn Phúc', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630967', '2024-09-01', '2028-06-30', N'Hoạt động'),
(37, N'242630969', N'Vũ Trần Bảo Quân', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630969', '2024-09-01', '2028-06-30', N'Hoạt động'),
(38, N'242630970', N'Phăng Phú Sáng', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630970', '2024-09-01', '2028-06-30', N'Hoạt động'),
(39, N'242630971', N'Bùi Văn Sơn', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630971', '2024-09-01', '2028-06-30', N'Hoạt động'),
(40, N'242624501', N'Trần Đức Tài', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242624501', '2024-09-01', '2028-06-30', N'Hoạt động');
INSERT INTO dbo.[BanDoc] ([MaBanDoc], [MaSinhVien], [HoTen], [NgaySinh], [GioiTinh], [DiaChi], [SoDienThoai], [Email], [MaLop], [TrangThai], [NgayTao], [SoThe], [NgayCapThe], [NgayHetHanThe], [TrangThaiThe]) VALUES
(41, N'242603897', N'Lê Đình Thành', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242603897', '2024-09-01', '2028-06-30', N'Hoạt động'),
(42, N'242630976', N'Nguyễn Quang Thắng', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630976', '2024-09-01', '2028-06-30', N'Hoạt động'),
(43, N'242630978', N'Trần Minh Tiến', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630978', '2024-09-01', '2028-06-30', N'Hoạt động'),
(44, N'242630979', N'Đỗ Kiếm Tiêu', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630979', '2024-09-01', '2028-06-30', N'Hoạt động'),
(45, N'242630983', N'Đặng Thanh Tùng', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630983', '2024-09-01', '2028-06-30', N'Hoạt động'),
(46, N'242630984', N'Hoàng Văn Tùng', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630984', '2024-09-01', '2028-06-30', N'Hoạt động'),
(47, N'242630985', N'Nguyễn Thị Ánh Tuyết', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630985', '2024-09-01', '2028-06-30', N'Hoạt động'),
(48, N'242630988', N'Lê Anh Việt', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630988', '2024-09-01', '2028-06-30', N'Hoạt động'),
(49, N'242630991', N'Nguyễn Quốc Vương', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242630991', '2024-09-01', '2028-06-30', N'Hoạt động'),
(50, N'GV01', N'Nguyễn Văn Tuấn', '1980-04-12', N'Nam', N'Số 15 Cầu Giấy, Láng Thượng, Đống Đa, Hà Nội', N'0912111001', N'tuan.nv@utc.edu.vn', N'KHOA.CNTT', N'Hoạt động', '2026-11-06 17:02:39.330', N'THE-GV01', '2024-01-15', '2028-12-31', N'Hoạt động'),
(51, N'GV02', N'Trần Thị Mai', '1976-08-25', N'Nữ', N'Phòng 302 Nhà A3, ĐH Giao thông Vận tải, Hà Nội', N'0912111002', N'mai.tt@utc.edu.vn', N'KHOA.CNTT', N'Hoạt động', '2026-11-06 17:02:39.333', N'THE-GV02', '2024-01-15', '2028-12-31', N'Hoạt động'),
(52, N'GV03', N'Lê Hoàng Nam', '1988-11-05', N'Nam', N'Số 48 Nguyễn Chí Thanh, Láng Thượng, Đống Đa, Hà Nội', N'0912111003', N'nam.lh@utc.edu.vn', N'KHOA.CNTT', N'Hoạt động', '2026-11-06 17:02:39.333', N'THE-GV03', '2024-01-15', '2028-12-31', N'Hoạt động'),
(53, N'GV04', N'Phạm Quốc Hùng', '1982-01-19', N'Nam', N'Số 88 Trần Duy Hưng, Trung Hòa, Cầu Giấy, Hà Nội', N'0912111004', N'hung.pq@utc.edu.vn', N'KHOA.CNTT', N'Hoạt động', '2026-11-06 17:02:39.333', N'THE-GV04', '2024-01-15', '2028-12-31', N'Hoạt động'),
(54, N'GV05', N'Vũ Minh Trang', '1990-09-14', N'Nữ', N'Số 102 Chùa Láng, Láng Thượng, Đống Đa, Hà Nội', N'0912111005', N'trang.vm@utc.edu.vn', N'KHOA.CNTT', N'Hoạt động', '2026-11-06 17:02:39.336', N'THE-GV05', '2024-01-15', '2028-12-31', N'Hoạt động'),
(55, N'GV06', N'Hoàng Đình Dũng', '1975-03-30', N'Nam', N'Số 26 Huỳnh Thúc Kháng, Láng Hạ, Đống Đa, Hà Nội', N'0912111006', N'dung.hd@utc.edu.vn', N'KHOA.VTKT', N'Hoạt động', '2026-11-06 17:02:39.336', N'THE-GV06', '2024-01-15', '2028-12-31', N'Hoạt động'),
(56, N'GV07', N'Đặng Thu Hương', '1983-06-22', N'Nữ', N'Số 71 Kim Mã, Giảng Võ, Ba Đình, Hà Nội', N'0912111007', N'huong.dt@utc.edu.vn', N'KHOA.VTKT', N'Hoạt động', '2026-11-06 17:02:39.336', N'THE-GV07', '2024-01-15', '2028-12-31', N'Hoạt động'),
(57, N'GV08', N'Ngô Văn Thắng', '1987-12-08', N'Nam', N'Số 35 Đội Cấn, Ba Đình, Hà Nội', N'0912111008', N'thang.nv@utc.edu.vn', N'KHOA.VTKT', N'Hoạt động', '2026-11-06 17:02:39.340', N'THE-GV08', '2024-01-15', '2028-12-31', N'Hoạt động'),
(58, N'GV09', N'Bùi Đức Thành', '1974-10-18', N'Nam', N'Số 120 La Thành, Ô Chợ Dừa, Đống Đa, Hà Nội', N'0912111009', N'thanh.bd@utc.edu.vn', N'KHOA.DDT', N'Hoạt động', '2026-11-06 17:02:39.340', N'THE-GV09', '2024-01-15', '2028-12-31', N'Hoạt động'),
(59, N'GV10', N'Phan Ngọc Ánh', '1984-05-03', N'Nữ', N'Số 54 Thái Hà, Trung Liệt, Đống Đa, Hà Nội', N'0912111010', N'anh.pn@utc.edu.vn', N'KHOA.DDT', N'Hoạt động', '2026-11-06 17:02:39.340', N'THE-GV10', '2024-01-15', '2028-12-31', N'Hoạt động'),
(60, N'GV11', N'Đỗ Quang Huy', '1989-07-27', N'Nam', N'Số 63 Tây Sơn, Quang Trung, Đống Đa, Hà Nội', N'0912111011', N'huy.dq@utc.edu.vn', N'KHOA.DDT', N'Hoạt động', '2026-11-06 17:02:39.340', N'THE-GV11', '2024-01-15', '2028-12-31', N'Hoạt động'),
(61, N'GV12', N'Dương Thanh Tùng', '1978-02-11', N'Nam', N'Số 9 Hoàng Cầu, Ô Chợ Dừa, Đống Đa, Hà Nội', N'0912111012', N'tung.dt@utc.edu.vn', N'KHOA.CK', N'Hoạt động', '2026-11-06 17:02:39.343', N'THE-GV12', '2024-01-15', '2028-12-31', N'Hoạt động'),
(62, N'GV13', N'Nguyễn Bích Ngọc', '1985-11-20', N'Nữ', N'Số 210 Xã Đàn, Nam Đồng, Đống Đa, Hà Nội', N'0912111013', N'ngoc.nb@utc.edu.vn', N'KHOA.CK', N'Hoạt động', '2026-11-06 17:02:39.343', N'THE-GV13', '2024-01-15', '2028-12-31', N'Hoạt động'),
(63, N'GV14', N'Trịnh Xuân Bách', '1991-04-09', N'Nam', N'Số 18 Phạm Ngọc Thạch, Kim Liên, Đống Đa, Hà Nội', N'0912111014', N'bach.tx@utc.edu.vn', N'KHOA.CK', N'Hoạt động', '2026-11-06 17:02:39.346', N'THE-GV14', '2024-01-15', '2028-12-31', N'Hoạt động'),
(64, N'GV15', N'Chu Mạnh Cường', '1977-12-03', N'Nam', N'Số 42 Láng Hạ, Đống Đa, Hà Nội', N'0912111015', N'cuong.cm@utc.edu.vn', N'KHOA.CT', N'Hoạt động', '2026-11-06 17:02:39.346', N'THE-GV15', '2024-01-15', '2028-12-31', N'Hoạt động'),
(65, N'GV16', N'Tạ Thị Kim Oanh', '1981-08-15', N'Nữ', N'Số 150 Chùa Bộc, Quang Trung, Đống Đa, Hà Nội', N'0912111016', N'oanh.ttk@utc.edu.vn', N'KHOA.CT', N'Hoạt động', '2026-11-06 17:02:39.350', N'THE-GV16', '2024-01-15', '2028-12-31', N'Hoạt động'),
(66, N'GV17', N'Lưu Văn Hưng', '1986-06-01', N'Nam', N'Số 77 Đặng Văn Ngữ, Trung Tự, Đống Đa, Hà Nội', N'0912111017', N'hung.lv@utc.edu.vn', N'KHOA.CT', N'Hoạt động', '2026-11-06 17:02:39.350', N'THE-GV17', '2024-01-15', '2028-12-31', N'Hoạt động'),
(67, N'GV18', N'Mai Tiến Đạt', '1983-03-29', N'Nam', N'Số 8 Chùa Láng, Láng Thượng, Đống Đa, Hà Nội', N'0912111018', N'dat.mt@utc.edu.vn', N'KHOA.KHCB', N'Hoạt động', '2026-11-06 17:02:39.350', N'THE-GV18', '2024-01-15', '2028-12-31', N'Hoạt động'),
(69, N'GV20', N'Lâm Hoài Nam', '1973-05-16', N'Nam', N'Số 136 Xuân Thủy, Dịch Vọng Hậu, Cầu Giấy, Hà Nội', N'0912111020', N'nam.lh2@utc.edu.vn', N'KHOA.KHCB', N'Hoạt động', '2026-11-06 17:02:39.356', N'THE-GV20', '2024-01-15', '2028-12-31', N'Hoạt động');
SET IDENTITY_INSERT dbo.[BanDoc] OFF;
GO

-- Dữ liệu bảng [TaiKhoan] (73 bản ghi)
SET IDENTITY_INSERT dbo.[TaiKhoan] ON;
INSERT INTO dbo.[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [EmailDangNhap], [MatKhauHash], [MaBanDoc], [MaNhanVien], [TrangThai], [NgayTao], [LanDangNhapCuoi], [VaiTro]) VALUES
(1, N'admin', N'admin@utc.edu.vn', N'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', NULL, 1, N'Hoạt động', '2026-11-06 01:43:53.868', '2026-11-07 10:29:41.138', N'ADMIN'),
(2, N'242630899', N'242630899@utc.edu.vn', N'F5A7A5A53F54429EB3E2A689270AF812CE1905736B78DE405389413E03F2963C', 1, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', '2026-11-06 21:21:28.881', N'READER'),
(3, N'242624495', N'242624495@utc.edu.vn', N'EA126ECCCA35151173100CDFEEFFB379654D8B2B80E967F14D85A0D5DDFF6A03', 2, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(4, N'242630904', N'242630904@utc.edu.vn', N'90866B3FEB101F90ADCA220751D815D4A401A810D8A9AF4777955BE2FE85008A', 3, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(5, N'242630905', N'242630905@utc.edu.vn', N'6074C049514AC173240FF7EEA33F51AE4E3A2B14F433BFC13482DD834AC24551', 4, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(6, N'242630906', N'242630906@utc.edu.vn', N'E7E47BA51575D3B569DE8EF9C7871AC15E69DAF834FD301A53DAF993D99223DB', 5, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(7, N'242630909', N'242630909@utc.edu.vn', N'CE9AD92DF8D27C181356BEC30DFF01D98CC5EBAAB027B01CA1FCA587A401E37F', 6, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(8, N'242630910', N'242630910@utc.edu.vn', N'446560B4A25D9192E309DADF6AA21CEB88E79066CD79D8B319B8822AE3B86EB3', 7, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(9, N'242630911', N'242630911@utc.edu.vn', N'053FD4703010A654847D84C8E2AC5D8C626B219A54E864858F51D47BB91930E1', 8, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(10, N'242630913', N'242630913@utc.edu.vn', N'3331141EB2D2E4F978012277552226D91E26C364E13AC6446E1929511455C7D0', 9, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(11, N'242630916', N'242630916@utc.edu.vn', N'FF4842B8F1714355DCADF1A772215E6F4EC22714E5F02B19052F9B5233107281', 10, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(12, N'242630917', N'242630917@utc.edu.vn', N'9E42809DB95BB2C3EAFD294C4E28814213E9D8AD468523819D9499A0616F6FC0', 11, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(13, N'242603895', N'242603895@utc.edu.vn', N'C6AB3BFBDC407CE8C6F5F326224862EB60692F95D17F48C97383BC1E2800E385', 12, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(14, N'242630922', N'242630922@utc.edu.vn', N'DD0909C98D288DEB2ADA8B7C67C829E0D25AFAC3F1AABB5700299BD4A75714AE', 13, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(15, N'242630924', N'242630924@utc.edu.vn', N'FE281C00D8C46BB238A0049647D5D9ABA9194ADEFF0A56E12B3D969F08A39A37', 14, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(16, N'242630923', N'242630923@utc.edu.vn', N'3F79DF17B5F2B19028B6CF39034E3D5E18D6E9896D1883229F7DD6B5946A4F1E', 15, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', '2026-11-07 11:14:48.952', N'READER'),
(17, N'242630926', N'242630926@utc.edu.vn', N'4C399ED42A632501E8A11F08F2AC9B9AA7CDE03FE17E0A41DA6434977C172CB6', 16, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(18, N'242630928', N'242630928@utc.edu.vn', N'765E27D6AC72B0AF0163008FB83B50C8EE4B1156073FC87054F8B289E3001F1B', 17, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(19, N'242603896', N'242603896@utc.edu.vn', N'AAB741E90B3D260AD73B7AA36EAB047404771F8ECEBF72CB75711681AF1F4D6C', 18, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(20, N'242630932', N'242630932@utc.edu.vn', N'1E4017CE570360F475EF04286892AF5A3A4B808476C97B260D1966135AF8B25F', 19, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(21, N'242630933', N'242630933@utc.edu.vn', N'A3DC8D788D569AB85E6430BAD9C2317A8C751458C64EFAE7E42677C5E8743B94', 20, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(22, N'242630934', N'242630934@utc.edu.vn', N'EC4B1FA9D6D67A534255013F5DAF9E59AB429CEC2DADE9560B6BB62BE808B25C', 21, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(23, N'242624497', N'242624497@utc.edu.vn', N'76AD92CFD3F45DB927B8D93B4DD97A1732A64D42D1BF29602ADE01DABE9EF6C0', 22, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(24, N'242630936', N'242630936@utc.edu.vn', N'7426FAA1BC1C50D6101791BC6B6323FCC4DFD98B0733A365F9477FE96EA28E8E', 23, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(25, N'242630941', N'242630941@utc.edu.vn', N'FCFC43EDE43297B020BB0BA402C07B32432A43F0C72FD3605BC1E7532B2F9EA5', 24, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(26, N'242630942', N'242630942@utc.edu.vn', N'62D672B2AFA4173A894E3DFF5DD66310569896B3DDCAF2DA97201D8AF4F61AAB', 25, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(27, N'242630943', N'242630943@utc.edu.vn', N'14456773395B2FA95727C70236B330491025150C52A7DB4F090BBE4723D29DD9', 26, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(28, N'242630944', N'242630944@utc.edu.vn', N'B0BD79BFCD4547D26BF79551F63E703CC5E556C8544A8F3A3B2CAF2F84E17C29', 27, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(29, N'242630946', N'242630946@utc.edu.vn', N'5D3868EC5F3807EC4417D877D0EE5DF7705EB20D106D5CFEC3FA2FC25F003DA2', 28, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(30, N'242630954', N'242630954@utc.edu.vn', N'A760C354771BBEAA2B471587EC00FA0311F8912951B90F7EFF3D780EA2AFE949', 29, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(31, N'242630955', N'242630955@utc.edu.vn', N'57043715894D19DBE99BDABA115AA25F398CCD16C95B682B504C1EC80EDB0F75', 30, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(32, N'242630956', N'242630956@utc.edu.vn', N'E3BBAF7F639D3488FCE895BDAEC99160CCB76093556DEF6A5BBEACF3D8A7C6C7', 31, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(33, N'242630957', N'242630957@utc.edu.vn', N'FBCF5C85E9F62E385FB01690831F04ABC8323160C2F12C5A43E85DBFFB715762', 32, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(34, N'242630958', N'242630958@utc.edu.vn', N'C4EAC36BE9DED5098FB26C12AF25013B12F77B458E8A73F83B84F09EC5D00347', 33, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(35, N'242630964', N'242630964@utc.edu.vn', N'E0B15F02F8A733B20788FC0CB5DB242041D87F8A371C50E287A2D4AE42CD70EB', 34, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(36, N'242624500', N'242624500@utc.edu.vn', N'C86EB6680E93A7E4FD8DB519FE90E01B6EA85A92DB6B50D92EC4C52C07EB3787', 35, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(37, N'242630967', N'242630967@utc.edu.vn', N'CAC3C97F30ED333CEE96D55E0A9E834A720505FF300C0E6AF353825EB39D5006', 36, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(38, N'242630969', N'242630969@utc.edu.vn', N'6C13444419291656E6E3B346B3A6C50F41507DE713D399A85A90822B500BCF76', 37, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(39, N'242630970', N'242630970@utc.edu.vn', N'DE5D39115DFFCFC13484B09A442EEB8305375C2366AE839C117976045984539E', 38, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(40, N'242630971', N'242630971@utc.edu.vn', N'F9351B2733AC02298D2B606707064625CAA4A9D7648949B4666CB48CC1EDA508', 39, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER');
INSERT INTO dbo.[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [EmailDangNhap], [MatKhauHash], [MaBanDoc], [MaNhanVien], [TrangThai], [NgayTao], [LanDangNhapCuoi], [VaiTro]) VALUES
(41, N'242624501', N'242624501@utc.edu.vn', N'610DFE7DF14173AD3EC4F821E2C9D6895E3C79B0C21A99E27812C184934600EE', 40, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(42, N'242603897', N'242603897@utc.edu.vn', N'442EFB7C388312CC00F335B5B24304C3F66F337390BDC410B36407BBE3E881EF', 41, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(43, N'242630976', N'242630976@utc.edu.vn', N'9289208C699F260E24914F57AC66BC7D27F0E7F57305DAA2E811D3799466A51C', 42, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(44, N'242630978', N'242630978@utc.edu.vn', N'A398939B4AB241D33C2354020AD5FB8E567A8316E9D526BC8BA14679F87E69D1', 43, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(45, N'242630979', N'242630979@utc.edu.vn', N'E3926420BBAE5A93AE59F20DB879BF44523B58C3A41961CEC7A3E479F354BE30', 44, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(46, N'242630983', N'242630983@utc.edu.vn', N'58E65B7A5800CD641BA8EC80826A251A98B8B3165D66233B27F129081A7DE277', 45, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(47, N'242630984', N'242630984@utc.edu.vn', N'1E44A7C4A55B3EE77C75237D458252EFCB6B5D0906F999D8E390A4B8E58DF270', 46, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(48, N'242630985', N'242630985@utc.edu.vn', N'B7C8EA1F91CF9735CDD44200BFD8229467AED7DA44AFF72F96A3FBEB5E707762', 47, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(49, N'242630988', N'242630988@utc.edu.vn', N'23A5FA8555EB5E72128A29E9F4EA561AB30B4D2FC72A19665A5BADD9AD03CDA8', 48, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(50, N'242630991', N'242630991@utc.edu.vn', N'D7A77071E087317B6B21A7D014DC89EFD366E0999C23675997F4B91E1BA79C66', 49, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
(51, N'NV26-01', N'lan.demo@utc.edu.vn', N'34D0A98D93BC56782E18FEE57B31E0D04F31B5952595E71BFB6677A833AF8339', NULL, 3, N'Hoạt động', '2026-11-06 15:40:13.372', NULL, N'ADMIN'),
(52, N'NV26-02', N'bandoc.demo@utc.edu.vn', N'22F7BF1DF664FEDA8BD8306969A06B2A2F13FCB83F06BA1FA060D66F6FB999A7', NULL, 4, N'Hoạt động', '2026-11-06 15:40:13.392', '2026-11-06 21:22:34.680', N'QUANLY_BANDOC'),
(53, N'NV26-03', N'thuthu.demo@utc.edu.vn', N'2273A4C844A2F9CF4D8950FE46728688F175CDB54524FA0E48DBC7D045DE9E67', NULL, 5, N'Hoạt động', '2026-11-06 15:40:13.407', '2026-11-06 21:23:27.861', N'QUANLY_TAILIEU'),
(54, N'GV01', N'tuan.nv@utc.edu.vn', N'0A187109F3D1B0907E92E249CFE90F99AEE7A4E247982008C0862F9D5E52EC32', 50, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.333', '2026-11-06 21:18:58.477', N'Reader'),
(55, N'GV02', N'mai.tt@utc.edu.vn', N'FAFB959218CB47525AF8E0FF7D0260275472F1F4FF5B1085667452598C37300B', 51, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.333', '2026-11-07 19:26:47.539', N'Reader'),
(56, N'GV03', N'nam.lh@utc.edu.vn', N'B5A3E46F6AA4C4A7959FCF4F0670ECAD5D8A9B4203C66510D1C25ED0A791F21A', 52, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.333', NULL, N'Reader'),
(57, N'GV04', N'hung.pq@utc.edu.vn', N'E3B2D64C0023440BB9747E2C38858A1CBBD6F274096FDC20E8C130669DAF97A5', 53, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.336', NULL, N'Reader'),
(58, N'GV05', N'trang.vm@utc.edu.vn', N'26826049F2455EE8880B3FAA6CA8796646FA60CC4299905DB63498E8C0B37B24', 54, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.336', NULL, N'Reader'),
(59, N'GV06', N'dung.hd@utc.edu.vn', N'08B71F97F218A26F57C989CAE6C6A4376813D92769380CB240E08E5C91ED693B', 55, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.336', NULL, N'Reader'),
(60, N'GV07', N'huong.dt@utc.edu.vn', N'9E0513AAB0FB92370993843FC3BD710BB638C15168D213C29847ED970AB79F7D', 56, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.336', NULL, N'Reader'),
(61, N'GV08', N'thang.nv@utc.edu.vn', N'CDC5831B82C3F8A03F24B8DF15ECF952E429C152F7E40CBF54114D84929E4A25', 57, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.340', NULL, N'Reader'),
(62, N'GV09', N'thanh.bd@utc.edu.vn', N'1784C28FF194B08C284199B44A2D925208ADF022F090490BFFC5A7F3627B1621', 58, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.340', NULL, N'Reader'),
(63, N'GV10', N'anh.pn@utc.edu.vn', N'C83DC4613BEF5E1839F60EFFD4B2D6567C44872A7B445EDD86DB5CA2F76A72CD', 59, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.340', NULL, N'Reader'),
(64, N'GV11', N'huy.dq@utc.edu.vn', N'3EC784688EDE7514E8DCA059D8B0A7A40FB18B40C4115819E54380A65DB1B17C', 60, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.340', NULL, N'Reader'),
(65, N'GV12', N'tung.dt@utc.edu.vn', N'0870A4B28596383D05E4ADA58EEB01107E61055B302368C18F086BEBBC6787A7', 61, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.343', NULL, N'Reader'),
(66, N'GV13', N'ngoc.nb@utc.edu.vn', N'7A7D4F6E4A59A278EB1D706C5651C90D8A4EFE165835C7FED32110BEC9C84C65', 62, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.343', NULL, N'Reader'),
(67, N'GV14', N'bach.tx@utc.edu.vn', N'B128D8C9F4DD66EEB47E7BE83B018B56FBFD5937F311ACBC361637D3EE3779A8', 63, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.346', NULL, N'Reader'),
(68, N'GV15', N'cuong.cm@utc.edu.vn', N'8CD8B2E16F31316CFD2454EB038F701D0E65C3681984B604DA239DE940FF8F57', 64, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.346', NULL, N'Reader'),
(69, N'GV16', N'oanh.ttk@utc.edu.vn', N'580DEACF7574AD17F33FD49FA5FF5DF664F9C207A501AC690EF2978A09FF7B5D', 65, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.350', NULL, N'Reader'),
(70, N'GV17', N'hung.lv@utc.edu.vn', N'BD83D83DB93BC66EA59193CF766270F38BED56043757EF40594435D9B30D772E', 66, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.350', NULL, N'Reader'),
(71, N'GV18', N'dat.mt@utc.edu.vn', N'4764ED2DA88ECE746429BE1128A0C49D0CB31E8ACB0480094A3CE47F21A08258', 67, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.353', NULL, N'Reader'),
(72, N'GV19', N'nhung.dh@utc.edu.vn', N'27E5DAA7EE1F35C7415942BAF70E706A2AAF4C0FCD6C958D14971F9B1BD15514', 68, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.353', NULL, N'Reader'),
(73, N'GV20', N'nam.lh2@utc.edu.vn', N'37FAB215C6C142668DD2DB96C73D712AE944F3D2664DBA3D470DB0C1015D6AD9', 69, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.356', NULL, N'Reader');
SET IDENTITY_INSERT dbo.[TaiKhoan] OFF;
GO

-- Bảng [TaiLieu]: Hiện chưa có dữ liệu


-- Dữ liệu bảng [TaiLieu_TacGia] (47 bản ghi)
INSERT INTO dbo.[TaiLieu_TacGia] ([MaTaiLieu], [MaTacGia], [VaiTroTacGia]) VALUES
(1, 1, N'Chủ biên'),
(2, 2, N'Chủ biên'),
(2, 3, N'Đồng tác giả'),
(2, 4, N'Đồng tác giả'),
(3, 5, N'Chủ biên'),
(3, 6, N'Đồng tác giả'),
(4, 7, N'Chủ biên'),
(5, 8, N'Chủ biên'),
(5, 9, N'Đồng tác giả'),
(6, 8, N'Chủ biên'),
(6, 10, N'Đồng tác giả'),
(7, 11, N'Chủ biên'),
(8, 12, N'Chủ biên'),
(9, 13, N'Chủ biên'),
(10, 14, N'Chủ biên'),
(11, 15, N'Chủ biên'),
(11, 16, N'Đồng tác giả'),
(11, 17, N'Đồng tác giả'),
(12, 15, N'Chủ biên'),
(12, 16, N'Đồng tác giả'),
(12, 17, N'Đồng tác giả'),
(13, 15, N'Chủ biên'),
(13, 16, N'Đồng tác giả'),
(13, 17, N'Đồng tác giả'),
(14, 18, N'Chủ biên'),
(15, 19, N'Chủ biên'),
(16, 19, N'Chủ biên'),
(17, 20, N'Chủ biên'),
(17, 21, N'Đồng tác giả'),
(18, 22, N'Chủ biên'),
(19, 23, N'Chủ biên'),
(19, 24, N'Đồng tác giả'),
(20, 25, N'Chủ biên'),
(20, 26, N'Đồng tác giả'),
(21, 27, N'Chủ biên'),
(22, 28, N'Chủ biên'),
(22, 29, N'Đồng tác giả'),
(23, 30, N'Chủ biên'),
(24, 31, N'Chủ biên'),
(24, 32, N'Dịch giả');
INSERT INTO dbo.[TaiLieu_TacGia] ([MaTaiLieu], [MaTacGia], [VaiTroTacGia]) VALUES
(25, 33, N'Chủ biên'),
(25, 34, N'Đồng tác giả'),
(26, 35, N'Chủ biên'),
(27, 36, N'Chủ biên'),
(28, 37, N'Chủ biên'),
(29, 37, N'Chủ biên'),
(30, 37, N'Chủ biên');
GO

-- Dữ liệu bảng [BanSao] (120 bản ghi)
SET IDENTITY_INSERT dbo.[BanSao] ON;
INSERT INTO dbo.[BanSao] ([MaBanSao], [MaTaiLieu], [MaVach], [ViTriKe], [TinhTrang], [TrangThai], [NgayNhap]) VALUES
(1, 1, N'UTC-01-01', N'Kệ IT-01', N'Mới', N'Đang mượn', '2024-01-15'),
(2, 1, N'UTC-01-02', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-01-15'),
(3, 1, N'UTC-01-03', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-02-20'),
(4, 1, N'UTC-01-04', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-02-20'),
(5, 2, N'UTC-02-01', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
(6, 2, N'UTC-02-02', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-01-15'),
(7, 2, N'UTC-02-03', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-02-20'),
(8, 2, N'UTC-02-04', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-02-20'),
(9, 3, N'UTC-03-01', N'Kệ IT-02', N'Mới', N'Đang mượn', '2024-01-15'),
(10, 3, N'UTC-03-02', N'Kệ IT-02', N'Tốt', N'Đang mượn', '2024-01-15'),
(11, 3, N'UTC-03-03', N'Kệ IT-02', N'Tốt', N'Đang mượn', '2024-02-20'),
(12, 3, N'UTC-03-04', N'Kệ IT-02', N'Tốt', N'Đang mượn', '2024-02-20'),
(13, 4, N'UTC-04-01', N'Kệ IT-02', N'Tốt', N'Đang mượn', '2024-01-15'),
(14, 4, N'UTC-04-02', N'Kệ IT-02', N'Tốt', N'Đang mượn', '2024-01-15'),
(15, 4, N'UTC-04-03', N'Kệ IT-02', N'Tốt', N'Đang mượn', '2024-02-20'),
(16, 4, N'UTC-04-04', N'Kệ IT-02', N'Tốt', N'Đang mượn', '2024-02-20'),
(17, 5, N'UTC-05-01', N'Kệ IT-03', N'Mới', N'Đang mượn', '2024-01-15'),
(18, 5, N'UTC-05-02', N'Kệ IT-03', N'Tốt', N'Đang mượn', '2024-01-15'),
(19, 5, N'UTC-05-03', N'Kệ IT-03', N'Tốt', N'Đang mượn', '2024-02-20'),
(20, 5, N'UTC-05-04', N'Kệ IT-03', N'Tốt', N'Đang mượn', '2024-02-20'),
(21, 6, N'UTC-06-01', N'Kệ IT-03', N'Tốt', N'Đang mượn', '2024-01-15'),
(22, 6, N'UTC-06-02', N'Kệ IT-03', N'Tốt', N'Đang mượn', '2024-01-15'),
(23, 6, N'UTC-06-03', N'Kệ IT-03', N'Tốt', N'Đang mượn', '2024-02-20'),
(24, 6, N'UTC-06-04', N'Kệ IT-03', N'Tốt', N'Đang mượn', '2024-02-20'),
(25, 7, N'UTC-07-01', N'Kệ IT-04', N'Mới', N'Đang mượn', '2024-01-15'),
(26, 7, N'UTC-07-02', N'Kệ IT-04', N'Tốt', N'Đang mượn', '2024-01-15'),
(27, 7, N'UTC-07-03', N'Kệ IT-04', N'Tốt', N'Đang mượn', '2024-02-20'),
(28, 7, N'UTC-07-04', N'Kệ IT-04', N'Tốt', N'Đang mượn', '2024-02-20'),
(29, 8, N'UTC-08-01', N'Kệ IT-04', N'Mới', N'Đang mượn', '2024-01-15'),
(30, 8, N'UTC-08-02', N'Kệ IT-04', N'Tốt', N'Đang mượn', '2024-01-15'),
(31, 8, N'UTC-08-03', N'Kệ IT-04', N'Tốt', N'Đang mượn', '2024-02-20'),
(32, 8, N'UTC-08-04', N'Kệ IT-04', N'Tốt', N'Đang mượn', '2024-02-20'),
(33, 9, N'UTC-09-01', N'Kệ IT-05', N'Mới', N'Đang mượn', '2024-01-15'),
(34, 9, N'UTC-09-02', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-01-15'),
(35, 9, N'UTC-09-03', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-02-20'),
(36, 9, N'UTC-09-04', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-02-20'),
(37, 10, N'UTC-10-01', N'Kệ IT-05', N'Mới', N'Đang mượn', '2024-01-15'),
(38, 10, N'UTC-10-02', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-01-15'),
(39, 10, N'UTC-10-03', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-02-20'),
(40, 10, N'UTC-10-04', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-02-20');
INSERT INTO dbo.[BanSao] ([MaBanSao], [MaTaiLieu], [MaVach], [ViTriKe], [TinhTrang], [TrangThai], [NgayNhap]) VALUES
(41, 11, N'UTC-11-01', N'Kệ CB-01', N'Mới', N'Đang mượn', '2024-01-15'),
(42, 11, N'UTC-11-02', N'Kệ CB-01', N'Tốt', N'Đang mượn', '2024-01-15'),
(43, 11, N'UTC-11-03', N'Kệ CB-01', N'Tốt', N'Đang mượn', '2024-02-20'),
(44, 11, N'UTC-11-04', N'Kệ CB-01', N'Tốt', N'Đang mượn', '2024-02-20'),
(45, 12, N'UTC-12-01', N'Kệ CB-01', N'Mới', N'Đang mượn', '2024-01-15'),
(46, 12, N'UTC-12-02', N'Kệ CB-01', N'Tốt', N'Đang mượn', '2024-01-15'),
(47, 12, N'UTC-12-03', N'Kệ CB-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(48, 12, N'UTC-12-04', N'Kệ CB-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(49, 13, N'UTC-13-01', N'Kệ CB-02', N'Mới', N'Có sẵn', '2024-01-15'),
(50, 13, N'UTC-13-02', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-01-15'),
(51, 13, N'UTC-13-03', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(52, 13, N'UTC-13-04', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(53, 14, N'UTC-14-01', N'Kệ CB-02', N'Mới', N'Có sẵn', '2024-01-15'),
(54, 14, N'UTC-14-02', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-01-15'),
(55, 14, N'UTC-14-03', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(56, 14, N'UTC-14-04', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(57, 15, N'UTC-15-01', N'Kệ CB-03', N'Mới', N'Có sẵn', '2024-01-15'),
(58, 15, N'UTC-15-02', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-01-15'),
(59, 15, N'UTC-15-03', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-02-20'),
(60, 15, N'UTC-15-04', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-02-20'),
(61, 16, N'UTC-16-01', N'Kệ CB-03', N'Mới', N'Có sẵn', '2024-01-15'),
(62, 16, N'UTC-16-02', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-01-15'),
(63, 16, N'UTC-16-03', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-02-20'),
(64, 16, N'UTC-16-04', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-02-20'),
(65, 17, N'UTC-17-01', N'Kệ GT-01', N'Mới', N'Có sẵn', '2024-01-15'),
(66, 17, N'UTC-17-02', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
(67, 17, N'UTC-17-03', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(68, 17, N'UTC-17-04', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(69, 18, N'UTC-18-01', N'Kệ GT-01', N'Mới', N'Có sẵn', '2024-01-15'),
(70, 18, N'UTC-18-02', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
(71, 18, N'UTC-18-03', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(72, 18, N'UTC-18-04', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(73, 19, N'UTC-19-01', N'Kệ GT-02', N'Mới', N'Có sẵn', '2024-01-15'),
(74, 19, N'UTC-19-02', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
(75, 19, N'UTC-19-03', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(76, 19, N'UTC-19-04', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(77, 20, N'UTC-20-01', N'Kệ GT-02', N'Mới', N'Có sẵn', '2024-01-15'),
(78, 20, N'UTC-20-02', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
(79, 20, N'UTC-20-03', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(80, 20, N'UTC-20-04', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20');
INSERT INTO dbo.[BanSao] ([MaBanSao], [MaTaiLieu], [MaVach], [ViTriKe], [TinhTrang], [TrangThai], [NgayNhap]) VALUES
(81, 21, N'UTC-21-01', N'Kệ GT-03', N'Mới', N'Có sẵn', '2024-01-15'),
(82, 21, N'UTC-21-02', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-01-15'),
(83, 21, N'UTC-21-03', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-02-20'),
(84, 21, N'UTC-21-04', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-02-20'),
(85, 22, N'UTC-22-01', N'Kệ GT-03', N'Mới', N'Có sẵn', '2024-01-15'),
(86, 22, N'UTC-22-02', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-01-15'),
(87, 22, N'UTC-22-03', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-02-20'),
(88, 22, N'UTC-22-04', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-02-20'),
(89, 23, N'UTC-23-01', N'Kệ GT-04', N'Mới', N'Có sẵn', '2024-01-15'),
(90, 23, N'UTC-23-02', N'Kệ GT-04', N'Tốt', N'Có sẵn', '2024-01-15'),
(91, 23, N'UTC-23-03', N'Kệ GT-04', N'Tốt', N'Có sẵn', '2024-02-20'),
(92, 23, N'UTC-23-04', N'Kệ GT-04', N'Tốt', N'Có sẵn', '2024-02-20'),
(93, 24, N'UTC-24-01', N'Kệ KT-01', N'Mới', N'Có sẵn', '2024-01-15'),
(94, 24, N'UTC-24-02', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
(95, 24, N'UTC-24-03', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(96, 24, N'UTC-24-04', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(97, 25, N'UTC-25-01', N'Kệ KT-01', N'Mới', N'Có sẵn', '2024-01-15'),
(98, 25, N'UTC-25-02', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
(99, 25, N'UTC-25-03', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(100, 25, N'UTC-25-04', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(101, 26, N'UTC-26-01', N'Kệ KT-02', N'Mới', N'Có sẵn', '2024-01-15'),
(102, 26, N'UTC-26-02', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
(103, 26, N'UTC-26-03', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(104, 26, N'UTC-26-04', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(105, 27, N'UTC-27-01', N'Kệ KT-02', N'Mới', N'Có sẵn', '2024-01-15'),
(106, 27, N'UTC-27-02', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
(107, 27, N'UTC-27-03', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(108, 27, N'UTC-27-04', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(109, 28, N'UTC-28-01', N'Kệ LL-01', N'Mới', N'Có sẵn', '2024-01-15'),
(110, 28, N'UTC-28-02', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-01-15'),
(111, 28, N'UTC-28-03', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(112, 28, N'UTC-28-04', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(113, 29, N'UTC-29-01', N'Kệ LL-01', N'Mới', N'Có sẵn', '2024-01-15'),
(114, 29, N'UTC-29-02', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-01-15'),
(115, 29, N'UTC-29-03', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(116, 29, N'UTC-29-04', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-02-20'),
(117, 30, N'UTC-30-01', N'Kệ LL-02', N'Mới', N'Có sẵn', '2024-01-15'),
(118, 30, N'UTC-30-02', N'Kệ LL-02', N'Tốt', N'Có sẵn', '2024-01-15'),
(119, 30, N'UTC-30-03', N'Kệ LL-02', N'Tốt', N'Có sẵn', '2024-02-20'),
(120, 30, N'UTC-30-04', N'Kệ LL-02', N'Tốt', N'Có sẵn', '2024-02-20');
SET IDENTITY_INSERT dbo.[BanSao] OFF;
GO

-- Dữ liệu bảng [PhieuMuon] (9 bản ghi)
SET IDENTITY_INSERT dbo.[PhieuMuon] ON;
INSERT INTO dbo.[PhieuMuon] ([MaPhieuMuon], [MaNhanVien], [NgayMuon], [TrangThai], [GhiChu], [MaBanDoc]) VALUES
(1, 1, '2025-08-25 08:30:00.000', N'Đang mượn', N'Mượn học phần kỳ 1 năm học 2025-2026', 15),
(2, 1, '2026-03-15 10:00:00.000', N'Đang mượn', N'Mượn học phần kỳ 2 năm học 2026', 15),
(3, 1, '2025-09-10 14:00:00.000', N'Đang mượn', N'Mượn sách chuyên ngành', 1),
(4, 1, '2026-11-06 14:13:15.929', N'Đang mượn', N'Mượn tại quầy lưu thông thư viện', 2),
(5, 3, '2026-02-15 00:00:00.000', N'Đang mượn', N'Giảng viên mượn nghiên cứu & soạn bài giảng (Hạn 1 năm)', 50),
(6, 3, '2025-10-15 00:00:00.000', N'Đang mượn', N'Giảng viên mượn đề tài NCKH cấp Bộ', 51),
(7, 3, '2025-09-20 00:00:00.000', N'Đang mượn', N'Giảng viên mượn tối đa 10 cuốn (hạn mức cao nhất)', 52),
(8, 3, '2026-04-10 00:00:00.000', N'Đang mượn', N'Giảng viên mượn hướng dẫn luận văn thạc sĩ', 53),
(9, 3, '2025-10-01 00:00:00.000', N'Đang mượn', N'Giảng viên Khoa Vận tải - Kinh tế mượn tài liệu', 55);
SET IDENTITY_INSERT dbo.[PhieuMuon] OFF;
GO

-- Dữ liệu bảng [ChiTietMuon] (49 bản ghi)
SET IDENTITY_INSERT dbo.[ChiTietMuon] ON;
INSERT INTO dbo.[ChiTietMuon] ([MaChiTietMuon], [MaPhieuMuon], [MaBanSao], [HanTra], [NgayTra], [TinhTrangKhiTra], [TrangThai]) VALUES
(1, 2, 1, '2027-03-15', NULL, NULL, N'Đang mượn'),
(2, 2, 37, '2027-05-20', NULL, NULL, N'Đang mượn'),
(3, 1, 5, '2026-10-10', '2026-11-06 01:55:45.458', N'Bình thường', N'Đã trả'),
(4, 1, 21, '2026-08-25', '2026-11-06 15:03:40.653', N'Bình thường', N'Đã trả'),
(5, 1, 33, '2026-11-01', NULL, NULL, N'Quá hạn'),
(6, 3, 9, '2026-09-10', NULL, NULL, N'Quá hạn'),
(7, 3, 13, '2027-04-10', '2026-11-06 15:25:21.694', N'Bình thường', N'Đã trả'),
(8, 4, 22, '2027-11-06', NULL, NULL, N'Đang mượn'),
(9, 4, 14, '2027-11-06', NULL, NULL, N'Đang mượn'),
(10, 4, 17, '2027-11-06', NULL, NULL, N'Đang mượn'),
(11, 3, 5, '2027-11-06', '2026-11-06 21:17:59.131', N'Bình thường', N'Đã trả'),
(12, 3, 2, '2027-11-06', NULL, NULL, N'Đang mượn'),
(13, 3, 18, '2027-11-06', NULL, NULL, N'Đang mượn'),
(14, 5, 3, '2027-02-15', NULL, NULL, N'Đang mượn'),
(15, 5, 4, '2027-02-15', NULL, NULL, N'Đang mượn'),
(16, 5, 6, '2027-02-15', NULL, NULL, N'Đang mượn'),
(17, 5, 7, '2027-02-15', NULL, NULL, N'Đang mượn'),
(18, 5, 8, '2027-02-15', NULL, NULL, N'Đang mượn'),
(19, 5, 10, '2027-02-15', NULL, NULL, N'Đang mượn'),
(20, 6, 11, '2026-10-15', NULL, NULL, N'Quá hạn'),
(21, 6, 12, '2026-10-15', NULL, NULL, N'Quá hạn'),
(22, 6, 13, '2027-03-20', NULL, NULL, N'Đang mượn'),
(23, 6, 15, '2027-03-20', NULL, NULL, N'Đang mượn'),
(24, 6, 16, '2027-03-20', NULL, NULL, N'Đang mượn'),
(25, 6, 19, '2027-03-20', NULL, NULL, N'Đang mượn'),
(26, 6, 20, '2027-03-20', NULL, NULL, N'Đang mượn'),
(27, 6, 21, '2027-03-20', NULL, NULL, N'Đang mượn'),
(28, 6, 23, '2027-03-20', NULL, NULL, N'Đang mượn'),
(29, 7, 24, '2026-09-20', NULL, NULL, N'Quá hạn'),
(30, 7, 25, '2026-09-20', NULL, NULL, N'Quá hạn'),
(31, 7, 26, '2026-09-20', NULL, NULL, N'Quá hạn'),
(32, 7, 27, '2027-05-10', NULL, NULL, N'Đang mượn'),
(33, 7, 28, '2027-05-10', NULL, NULL, N'Đang mượn'),
(34, 7, 29, '2027-05-10', NULL, NULL, N'Đang mượn'),
(35, 7, 30, '2027-05-10', NULL, NULL, N'Đang mượn'),
(36, 7, 31, '2027-05-10', NULL, NULL, N'Đang mượn'),
(37, 7, 32, '2027-05-10', NULL, NULL, N'Đang mượn'),
(38, 7, 34, '2027-05-10', NULL, NULL, N'Đang mượn'),
(39, 8, 35, '2027-04-10', NULL, NULL, N'Đang mượn'),
(40, 8, 36, '2027-04-10', NULL, NULL, N'Đang mượn');
INSERT INTO dbo.[ChiTietMuon] ([MaChiTietMuon], [MaPhieuMuon], [MaBanSao], [HanTra], [NgayTra], [TinhTrangKhiTra], [TrangThai]) VALUES
(41, 8, 38, '2027-04-10', NULL, NULL, N'Đang mượn'),
(42, 8, 39, '2027-04-10', NULL, NULL, N'Đang mượn'),
(43, 9, 40, '2026-10-01', NULL, NULL, N'Quá hạn'),
(44, 9, 41, '2027-06-15', NULL, NULL, N'Đang mượn'),
(45, 9, 42, '2027-06-15', NULL, NULL, N'Đang mượn'),
(46, 9, 43, '2027-06-15', NULL, NULL, N'Đang mượn'),
(47, 9, 44, '2027-06-15', NULL, NULL, N'Đang mượn'),
(48, 9, 45, '2027-06-15', NULL, NULL, N'Đang mượn'),
(49, 9, 46, '2027-06-15', NULL, NULL, N'Đang mượn');
SET IDENTITY_INSERT dbo.[ChiTietMuon] OFF;
GO

-- Bảng [GiaHan]: Hiện chưa có dữ liệu


-- Bảng [DatTruoc]: Hiện chưa có dữ liệu


-- Dữ liệu bảng [ViPham] (4 bản ghi)
SET IDENTITY_INSERT dbo.[ViPham] ON;
INSERT INTO dbo.[ViPham] ([MaViPham], [MaBanDoc], [MaChiTietMuon], [LoaiViPham], [MoTa], [NgayViPham], [SoTienPhat], [TrangThaiNop], [NgayNop], [MaNhanVienThu]) VALUES
(1, 15, 3, N'Mượn quá hạn', N'Quá hạn trả sách Cơ sở dữ liệu quan hệ (thời hạn 1 năm)', '2026-10-11 00:00:00.000', 100000.00, N'Đã thanh toán', NULL, NULL),
(2, 15, 4, N'Mượn quá hạn', N'Quá hạn trả sách C++ và Lập trình hướng đối tượng (thời hạn 1 năm)', '2026-08-26 00:00:00.000', 100000.00, N'Đã thanh toán', NULL, NULL),
(3, 15, 5, N'Mượn quá hạn', N'Quá hạn trả sách Kỹ nghệ phần mềm (thời hạn 1 năm)', '2026-11-02 00:00:00.000', 100000.00, N'Chưa thanh toán', NULL, NULL),
(4, 1, 6, N'Mượn quá hạn', N'Quá hạn sách Mạng máy tính', '2026-09-11 00:00:00.000', 100000.00, N'Chưa thanh toán', NULL, NULL);
SET IDENTITY_INSERT dbo.[ViPham] OFF;
GO

-- Dữ liệu bảng [ThanhToan] (2 bản ghi)
SET IDENTITY_INSERT dbo.[ThanhToan] ON;
INSERT INTO dbo.[ThanhToan] ([MaThanhToan], [MaBanDoc], [MaNhanVien], [NgayThanhToan], [TongTien], [PhuongThuc], [TrangThai], [NoiDung]) VALUES
(1, 15, 1, '2026-11-06 01:55:45.556', 100000.00, N'Tiền mặt', N'Thành công', NULL),
(2, 15, 1, '2026-11-06 15:03:40.727', 100000.00, N'Tiền mặt', N'Thành công', NULL);
SET IDENTITY_INSERT dbo.[ThanhToan] OFF;
GO

PRINT N'=========================================================================================';
PRINT N'  KHỞI TẠO CƠ SỞ DỮ LIỆU QLThuVienUTC (16 BẢNG CHUẨN) THÀNH CÔNG RỰC RỠ!';
PRINT N'  Tài khoản quản trị: admin / 123456';
PRINT N'  Tài khoản thủ thư:  thuthu / 123456';
PRINT N'=========================================================================================';
GO
