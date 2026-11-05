/*
    CSDL QUAN LY THU VIEN - PHIEN BAN TIENG VIET
    Nen tang: Microsoft SQL Server
    Database: QLThuVienUTC

    Du lieu ban doc ban dau duoc nhap tu danh sach lop K65.CLCCNTT1.
    Luu y:
    - Khong tao san tai khoan/mat khau cho sinh vien.
    - Mat khau phai duoc hash bang ASP.NET Core PasswordHasher,
      khong luu mat khau thuong trong SQL.
    - Kiem tra lai danh sach MaSinhVien va HoTen voi ban goc truoc khi dua len production.
*/

IF DB_ID(N'QLThuVienUTC') IS NULL
BEGIN
    CREATE DATABASE QLThuVienUTC;
END
GO

USE QLThuVienUTC;
GO

/* XOA CAC BANG CU CUNG TEN DE CO THE CHAY LAI SCRIPT (CHI DUNG CHO MOI TRUONG DEMO) */
DROP TABLE IF EXISTS dbo.ChiTietThanhToan;
DROP TABLE IF EXISTS dbo.ThanhToan;
DROP TABLE IF EXISTS dbo.TienPhat;
DROP TABLE IF EXISTS dbo.ViPham;
DROP TABLE IF EXISTS dbo.LichSuDatTruoc;
DROP TABLE IF EXISTS dbo.DatTruoc;
DROP TABLE IF EXISTS dbo.GiaHan;
DROP TABLE IF EXISTS dbo.ChiTietMuon;
DROP TABLE IF EXISTS dbo.PhieuMuon;
DROP TABLE IF EXISTS dbo.BanSao;
DROP TABLE IF EXISTS dbo.TaiLieu_TacGia;
DROP TABLE IF EXISTS dbo.TaiLieu;
DROP TABLE IF EXISTS dbo.TacGia;
DROP TABLE IF EXISTS dbo.NhaXuatBan;
DROP TABLE IF EXISTS dbo.TheLoai;
DROP TABLE IF EXISTS dbo.NgonNgu;
DROP TABLE IF EXISTS dbo.QuocGia;
DROP TABLE IF EXISTS dbo.DinhDang;
DROP TABLE IF EXISTS dbo.TheBanDoc;
DROP TABLE IF EXISTS dbo.TaiKhoan_VaiTro;
DROP TABLE IF EXISTS dbo.VaiTro_Quyen;
DROP TABLE IF EXISTS dbo.Quyen;
DROP TABLE IF EXISTS dbo.TaiKhoan;
DROP TABLE IF EXISTS dbo.NhanVien;
DROP TABLE IF EXISTS dbo.BanDoc;
DROP TABLE IF EXISTS dbo.Lop;
DROP TABLE IF EXISTS dbo.VaiTro;
GO

/* =========================================================
   1. DANH MUC LOP, BAN DOC, NHAN VIEN
   ========================================================= */

CREATE TABLE dbo.Lop (
    MaLop       VARCHAR(20) NOT NULL PRIMARY KEY,
    TenLop      NVARCHAR(100) NOT NULL,
    KhoaHoc     NVARCHAR(30) NULL,
    GhiChu      NVARCHAR(255) NULL
);

CREATE TABLE dbo.BanDoc (
    MaBanDoc        INT IDENTITY(1,1) PRIMARY KEY,
    MaSinhVien      VARCHAR(15) NOT NULL UNIQUE,
    HoTen           NVARCHAR(100) NOT NULL,
    NgaySinh        DATE NULL,
    GioiTinh        NVARCHAR(10) NULL,
    DiaChi          NVARCHAR(255) NULL,
    SoDienThoai     VARCHAR(20) NULL,
    Email           VARCHAR(150) NULL,
    MaLop           VARCHAR(20) NULL,
    TrangThai       NVARCHAR(30) NOT NULL DEFAULT N'Hoạt động',
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_BanDoc_Lop FOREIGN KEY (MaLop) REFERENCES dbo.Lop(MaLop)
);

CREATE TABLE dbo.NhanVien (
    MaNhanVien     INT IDENTITY(1,1) PRIMARY KEY,
    MaSoNhanVien   VARCHAR(20) NULL UNIQUE,
    HoTen          NVARCHAR(100) NOT NULL,
    NgaySinh       DATE NULL,
    GioiTinh       NVARCHAR(10) NULL,
    SoDienThoai    VARCHAR(20) NULL,
    Email          VARCHAR(150) NULL,
    ChucVu         NVARCHAR(100) NULL,
    TrangThai      NVARCHAR(30) NOT NULL DEFAULT N'Đang làm việc'
);

/* =========================================================
   2. TAI KHOAN VA PHAN QUYEN
   Mot tai khoan thuoc mot BanDoc hoac mot NhanVien.
   Mot tai khoan co the duoc gan nhieu vai tro.
   ========================================================= */

CREATE TABLE dbo.VaiTro (
    MaVaiTro    VARCHAR(20) NOT NULL PRIMARY KEY,
    TenVaiTro   NVARCHAR(100) NOT NULL,
    MoTa        NVARCHAR(255) NULL
);

CREATE TABLE dbo.TaiKhoan (
    MaTaiKhoan      INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap     VARCHAR(100) NOT NULL UNIQUE,
    EmailDangNhap   VARCHAR(150) NULL UNIQUE,
    MatKhauHash     NVARCHAR(500) NOT NULL,
    MaBanDoc        INT NULL UNIQUE,
    MaNhanVien      INT NULL UNIQUE,
    TrangThai       NVARCHAR(30) NOT NULL DEFAULT N'Hoạt động',
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    LanDangNhapCuoi DATETIME2 NULL,
    CONSTRAINT FK_TaiKhoan_BanDoc FOREIGN KEY (MaBanDoc) REFERENCES dbo.BanDoc(MaBanDoc),
    CONSTRAINT FK_TaiKhoan_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES dbo.NhanVien(MaNhanVien),
    CONSTRAINT CK_TaiKhoan_DoiTuong CHECK (
        (MaBanDoc IS NOT NULL AND MaNhanVien IS NULL)
        OR
        (MaBanDoc IS NULL AND MaNhanVien IS NOT NULL)
    )
);

CREATE TABLE dbo.Quyen (
    MaQuyen     VARCHAR(50) NOT NULL PRIMARY KEY,
    TenQuyen    NVARCHAR(100) NOT NULL,
    MoTa        NVARCHAR(255) NULL
);

CREATE TABLE dbo.TaiKhoan_VaiTro (
    MaTaiKhoan INT NOT NULL,
    MaVaiTro   VARCHAR(20) NOT NULL,
    PRIMARY KEY (MaTaiKhoan, MaVaiTro),
    FOREIGN KEY (MaTaiKhoan) REFERENCES dbo.TaiKhoan(MaTaiKhoan),
    FOREIGN KEY (MaVaiTro) REFERENCES dbo.VaiTro(MaVaiTro)
);

CREATE TABLE dbo.VaiTro_Quyen (
    MaVaiTro VARCHAR(20) NOT NULL,
    MaQuyen  VARCHAR(50) NOT NULL,
    PRIMARY KEY (MaVaiTro, MaQuyen),
    FOREIGN KEY (MaVaiTro) REFERENCES dbo.VaiTro(MaVaiTro),
    FOREIGN KEY (MaQuyen) REFERENCES dbo.Quyen(MaQuyen)
);

/* =========================================================
   3. THE BAN DOC
   ========================================================= */

CREATE TABLE dbo.TheBanDoc (
    MaTheBanDoc     INT IDENTITY(1,1) PRIMARY KEY,
    MaBanDoc        INT NOT NULL UNIQUE,
    SoThe           VARCHAR(30) NOT NULL UNIQUE,
    NgayCap         DATE NOT NULL,
    NgayHetHan      DATE NULL,
    TrangThai       NVARCHAR(30) NOT NULL DEFAULT N'Hoạt động',
    FOREIGN KEY (MaBanDoc) REFERENCES dbo.BanDoc(MaBanDoc),
    CONSTRAINT CK_TheBanDoc_Ngay CHECK (NgayHetHan IS NULL OR NgayHetHan >= NgayCap)
);

/* =========================================================
   4. DANH MUC TAI LIEU
   TaiLieu = dau sach; BanSao = tung cuon sach vat ly.
   ========================================================= */

CREATE TABLE dbo.QuocGia (
    MaQuocGia CHAR(2) NOT NULL PRIMARY KEY,
    TenQuocGia NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.NgonNgu (
    MaNgonNgu VARCHAR(10) NOT NULL PRIMARY KEY,
    TenNgonNgu NVARCHAR(50) NOT NULL
);

CREATE TABLE dbo.TheLoai (
    MaTheLoai INT IDENTITY(1,1) PRIMARY KEY,
    TenTheLoai NVARCHAR(100) NOT NULL UNIQUE,
    MaTheLoaiCha INT NULL,
    FOREIGN KEY (MaTheLoaiCha) REFERENCES dbo.TheLoai(MaTheLoai)
);

CREATE TABLE dbo.DinhDang (
    MaDinhDang INT IDENTITY(1,1) PRIMARY KEY,
    TenDinhDang NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE dbo.NhaXuatBan (
    MaNhaXuatBan INT IDENTITY(1,1) PRIMARY KEY,
    TenNhaXuatBan NVARCHAR(150) NOT NULL,
    MaQuocGia CHAR(2) NULL,
    DiaChi NVARCHAR(255) NULL,
    SoDienThoai VARCHAR(20) NULL,
    FOREIGN KEY (MaQuocGia) REFERENCES dbo.QuocGia(MaQuocGia)
);

CREATE TABLE dbo.TacGia (
    MaTacGia INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(150) NOT NULL,
    MaQuocGia CHAR(2) NULL,
    NamSinh SMALLINT NULL,
    FOREIGN KEY (MaQuocGia) REFERENCES dbo.QuocGia(MaQuocGia)
);

CREATE TABLE dbo.TaiLieu (
    MaTaiLieu INT IDENTITY(1,1) PRIMARY KEY,
    ISBN VARCHAR(20) NULL,
    TenTaiLieu NVARCHAR(250) NOT NULL,
    MaNhaXuatBan INT NULL,
    MaNgonNgu VARCHAR(10) NULL,
    MaTheLoai INT NULL,
    MaDinhDang INT NULL,
    NamXuatBan SMALLINT NULL,
    LanXuatBan INT NULL,
    SoTrang INT NULL,
    MoTa NVARCHAR(MAX) NULL,
    AnhBia NVARCHAR(500) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (MaNhaXuatBan) REFERENCES dbo.NhaXuatBan(MaNhaXuatBan),
    FOREIGN KEY (MaNgonNgu) REFERENCES dbo.NgonNgu(MaNgonNgu),
    FOREIGN KEY (MaTheLoai) REFERENCES dbo.TheLoai(MaTheLoai),
    FOREIGN KEY (MaDinhDang) REFERENCES dbo.DinhDang(MaDinhDang),
    CONSTRAINT CK_TaiLieu_NamXuatBan CHECK (NamXuatBan IS NULL OR NamXuatBan BETWEEN 1000 AND 9999),
    CONSTRAINT CK_TaiLieu_SoTrang CHECK (SoTrang IS NULL OR SoTrang > 0)
);

CREATE TABLE dbo.TaiLieu_TacGia (
    MaTaiLieu INT NOT NULL,
    MaTacGia INT NOT NULL,
    VaiTroTacGia NVARCHAR(50) NULL,
    PRIMARY KEY (MaTaiLieu, MaTacGia),
    FOREIGN KEY (MaTaiLieu) REFERENCES dbo.TaiLieu(MaTaiLieu),
    FOREIGN KEY (MaTacGia) REFERENCES dbo.TacGia(MaTacGia)
);

CREATE TABLE dbo.BanSao (
    MaBanSao       INT IDENTITY(1,1) PRIMARY KEY,
    MaTaiLieu      INT NOT NULL,
    MaVach         VARCHAR(50) NOT NULL UNIQUE,
    ViTriKe        NVARCHAR(100) NULL,
    TinhTrang      NVARCHAR(30) NOT NULL DEFAULT N'Tốt',
    TrangThai      NVARCHAR(30) NOT NULL DEFAULT N'Có sẵn',
    NgayNhap       DATE NULL,
    FOREIGN KEY (MaTaiLieu) REFERENCES dbo.TaiLieu(MaTaiLieu)
);

/* =========================================================
   5. MUON - TRA - GIA HAN
   ========================================================= */

CREATE TABLE dbo.PhieuMuon (
    MaPhieuMuon    INT IDENTITY(1,1) PRIMARY KEY,
    MaTheBanDoc    INT NOT NULL,
    MaNhanVien     INT NOT NULL,
    NgayMuon       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TrangThai      NVARCHAR(30) NOT NULL DEFAULT N'Đang mượn',
    GhiChu         NVARCHAR(500) NULL,
    FOREIGN KEY (MaTheBanDoc) REFERENCES dbo.TheBanDoc(MaTheBanDoc),
    FOREIGN KEY (MaNhanVien) REFERENCES dbo.NhanVien(MaNhanVien)
);

CREATE TABLE dbo.ChiTietMuon (
    MaChiTietMuon  INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieuMuon    INT NOT NULL,
    MaBanSao       INT NOT NULL,
    HanTra          DATE NOT NULL,
    NgayTra         DATETIME2 NULL,
    TinhTrangKhiTra NVARCHAR(100) NULL,
    TrangThai       NVARCHAR(30) NOT NULL DEFAULT N'Đang mượn',
    FOREIGN KEY (MaPhieuMuon) REFERENCES dbo.PhieuMuon(MaPhieuMuon),
    FOREIGN KEY (MaBanSao) REFERENCES dbo.BanSao(MaBanSao)
);

CREATE TABLE dbo.GiaHan (
    MaGiaHan       INT IDENTITY(1,1) PRIMARY KEY,
    MaChiTietMuon  INT NOT NULL,
    HanTraCu       DATE NOT NULL,
    HanTraMoi      DATE NOT NULL,
    MaNhanVienDuyet INT NOT NULL,
    NgayGiaHan     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    LyDo           NVARCHAR(255) NULL,
    FOREIGN KEY (MaChiTietMuon) REFERENCES dbo.ChiTietMuon(MaChiTietMuon),
    FOREIGN KEY (MaNhanVienDuyet) REFERENCES dbo.NhanVien(MaNhanVien),
    CONSTRAINT CK_GiaHan_Ngay CHECK (HanTraMoi > HanTraCu)
);

/* =========================================================
   6. DAT TRUOC
   ========================================================= */

CREATE TABLE dbo.DatTruoc (
    MaDatTruoc    INT IDENTITY(1,1) PRIMARY KEY,
    MaBanDoc      INT NOT NULL,
    MaTaiLieu     INT NOT NULL,
    NgayDat       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    HanNhan       DATETIME2 NULL,
    TrangThai     NVARCHAR(30) NOT NULL DEFAULT N'Đang chờ',
    FOREIGN KEY (MaBanDoc) REFERENCES dbo.BanDoc(MaBanDoc),
    FOREIGN KEY (MaTaiLieu) REFERENCES dbo.TaiLieu(MaTaiLieu)
);

CREATE TABLE dbo.LichSuDatTruoc (
    MaLichSu      INT IDENTITY(1,1) PRIMARY KEY,
    MaDatTruoc    INT NOT NULL,
    TrangThaiCu   NVARCHAR(30) NULL,
    TrangThaiMoi  NVARCHAR(30) NOT NULL,
    ThoiGian      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    GhiChu        NVARCHAR(255) NULL,
    FOREIGN KEY (MaDatTruoc) REFERENCES dbo.DatTruoc(MaDatTruoc)
);

/* =========================================================
   7. VI PHAM - TIEN PHAT - THANH TOAN
   ========================================================= */

CREATE TABLE dbo.ViPham (
    MaViPham      INT IDENTITY(1,1) PRIMARY KEY,
    MaBanDoc      INT NOT NULL,
    MaChiTietMuon INT NULL,
    LoaiViPham    NVARCHAR(100) NOT NULL,
    MoTa          NVARCHAR(500) NULL,
    NgayViPham    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FOREIGN KEY (MaBanDoc) REFERENCES dbo.BanDoc(MaBanDoc),
    FOREIGN KEY (MaChiTietMuon) REFERENCES dbo.ChiTietMuon(MaChiTietMuon)
);

CREATE TABLE dbo.TienPhat (
    MaTienPhat    INT IDENTITY(1,1) PRIMARY KEY,
    MaViPham      INT NOT NULL UNIQUE,
    SoTien        DECIMAL(18,2) NOT NULL,
    TrangThai     NVARCHAR(30) NOT NULL DEFAULT N'Chưa thanh toán',
    FOREIGN KEY (MaViPham) REFERENCES dbo.ViPham(MaViPham),
    CONSTRAINT CK_TienPhat_SoTien CHECK (SoTien >= 0)
);

CREATE TABLE dbo.ThanhToan (
    MaThanhToan   INT IDENTITY(1,1) PRIMARY KEY,
    MaBanDoc      INT NOT NULL,
    MaNhanVien    INT NOT NULL,
    NgayThanhToan DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TongTien      DECIMAL(18,2) NOT NULL,
    PhuongThuc    NVARCHAR(50) NOT NULL,
    TrangThai     NVARCHAR(30) NOT NULL DEFAULT N'Đã thanh toán',
    FOREIGN KEY (MaBanDoc) REFERENCES dbo.BanDoc(MaBanDoc),
    FOREIGN KEY (MaNhanVien) REFERENCES dbo.NhanVien(MaNhanVien),
    CONSTRAINT CK_ThanhToan_TongTien CHECK (TongTien >= 0)
);

CREATE TABLE dbo.ChiTietThanhToan (
    MaChiTietThanhToan INT IDENTITY(1,1) PRIMARY KEY,
    MaThanhToan INT NOT NULL,
    MaTienPhat   INT NOT NULL,
    SoTien       DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (MaThanhToan) REFERENCES dbo.ThanhToan(MaThanhToan),
    FOREIGN KEY (MaTienPhat) REFERENCES dbo.TienPhat(MaTienPhat),
    CONSTRAINT CK_ChiTietThanhToan_SoTien CHECK (SoTien > 0)
);

/* =========================================================
   8. DU LIEU DANH MUC BAN DAU
   ========================================================= */

INSERT INTO dbo.VaiTro (MaVaiTro, TenVaiTro, MoTa) VALUES
('ADMIN', N'Quản trị viên', N'Quản lý toàn bộ hệ thống'),
('THUTHU', N'Thủ thư', N'Quản lý tài liệu và nghiệp vụ mượn trả'),
('BANDOC', N'Bạn đọc', N'Tra cứu, đặt trước và xem lịch sử cá nhân');

INSERT INTO dbo.Quyen (MaQuyen, TenQuyen, MoTa) VALUES
('TAI_LIEU_XEM', N'Xem tài liệu', N'Tra cứu danh mục tài liệu'),
('TAI_LIEU_QUAN_LY', N'Quản lý tài liệu', N'Thêm, sửa, ẩn tài liệu và bản sao'),
('BANDOC_QUAN_LY', N'Quản lý bạn đọc', N'Tạo và cập nhật hồ sơ bạn đọc'),
('MUON_TAO', N'Tạo phiếu mượn', N'Lập phiếu mượn tài liệu'),
('TRA_XAC_NHAN', N'Xác nhận trả', N'Xác nhận tình trạng trả tài liệu'),
('DAT_TRUOC_TAO', N'Đặt trước tài liệu', N'Bạn đọc tạo yêu cầu đặt trước'),
('BAO_CAO_XEM', N'Xem báo cáo', N'Xem thống kê thư viện'),
('TAIKHOAN_QUAN_LY', N'Quản lý tài khoản', N'Tạo, khóa và phân quyền tài khoản');

INSERT INTO dbo.VaiTro_Quyen (MaVaiTro, MaQuyen) VALUES
('ADMIN', 'TAI_LIEU_XEM'),
('ADMIN', 'TAI_LIEU_QUAN_LY'),
('ADMIN', 'BANDOC_QUAN_LY'),
('ADMIN', 'MUON_TAO'),
('ADMIN', 'TRA_XAC_NHAN'),
('ADMIN', 'DAT_TRUOC_TAO'),
('ADMIN', 'BAO_CAO_XEM'),
('ADMIN', 'TAIKHOAN_QUAN_LY'),
('THUTHU', 'TAI_LIEU_XEM'),
('THUTHU', 'TAI_LIEU_QUAN_LY'),
('THUTHU', 'BANDOC_QUAN_LY'),
('THUTHU', 'MUON_TAO'),
('THUTHU', 'TRA_XAC_NHAN'),
('THUTHU', 'BAO_CAO_XEM'),
('BANDOC', 'TAI_LIEU_XEM'),
('BANDOC', 'DAT_TRUOC_TAO');

INSERT INTO dbo.Lop (MaLop, TenLop, KhoaHoc, GhiChu)
VALUES ('K65.CLCCNTT1', N'K65 Chất lượng cao Công nghệ thông tin 1', N'K65', N'Lớp lấy từ danh sách thi người dùng cung cấp');

INSERT INTO dbo.BanDoc (MaSinhVien, HoTen, MaLop)
VALUES
('242630899', N'Nguyễn Văn An', 'K65.CLCCNTT1'),
('242624495', N'Nguyễn Hoàng Anh', 'K65.CLCCNTT1'),
('242630904', N'Phạm Tú Anh', 'K65.CLCCNTT1'),
('242630905', N'Vương Duy Anh', 'K65.CLCCNTT1'),
('242630906', N'Cao Đắc Bảo', 'K65.CLCCNTT1'),
('242630909', N'Trương Đức Bình', 'K65.CLCCNTT1'),
('242630910', N'Nguyễn Đức Cảnh', 'K65.CLCCNTT1'),
('242630911', N'Dương Linh Chi', 'K65.CLCCNTT1'),
('242630913', N'Nguyễn Duy Dũng', 'K65.CLCCNTT1'),
('242630916', N'Hà Thế Duy', 'K65.CLCCNTT1'),
('242630917', N'Nguyễn Thành Duy', 'K65.CLCCNTT1'),
('242603895', N'Bạch Thành Dương', 'K65.CLCCNTT1'),
('242630922', N'Nguyễn Viết Tùng Dương', 'K65.CLCCNTT1'),
('242630924', N'Đinh Quang Đạt', 'K65.CLCCNTT1'),
('242630923', N'Đỗ Văn Đạt', 'K65.CLCCNTT1'),
('242630926', N'Lê Tiến Đạt', 'K65.CLCCNTT1'),
('242630928', N'Như Mạnh Đạt', 'K65.CLCCNTT1'),
('242603896', N'Đinh Việt Đăng', 'K65.CLCCNTT1'),
('242630932', N'Bùi Văn Hữu Đức', 'K65.CLCCNTT1'),
('242630933', N'Đào Mạnh Đức', 'K65.CLCCNTT1'),
('242630934', N'Lê Minh Đức', 'K65.CLCCNTT1'),
('242624497', N'Nguyễn Minh Đức', 'K65.CLCCNTT1'),
('242630936', N'Phạm Minh Đức', 'K65.CLCCNTT1'),
('242630941', N'Nguyễn Thị Hoài', 'K65.CLCCNTT1'),
('242630942', N'Đặng Việt Hoàn', 'K65.CLCCNTT1'),
('242630943', N'Nguyễn Văn Hùng', 'K65.CLCCNTT1'),
('242630944', N'Trần Quang Huy', 'K65.CLCCNTT1'),
('242630946', N'Vũ Phùng Quang Huy', 'K65.CLCCNTT1'),
('242630954', N'Nguyễn Thu Kiều', 'K65.CLCCNTT1'),
('242630955', N'Nguyễn Thành Lâm', 'K65.CLCCNTT1'),
('242630956', N'Trương Ngọc Lâm', 'K65.CLCCNTT1'),
('242630957', N'Hà Doãn Linh', 'K65.CLCCNTT1'),
('242630958', N'Phạm Ngọc Linh', 'K65.CLCCNTT1'),
('242630964', N'Nguyễn Tiến Minh', 'K65.CLCCNTT1'),
('242624500', N'Phạm Tiến Minh', 'K65.CLCCNTT1'),
('242630967', N'Nguyễn Văn Phúc', 'K65.CLCCNTT1'),
('242630969', N'Vũ Trần Bảo Quân', 'K65.CLCCNTT1'),
('242630970', N'Phăng Phú Sáng', 'K65.CLCCNTT1'),
('242630971', N'Bùi Văn Sơn', 'K65.CLCCNTT1'),
('242624501', N'Trần Đức Tài', 'K65.CLCCNTT1'),
('242603897', N'Lê Đình Thành', 'K65.CLCCNTT1'),
('242630976', N'Nguyễn Quang Thắng', 'K65.CLCCNTT1'),
('242630978', N'Trần Minh Tiến', 'K65.CLCCNTT1'),
('242630979', N'Đỗ Kiếm Tiêu', 'K65.CLCCNTT1'),
('242630983', N'Đặng Thanh Tùng', 'K65.CLCCNTT1'),
('242630984', N'Hoàng Văn Tùng', 'K65.CLCCNTT1'),
('242630985', N'Nguyễn Thị Ánh Tuyết', 'K65.CLCCNTT1'),
('242630988', N'Lê Anh Việt', 'K65.CLCCNTT1'),
('242630991', N'Nguyễn Quốc Vương', 'K65.CLCCNTT1');
GO

/* =========================================================
   8.1 DU LIEU DANH MUC HOC LIEU MO RONG (QUOC GIA, NGON NGU, DINH DANG, THE LOAI)
   ========================================================= */

INSERT INTO dbo.QuocGia (MaQuocGia, TenQuocGia) VALUES
('VN', N'Việt Nam'),
('US', N'Hoa Kỳ'),
('GB', N'Vương quốc Anh');
GO

INSERT INTO dbo.NgonNgu (MaNgonNgu, TenNgonNgu) VALUES
('VIE', N'Tiếng Việt'),
('ENG', N'Tiếng Anh');
GO

INSERT INTO dbo.DinhDang (TenDinhDang) VALUES
(N'Sách in'),
(N'Sách điện tử (PDF)'),
(N'Luận văn / Đồ án');
GO

INSERT INTO dbo.TheLoai (TenTheLoai) VALUES
(N'Công nghệ thông tin'),
(N'Khoa học cơ bản & Toán'),
(N'Kỹ thuật công trình & Giao thông'),
(N'Kinh tế & Logistics'),
(N'Lý luận chính trị & Pháp luật');
GO

/* DANH MUC NHA XUAT BAN */
INSERT INTO dbo.NhaXuatBan (TenNhaXuatBan, MaQuocGia) VALUES
(N'NXB Bách Khoa Hà Nội', 'VN'),
(N'NXB Chính Trị Quốc Gia Sự Thật', 'VN'),
(N'NXB Giao Thông Vận Tải', 'VN'),
(N'NXB Giáo Dục Việt Nam', 'VN'),
(N'NXB Khoa Học Và Kỹ Thuật', 'VN'),
(N'NXB Lao Động', 'VN'),
(N'NXB Thông Tin Và Truyền Thông', 'VN'),
(N'NXB Tài Chính', 'VN'),
(N'NXB Xây Dựng', 'VN'),
(N'NXB Đại Học Quốc Gia Hà Nội', 'VN'),
(N'NXB Đại Học Sư Phạm', 'VN');
GO

/* DANH MUC TAC GIA */
INSERT INTO dbo.TacGia (HoTen, MaQuocGia) VALUES
(N'Bùi Việt Hà', 'VN'),
(N'Phạm Minh Chuẩn', 'VN'),
(N'Trịnh Thị Nhị', 'VN'),
(N'Nguyễn Văn Quyết', 'VN'),
(N'Nguyễn Thế Lộc', 'VN'),
(N'Nguyễn Hồng Phương', 'VN'),
(N'Học viện Công nghệ Bưu chính Viễn thông', 'VN'),
(N'Phạm Văn Ất', 'VN'),
(N'Nguyễn Hiếu Cường', 'VN'),
(N'Lê Trường Thông', 'VN'),
(N'Nguyễn Tuấn Đăng', 'VN'),
(N'Phạm Văn Cường', 'VN'),
(N'Đoàn Văn Ban', 'VN'),
(N'Đậu Hải Long', 'VN'),
(N'Nguyễn Đình Trí', 'VN'),
(N'Trần Việt Dũng', 'VN'),
(N'Trần Xuân Hiển', 'VN'),
(N'Tống Đình Quỳ', 'VN'),
(N'Lương Duyên Bình', 'VN'),
(N'Nguyễn Văn Vượng', 'VN'),
(N'Lê Ngọc Hồng', 'VN'),
(N'Đỗ Sanh', 'VN'),
(N'Vũ Công Ngữ', 'VN'),
(N'Nguyễn Văn Quảng', 'VN'),
(N'Dương Học Hải', 'VN'),
(N'Nguyễn Quang Chiêu', 'VN'),
(N'Nguyễn Đình Cống', 'VN'),
(N'Bùi Hải', 'VN'),
(N'Trần Thế Sơn', 'VN'),
(N'Lê Cung', 'VN'),
(N'Michael H. Hugos', 'VN'),
(N'Đặng Đình Đào', 'VN'),
(N'Vũ Trọng Phong', 'VN'),
(N'Trần Thị Hoa', 'VN'),
(N'Nguyễn Văn Công', 'VN'),
(N'Nguyễn Thị Loan', 'VN'),
(N'Bộ Giáo Dục Và Đào Tạo', 'VN');
GO

/* =========================================================
   8.2 CHEN 30 DAU SACH CHUAN GIAO TRINH DAI HOC (CO ANH BIA THAT)
   ========================================================= */

-- [01] Cấu trúc dữ liệu và giải thuật (Python: Thuật toán & Cấu trúc dữ liệu)
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-32451-2', N'Cấu trúc dữ liệu và giải thuật (Python: Thuật toán & Cấu trúc dữ liệu)',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Đại Học Quốc Gia Hà Nội'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 506,
       N'Giáo trình trang bị nền tảng chuyên sâu về thuật toán và các cấu trúc dữ liệu kinh điển: mảng động, danh sách liên kết, ngăn xếp (Stack), hàng đợi (Queue), cây nhị phân, bảng băm (Hash Table) và đồ thị, ứng dụng trực tiếp bằng ngôn ngữ hiện đại.',
       'images/covers/sach_01.jpg';

-- [02] Giáo trình Cơ sở dữ liệu quan hệ - Lý thuyết và Thực hành
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-67-2103-1', N'Giáo trình Cơ sở dữ liệu quan hệ - Lý thuyết và Thực hành',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Khoa Học Và Kỹ Thuật'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2022, 1, 328,
       N'Trình bày có hệ thống mô hình thực thể liên kết (ERD), đại số quan hệ, phụ thuộc hàm, các dạng chuẩn chuẩn hóa dữ liệu (1NF, 2NF, 3NF, BCNF) cùng kỹ thuật thiết kế và truy vấn SQL tối ưu trên hệ quản trị CSDL.',
       'images/covers/sach_02.jpg';

-- [03] Giáo trình Mạng máy tính
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-54-8921-0', N'Giáo trình Mạng máy tính',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Đại Học Sư Phạm'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2022, 1, 272,
       N'Tài liệu giảng dạy chuẩn mực về kiến trúc mạng phân tầng OSI và TCP/IP, các giao thức tầng ứng dụng (HTTP, DNS), giao vận (TCP, UDP), mạng (IP, định tuyến OSPF/BGP) và kỹ thuật an toàn thông tin cơ bản trong môi trường truyền số liệu.',
       'images/covers/sach_03.jpg';

-- [04] Giáo trình Cơ sở An toàn thông tin
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-80-4512-8', N'Giáo trình Cơ sở An toàn thông tin',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Thông Tin Và Truyền Thông'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2021, 1, 350,
       N'Giáo trình cốt lõi trang bị các nguyên lý bảo mật hệ thống: mã hóa đối xứng (AES, DES), mã hóa bất đối xứng (RSA, ECC), chữ ký số, hàm băm mật mã (SHA), xác thực người dùng và các chính sách phòng chống tấn công mạng.',
       'images/covers/sach_04.jpg';

-- [05] Giáo trình Kỹ thuật lập trình C căn bản và nâng cao
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-67-1950-2', N'Giáo trình Kỹ thuật lập trình C căn bản và nâng cao',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giao Thông Vận Tải'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2021, 1, 440,
       N'Tác phẩm kinh điển của GS. Phạm Văn Ất hướng dẫn kỹ thuật lập trình từ cơ bản đến nâng cao: cấu trúc điều khiển, mảng, con trỏ (pointer), cấp phát động bộ nhớ, xử lý tệp tin và các kỹ thuật giải thuật tối ưu trên ngôn ngữ C.',
       'images/covers/sach_05.jpg';

-- [06] Giáo trình C++ và Lập trình hướng đối tượng
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-67-1845-1', N'Giáo trình C++ và Lập trình hướng đối tượng',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Khoa Học Và Kỹ Thuật'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2022, 1, 488,
       N'Cung cấp nền tảng vững chắc về tư duy hướng đối tượng (OOP): lớp và đối tượng (Class & Object), tính đóng gói, kế thừa, đa hình, hàm bạn, nạp chồng toán tử (operator overloading), khuôn hình (Template) và thư viện chuẩn STL.',
       'images/covers/sach_06.jpg';

-- [07] Giáo trình Trí tuệ nhân tạo, Học máy và Học sâu
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-67-2415-5', N'Giáo trình Trí tuệ nhân tạo, Học máy và Học sâu',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Khoa Học Và Kỹ Thuật'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 380,
       N'Tài liệu học thuật hiện đại trình bày các phương pháp tìm kiếm không gian trạng thái, thuật toán học máy có giám sát và không giám sát (SVM, Random Forest, K-Means), và mạng nơ-ron tích chập (CNN, RNN) trong xử lý hình ảnh và ngôn ngữ tự nhiên.',
       'images/covers/sach_07.jpg';

-- [08] Giáo trình Kiến trúc máy tính và Hợp ngữ
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-938-634-3', N'Giáo trình Kiến trúc máy tính và Hợp ngữ',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Bách Khoa Hà Nội'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2022, 1, 360,
       N'Nghiên cứu cấu trúc phần cứng máy tính: tập lệnh kiến trúc x86/ARM, ALU, đơn vị điều khiển (CU), hệ thống nhớ phân cấp (Cache, RAM), đường ống xử lý lệnh (Pipelining) và kỹ thuật lập trình hợp ngữ Assembly cơ bản.',
       'images/covers/sach_08.jpg';

-- [09] Giáo trình Kỹ nghệ phần mềm (Công nghệ phần mềm)
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-28731-2', N'Giáo trình Kỹ nghệ phần mềm (Công nghệ phần mềm)',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giáo Dục Việt Nam'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2021, 1, 310,
       N'Trang bị quy trình phát triển phần mềm chuyên nghiệp: phân tích yêu cầu (SRS), mô hình hóa UML, thiết kế kiến trúc hệ thống, quy trình Agile/Scrum, kiểm thử tự động (Unit Test, Integration Test) và quản trị dự án phần mềm.',
       'images/covers/sach_09.jpg';

-- [10] Giáo trình Lập trình Python cơ bản và ứng dụng
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-80-5120-4', N'Giáo trình Lập trình Python cơ bản và ứng dụng',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Thông Tin Và Truyền Thông'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Công nghệ thông tin'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 320,
       N'Hướng dẫn thực hành ngôn ngữ Python từ cú pháp cơ bản, xử lý ngoại lệ, thao tác file, đến ứng dụng phân tích dữ liệu kỹ thuật với thư viện NumPy, Pandas, Matplotlib và phát triển web với Django/Flask.',
       'images/covers/sach_10.jpg';

-- [11] Toán học cao cấp - Tập 1: Đại số và Hình học giải tích
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-34120-5', N'Toán học cao cấp - Tập 1: Đại số và Hình học giải tích',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giáo Dục Việt Nam'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Khoa học cơ bản & Toán'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2024, 1, 296,
       N'Bộ giáo trình chuẩn quốc gia dành cho sinh viên kỹ thuật: lý thuyết định thức, ma trận, hệ phương trình đại số tuyến tính, không gian véc tơ, ánh xạ tuyến tính, dạng toàn phương và hình học giải tích trong không gian.',
       'images/covers/sach_11.jpg';

-- [12] Toán học cao cấp - Tập 2: Giải tích một biến và nhiều biến
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-34121-2', N'Toán học cao cấp - Tập 2: Giải tích một biến và nhiều biến',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giáo Dục Việt Nam'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Khoa học cơ bản & Toán'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2024, 1, 384,
       N'Nội dung giải tích nền tảng: giới hạn và liên tục của hàm số, phép tính vi phân hàm một biến và nhiều biến, tích phân bất định, tích phân xác định và ứng dụng hình học, vật lý trong kỹ thuật.',
       'images/covers/sach_12.jpg';

-- [13] Toán học cao cấp - Tập 3: Chuỗi và Phương trình vi phân
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-34122-9', N'Toán học cao cấp - Tập 3: Chuỗi và Phương trình vi phân',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giáo Dục Việt Nam'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Khoa học cơ bản & Toán'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2024, 1, 340,
       N'Nghiên cứu chuỗi số, chuỗi hàm, chuỗi lũy thừa và chuỗi Fourier; phương trình vi phân cấp 1, phương trình vi phân tuyến tính cấp 2 hệ số hằng và hệ phương trình vi phân ứng dụng mô hình hóa dao động cơ học.',
       'images/covers/sach_13.jpg';

-- [14] Giáo trình Xác suất và Thống kê toán học
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-938-890-3', N'Giáo trình Xác suất và Thống kê toán học',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Bách Khoa Hà Nội'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Khoa học cơ bản & Toán'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 316,
       N'Cung cấp lý thuyết biến ngẫu nhiên rời rạc và liên tục, các quy luật phân phối chuẩn, Poisson, nhị thức; lý thuyết ước lượng tham số, kiểm định giả thuyết thống kê, phân tích phương sai và hồi quy tương quan.',
       'images/covers/sach_14.jpg';

-- [15] Vật lý đại cương - Tập 1: Cơ học và Nhiệt học
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-29105-0', N'Vật lý đại cương - Tập 1: Cơ học và Nhiệt học',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giáo Dục Việt Nam'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Khoa học cơ bản & Toán'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 280,
       N'Giáo trình vật lý đại cương của GS. Lương Duyên Bình: động học chất điểm, động lực học chất điểm và vật rắn, các định luật bảo toàn (cơ năng, động lượng), thuyết động học chất khí và hai nguyên lý nhiệt động lực học.',
       'images/covers/sach_15.jpg';

-- [16] Vật lý đại cương - Tập 2: Điện học và Từ học
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-29106-7', N'Vật lý đại cương - Tập 2: Điện học và Từ học',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giáo Dục Việt Nam'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Khoa học cơ bản & Toán'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 320,
       N'Trình bày trường tĩnh điện, định luật Gauss, điện thế, vật dẫn trong điện trường, từ trường của dòng điện không đổi, định luật Ampe, hiện tượng cảm ứng điện từ và hệ phương trình Maxwell về trường điện từ.',
       'images/covers/sach_16.jpg';

-- [17] Giáo trình Sức bền vật liệu - Tập 1
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-67-1520-7', N'Giáo trình Sức bền vật liệu - Tập 1',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giao Thông Vận Tải'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ thuật công trình & Giao thông'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2022, 1, 364,
       N'Học phần then chốt của kỹ sư xây dựng, giao thông và cơ khí: trạng thái ứng suất và biến dạng, kéo nén đúng tâm, xoắn thuần túy thanh tiết diện tròn, uốn phẳng của dầm và các thuyết bền phá hủy vật liệu.',
       'images/covers/sach_17.jpg';

-- [18] Cơ học kỹ thuật - Tập 1: Tĩnh học và Động học
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-31204-5', N'Cơ học kỹ thuật - Tập 1: Tĩnh học và Động học',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giáo Dục Việt Nam'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ thuật công trình & Giao thông'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 312,
       N'Cung cấp các tiên đề tĩnh học, thu gọn hệ lực không gian, điều kiện cân bằng của hệ lực phẳng và không gian, ma sát; động học điểm và động học vật rắn, chuyển động phức tạp của điểm và cơ cấu truyền động.',
       'images/covers/sach_18.jpg';

-- [19] Giáo trình Nền và Móng công trình
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-82-4102-1', N'Giáo trình Nền và Móng công trình',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Xây Dựng'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ thuật công trình & Giao thông'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2021, 1, 340,
       N'Hướng dẫn tính toán và thiết kế móng nông, móng cọc đài thấp, móng cọc đài cao chịu tải trọng lệch tâm, tính toán độ lún nền đất, sức chịu tải giới hạn của đất nền và các giải pháp gia cố nền đất yếu.',
       'images/covers/sach_19.jpg';

-- [20] Giáo trình Thiết kế hình học đường ô tô và đường cao tốc
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-67-1755-3', N'Giáo trình Thiết kế hình học đường ô tô và đường cao tốc',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giao Thông Vận Tải'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ thuật công trình & Giao thông'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2022, 1, 420,
       N'Tài liệu đào tạo chuyên sâu ngành Cầu đường UTC: nguyên lý vạch tuyến đường, thiết kế bình đồ, trắc dọc, trắc ngang, đường cong chuyển tiếp, tầm nhìn xe chạy và bố trí các nút giao thông giao cắt lập thể.',
       'images/covers/sach_20.jpg';

-- [21] Kết cấu bê tông cốt thép - Nguyên lý thiết kế
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-82-5301-7', N'Kết cấu bê tông cốt thép - Nguyên lý thiết kế',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Xây Dựng'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ thuật công trình & Giao thông'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 450,
       N'Trang bị phương pháp tính toán cấu kiện bê tông cốt thép theo trạng thái giới hạn (TCVN 5574): tính toán chịu uốn trên tiết diện thẳng góc và nghiêng, chịu nén đúng tâm và lệch tâm, độ võng và bề rộng vết nứt.',
       'images/covers/sach_21.jpg';

-- [22] Giáo trình Cơ sở Kỹ thuật nhiệt
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-0-30514-6', N'Giáo trình Cơ sở Kỹ thuật nhiệt',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Giáo Dục Việt Nam'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ thuật công trình & Giao thông'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2022, 1, 368,
       N'Nghiên cứu quá trình nhiệt động của khí lý tưởng, chu trình nhiệt động của động cơ đốt trong, turbine khí và máy lạnh; lý thuyết truyền nhiệt: dẫn nhiệt ổn định, trao đổi nhiệt đối lưu và bức xạ nhiệt công nghiệp.',
       'images/covers/sach_22.jpg';

-- [23] Giáo trình Nguyên lý máy
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-67-2015-8', N'Giáo trình Nguyên lý máy',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Khoa Học Và Kỹ Thuật'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kỹ thuật công trình & Giao thông'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2022, 1, 350,
       N'Cấu tạo và xếp loại cơ cấu, phân tích động học và lực học cơ cấu phẳng bằng phương pháp đồ họa và giải tích, ma sát trong khớp động, cân bằng cơ cấu và động lực học máy.',
       'images/covers/sach_23.jpg';

-- [24] Nguyên lý Quản trị chuỗi cung ứng và Logistics
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-59-9812-4', N'Nguyên lý Quản trị chuỗi cung ứng và Logistics',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Lao Động'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kinh tế & Logistics'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 396,
       N'Cẩm nang toàn diện về 5 khâu cốt lõi của chuỗi cung ứng: Sản xuất, Tồn kho, Địa điểm, Vận tải và Thông tin. Phân tích tối ưu hóa logistics cảng biển, kho bãi và hệ thống phân phối đa phương thức.',
       'images/covers/sach_24.jpg';

-- [25] Giáo trình Kinh tế vi mô
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-99-4215-0', N'Giáo trình Kinh tế vi mô',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Đại Học Quốc Gia Hà Nội'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kinh tế & Logistics'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 345,
       N'Trang bị lý thuyết kinh tế học cơ sở: cung - cầu và độ co giãn, lý thuyết hành vi người tiêu dùng, tối ưu hóa sản xuất và chi phí doanh nghiệp, cấu trúc thị trường cạnh tranh hoàn hảo, độc quyền và cạnh tranh độc quyền.',
       'images/covers/sach_25.jpg';

-- [26] Giáo trình Kinh tế vĩ mô
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-99-4216-7', N'Giáo trình Kinh tế vĩ mô',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Đại Học Quốc Gia Hà Nội'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kinh tế & Logistics'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 380,
       N'Nghiên cứu hoạt động nền kinh tế tổng thể: đo lường sản lượng quốc gia (GDP/GNP), lạm phát, thất nghiệp, mô hình tổng cung - tổng cầu (AS-AD), thị trường tiền tệ, chính sách tài khóa và tiền tệ của Ngân hàng Trung ương.',
       'images/covers/sach_26.jpg';

-- [27] Giáo trình Nguyên lý kế toán
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-79-2810-1', N'Giáo trình Nguyên lý kế toán',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Tài Chính'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Kinh tế & Logistics'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 410,
       N'Giáo trình nền tảng kế toán tài chính: đối tượng và phương pháp kế toán, chứng từ và kiểm kê tài sản, tài khoản và ghi sổ kép, tính giá các đối tượng kế toán, và quy trình lập Báo cáo tài chính chuẩn mực.',
       'images/covers/sach_27.jpg';

-- [28] Giáo trình Triết học Mác - Lênin (Dành cho bậc đại học hệ không chuyên lý luận chính trị)
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-57-7120-9', N'Giáo trình Triết học Mác - Lênin (Dành cho bậc đại học hệ không chuyên lý luận chính trị)',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Chính Trị Quốc Gia Sự Thật'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Lý luận chính trị & Pháp luật'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 492,
       N'Giáo trình chuẩn quốc gia ban hành theo chương trình mới của Bộ GD&ĐT: Triết học và vai trò của triết học trong đời sống xã hội; Chủ nghĩa duy vật biện chứng (vật chất và ý thức, phép biện chứng duy vật); Chủ nghĩa duy vật lịch sử.',
       'images/covers/sach_28.jpg';

-- [29] Giáo trình Kinh tế chính trị Mác – Lênin (Dành cho bậc đại học)
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-57-7121-6', N'Giáo trình Kinh tế chính trị Mác – Lênin (Dành cho bậc đại học)',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Chính Trị Quốc Gia Sự Thật'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Lý luận chính trị & Pháp luật'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 360,
       N'Cung cấp lý luận hàng hóa, thị trường và vai trò của các chủ thể tham gia thị trường; sản xuất giá trị thặng dư; cạnh tranh và độc quyền trong nền kinh tế thị trường; kinh tế thị trường định hướng XHCN tại Việt Nam.',
       'images/covers/sach_29.jpg';

-- [30] Giáo trình Tư tưởng Hồ Chí Minh (Dành cho bậc đại học)
INSERT INTO dbo.TaiLieu (ISBN, TenTaiLieu, MaNhaXuatBan, MaNgonNgu, MaTheLoai, MaDinhDang, NamXuatBan, LanXuatBan, SoTrang, MoTa, AnhBia)
SELECT '978-604-57-7122-3', N'Giáo trình Tư tưởng Hồ Chí Minh (Dành cho bậc đại học)',
       (SELECT MaNhaXuatBan FROM dbo.NhaXuatBan WHERE TenNhaXuatBan = N'NXB Chính Trị Quốc Gia Sự Thật'),
       'VIE',
       (SELECT MaTheLoai FROM dbo.TheLoai WHERE TenTheLoai = N'Lý luận chính trị & Pháp luật'),
       (SELECT MaDinhDang FROM dbo.DinhDang WHERE TenDinhDang = N'Sách in'),
       2023, 1, 320,
       N'Hệ thống hóa nguồn gốc, quá trình hình thành và phát triển tư tưởng Hồ Chí Minh: tư tưởng về độc lập dân tộc và chủ nghĩa xã hội, đại đoàn kết toàn dân tộc, xây dựng Nhà nước của nhân dân, do nhân dân, vì nhân dân và đạo đức cách mạng.',
       'images/covers/sach_30.jpg';

GO

/* LIEN KET TAI LIEU - TAC GIA */
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-32451-2'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Bùi Việt Hà'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2103-1'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Phạm Minh Chuẩn'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2103-1'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trịnh Thị Nhị'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2103-1'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Văn Quyết'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-54-8921-0'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Thế Lộc'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-54-8921-0'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Hồng Phương'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-4512-8'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Học viện Công nghệ Bưu chính Viễn thông'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1950-2'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Phạm Văn Ất'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1950-2'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Hiếu Cường'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1845-1'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Phạm Văn Ất'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1845-1'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Lê Trường Thông'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2415-5'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Tuấn Đăng'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-634-3'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Phạm Văn Cường'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-28731-2'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Đoàn Văn Ban'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-5120-4'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Đậu Hải Long'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34120-5'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Đình Trí'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34120-5'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trần Việt Dũng'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34120-5'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trần Xuân Hiển'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34121-2'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Đình Trí'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34121-2'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trần Việt Dũng'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34121-2'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trần Xuân Hiển'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34122-9'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Đình Trí'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34122-9'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trần Việt Dũng'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34122-9'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trần Xuân Hiển'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-890-3'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Tống Đình Quỳ'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29105-0'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Lương Duyên Bình'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29106-7'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Lương Duyên Bình'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1520-7'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Văn Vượng'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1520-7'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Lê Ngọc Hồng'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-31204-5'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Đỗ Sanh'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-4102-1'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Vũ Công Ngữ'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-4102-1'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Văn Quảng'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1755-3'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Dương Học Hải'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1755-3'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Quang Chiêu'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-5301-7'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Đình Cống'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-30514-6'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Bùi Hải'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-30514-6'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trần Thế Sơn'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2015-8'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Lê Cung'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-59-9812-4'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Michael H. Hugos'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-59-9812-4'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Đặng Đình Đào'),
       N'Dịch giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4215-0'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Vũ Trọng Phong'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4215-0'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Trần Thị Hoa'),
       N'Đồng tác giả';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4216-7'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Văn Công'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-79-2810-1'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Nguyễn Thị Loan'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7120-9'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Bộ Giáo Dục Và Đào Tạo'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7121-6'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Bộ Giáo Dục Và Đào Tạo'),
       N'Chủ biên';
INSERT INTO dbo.TaiLieu_TacGia (MaTaiLieu, MaTacGia, VaiTroTacGia)
SELECT (SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7122-3'),
       (SELECT MaTacGia FROM dbo.TacGia WHERE HoTen = N'Bộ Giáo Dục Và Đào Tạo'),
       N'Chủ biên';
GO

/* =========================================================
   8.3 BAN SAO VAT LY (MA VACH + VI TRI KE KHO THU VIEN UTC)
   Moi dau sach tao tu 3 den 4 ban sao de luu thong muon tra
   ========================================================= */
-- Ban sao cho sach 01: Cấu trúc dữ liệu và giải thuật (Pyt
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-32451-2'), 'UTC-01-01', N'Kệ IT-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-32451-2'), 'UTC-01-02', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-32451-2'), 'UTC-01-03', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-32451-2'), 'UTC-01-04', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 02: Giáo trình Cơ sở dữ liệu quan hệ - 
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2103-1'), 'UTC-02-01', N'Kệ IT-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2103-1'), 'UTC-02-02', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2103-1'), 'UTC-02-03', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2103-1'), 'UTC-02-04', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 03: Giáo trình Mạng máy tính
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-54-8921-0'), 'UTC-03-01', N'Kệ IT-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-54-8921-0'), 'UTC-03-02', N'Kệ IT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-54-8921-0'), 'UTC-03-03', N'Kệ IT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-54-8921-0'), 'UTC-03-04', N'Kệ IT-02', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 04: Giáo trình Cơ sở An toàn thông tin
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-4512-8'), 'UTC-04-01', N'Kệ IT-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-4512-8'), 'UTC-04-02', N'Kệ IT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-4512-8'), 'UTC-04-03', N'Kệ IT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-4512-8'), 'UTC-04-04', N'Kệ IT-02', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 05: Giáo trình Kỹ thuật lập trình C căn
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1950-2'), 'UTC-05-01', N'Kệ IT-03', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1950-2'), 'UTC-05-02', N'Kệ IT-03', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1950-2'), 'UTC-05-03', N'Kệ IT-03', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1950-2'), 'UTC-05-04', N'Kệ IT-03', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 06: Giáo trình C++ và Lập trình hướng đ
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1845-1'), 'UTC-06-01', N'Kệ IT-03', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1845-1'), 'UTC-06-02', N'Kệ IT-03', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1845-1'), 'UTC-06-03', N'Kệ IT-03', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1845-1'), 'UTC-06-04', N'Kệ IT-03', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 07: Giáo trình Trí tuệ nhân tạo, Học má
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2415-5'), 'UTC-07-01', N'Kệ IT-04', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2415-5'), 'UTC-07-02', N'Kệ IT-04', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2415-5'), 'UTC-07-03', N'Kệ IT-04', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2415-5'), 'UTC-07-04', N'Kệ IT-04', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 08: Giáo trình Kiến trúc máy tính và Hợ
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-634-3'), 'UTC-08-01', N'Kệ IT-04', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-634-3'), 'UTC-08-02', N'Kệ IT-04', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-634-3'), 'UTC-08-03', N'Kệ IT-04', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-634-3'), 'UTC-08-04', N'Kệ IT-04', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 09: Giáo trình Kỹ nghệ phần mềm (Công n
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-28731-2'), 'UTC-09-01', N'Kệ IT-05', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-28731-2'), 'UTC-09-02', N'Kệ IT-05', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-28731-2'), 'UTC-09-03', N'Kệ IT-05', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-28731-2'), 'UTC-09-04', N'Kệ IT-05', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 10: Giáo trình Lập trình Python cơ bản 
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-5120-4'), 'UTC-10-01', N'Kệ IT-05', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-5120-4'), 'UTC-10-02', N'Kệ IT-05', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-5120-4'), 'UTC-10-03', N'Kệ IT-05', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-80-5120-4'), 'UTC-10-04', N'Kệ IT-05', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 11: Toán học cao cấp - Tập 1: Đại số và
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34120-5'), 'UTC-11-01', N'Kệ CB-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34120-5'), 'UTC-11-02', N'Kệ CB-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34120-5'), 'UTC-11-03', N'Kệ CB-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34120-5'), 'UTC-11-04', N'Kệ CB-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 12: Toán học cao cấp - Tập 2: Giải tích
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34121-2'), 'UTC-12-01', N'Kệ CB-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34121-2'), 'UTC-12-02', N'Kệ CB-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34121-2'), 'UTC-12-03', N'Kệ CB-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34121-2'), 'UTC-12-04', N'Kệ CB-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 13: Toán học cao cấp - Tập 3: Chuỗi và 
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34122-9'), 'UTC-13-01', N'Kệ CB-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34122-9'), 'UTC-13-02', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34122-9'), 'UTC-13-03', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-34122-9'), 'UTC-13-04', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 14: Giáo trình Xác suất và Thống kê toá
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-890-3'), 'UTC-14-01', N'Kệ CB-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-890-3'), 'UTC-14-02', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-890-3'), 'UTC-14-03', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-938-890-3'), 'UTC-14-04', N'Kệ CB-02', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 15: Vật lý đại cương - Tập 1: Cơ học và
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29105-0'), 'UTC-15-01', N'Kệ CB-03', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29105-0'), 'UTC-15-02', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29105-0'), 'UTC-15-03', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29105-0'), 'UTC-15-04', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 16: Vật lý đại cương - Tập 2: Điện học 
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29106-7'), 'UTC-16-01', N'Kệ CB-03', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29106-7'), 'UTC-16-02', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29106-7'), 'UTC-16-03', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-29106-7'), 'UTC-16-04', N'Kệ CB-03', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 17: Giáo trình Sức bền vật liệu - Tập 1
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1520-7'), 'UTC-17-01', N'Kệ GT-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1520-7'), 'UTC-17-02', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1520-7'), 'UTC-17-03', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1520-7'), 'UTC-17-04', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 18: Cơ học kỹ thuật - Tập 1: Tĩnh học v
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-31204-5'), 'UTC-18-01', N'Kệ GT-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-31204-5'), 'UTC-18-02', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-31204-5'), 'UTC-18-03', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-31204-5'), 'UTC-18-04', N'Kệ GT-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 19: Giáo trình Nền và Móng công trình
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-4102-1'), 'UTC-19-01', N'Kệ GT-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-4102-1'), 'UTC-19-02', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-4102-1'), 'UTC-19-03', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-4102-1'), 'UTC-19-04', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 20: Giáo trình Thiết kế hình học đường 
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1755-3'), 'UTC-20-01', N'Kệ GT-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1755-3'), 'UTC-20-02', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1755-3'), 'UTC-20-03', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-1755-3'), 'UTC-20-04', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 21: Kết cấu bê tông cốt thép - Nguyên l
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-5301-7'), 'UTC-21-01', N'Kệ GT-03', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-5301-7'), 'UTC-21-02', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-5301-7'), 'UTC-21-03', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-82-5301-7'), 'UTC-21-04', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 22: Giáo trình Cơ sở Kỹ thuật nhiệt
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-30514-6'), 'UTC-22-01', N'Kệ GT-03', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-30514-6'), 'UTC-22-02', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-30514-6'), 'UTC-22-03', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-0-30514-6'), 'UTC-22-04', N'Kệ GT-03', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 23: Giáo trình Nguyên lý máy
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2015-8'), 'UTC-23-01', N'Kệ GT-04', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2015-8'), 'UTC-23-02', N'Kệ GT-04', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2015-8'), 'UTC-23-03', N'Kệ GT-04', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-67-2015-8'), 'UTC-23-04', N'Kệ GT-04', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 24: Nguyên lý Quản trị chuỗi cung ứng v
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-59-9812-4'), 'UTC-24-01', N'Kệ KT-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-59-9812-4'), 'UTC-24-02', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-59-9812-4'), 'UTC-24-03', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-59-9812-4'), 'UTC-24-04', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 25: Giáo trình Kinh tế vi mô
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4215-0'), 'UTC-25-01', N'Kệ KT-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4215-0'), 'UTC-25-02', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4215-0'), 'UTC-25-03', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4215-0'), 'UTC-25-04', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 26: Giáo trình Kinh tế vĩ mô
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4216-7'), 'UTC-26-01', N'Kệ KT-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4216-7'), 'UTC-26-02', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4216-7'), 'UTC-26-03', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-99-4216-7'), 'UTC-26-04', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 27: Giáo trình Nguyên lý kế toán
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-79-2810-1'), 'UTC-27-01', N'Kệ KT-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-79-2810-1'), 'UTC-27-02', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-79-2810-1'), 'UTC-27-03', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-79-2810-1'), 'UTC-27-04', N'Kệ KT-02', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 28: Giáo trình Triết học Mác - Lênin (D
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7120-9'), 'UTC-28-01', N'Kệ LL-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7120-9'), 'UTC-28-02', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7120-9'), 'UTC-28-03', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7120-9'), 'UTC-28-04', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 29: Giáo trình Kinh tế chính trị Mác – 
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7121-6'), 'UTC-29-01', N'Kệ LL-01', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7121-6'), 'UTC-29-02', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7121-6'), 'UTC-29-03', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7121-6'), 'UTC-29-04', N'Kệ LL-01', N'Tốt', N'Có sẵn', '2024-02-20');
-- Ban sao cho sach 30: Giáo trình Tư tưởng Hồ Chí Minh (Dà
INSERT INTO dbo.BanSao (MaTaiLieu, MaVach, ViTriKe, TinhTrang, TrangThai, NgayNhap) VALUES
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7122-3'), 'UTC-30-01', N'Kệ LL-02', N'Mới', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7122-3'), 'UTC-30-02', N'Kệ LL-02', N'Tốt', N'Có sẵn', '2024-01-15'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7122-3'), 'UTC-30-03', N'Kệ LL-02', N'Tốt', N'Có sẵn', '2024-02-20'),
((SELECT MaTaiLieu FROM dbo.TaiLieu WHERE ISBN = '978-604-57-7122-3'), 'UTC-30-04', N'Kệ LL-02', N'Tốt', N'Có sẵn', '2024-02-20');
GO

/* =========================================================
   8.4 TAI KHOAN QUAN TRI & THU THU & THE BAN DOC DEMO
   ========================================================= */
INSERT INTO dbo.NhanVien (MaSoNhanVien, HoTen, ChucVu, TrangThai) VALUES
('NV001', N'Trần Văn Quản Trị', N'Quản trị viên hệ thống', N'Đang làm việc'),
('NV002', N'Lê Thị Thủ Thư', N'Cán bộ nghiệp vụ lưu thông', N'Đang làm việc');
GO

-- Mat khau demo hash ASP.NET Core cho 'Admin@123':
INSERT INTO dbo.TaiKhoan (TenDangNhap, EmailDangNhap, MatKhauHash, MaNhanVien, TrangThai)
VALUES ('admin', 'admin@utc.edu.vn', 'AQAAAAIAAYagAAAAEO5aXUu7YVn8nNQqXoX2J9w2zDk4P1sL0q+M=', 1, N'Hoạt động');

INSERT INTO dbo.TaiKhoan_VaiTro (MaTaiKhoan, MaVaiTro) VALUES (1, 'ADMIN');
GO

-- Tao TaiKhoan cho cac sinh vien K65.CLCCNTT1 (Mat khau mac dinh: Ten + MSV, vi du Dat242630923):
INSERT INTO dbo.TaiKhoan (TenDangNhap, EmailDangNhap, MatKhauHash, MaBanDoc, TrangThai)
SELECT bd.MaSinhVien, bd.MaSinhVien + '@utc.edu.vn', 'HASH_' + bd.MaSinhVien, bd.MaBanDoc, N'Hoạt động'
FROM dbo.BanDoc bd
WHERE NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan tk WHERE tk.MaBanDoc = bd.MaBanDoc);
GO

-- Gan vai tro BANDOC cho tat ca tai khoan sinh vien:
INSERT INTO dbo.TaiKhoan_VaiTro (MaTaiKhoan, MaVaiTro)
SELECT tk.MaTaiKhoan, 'BANDOC'
FROM dbo.TaiKhoan tk
WHERE tk.MaBanDoc IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan_VaiTro tv WHERE tv.MaTaiKhoan = tk.MaTaiKhoan);
GO

-- Tao TheBanDoc mau cho top 10 sinh vien de test muon tra ngay:
INSERT INTO dbo.TheBanDoc (MaBanDoc, SoThe, NgayCap, NgayHetHan, TrangThai)
SELECT MaBanDoc, 'THE-UTC-' + MaSinhVien, '2024-09-01', '2028-06-30', N'Hoạt động'
FROM dbo.BanDoc WHERE MaBanDoc <= 10;
GO

/* =========================================================
   9. INDEX HO TRO TRUY VAN
   ========================================================= */

CREATE INDEX IX_BanDoc_HoTen ON dbo.BanDoc(HoTen);
CREATE INDEX IX_BanDoc_MaLop ON dbo.BanDoc(MaLop);
CREATE INDEX IX_TaiLieu_Ten ON dbo.TaiLieu(TenTaiLieu);
CREATE INDEX IX_BanSao_MaTaiLieu ON dbo.BanSao(MaTaiLieu);
CREATE INDEX IX_PhieuMuon_The ON dbo.PhieuMuon(MaTheBanDoc);
CREATE INDEX IX_ChiTietMuon_BanSao ON dbo.ChiTietMuon(MaBanSao);
CREATE INDEX IX_DatTruoc_BanDoc ON dbo.DatTruoc(MaBanDoc);
GO

/* =========================================================
   10. KIEM TRA DU LIEU BAN DOC
   ========================================================= */

SELECT
    bd.MaSinhVien,
    bd.HoTen,
    l.TenLop,
    bd.TrangThai
FROM dbo.BanDoc AS bd
LEFT JOIN dbo.Lop AS l ON bd.MaLop = l.MaLop
ORDER BY bd.MaSinhVien;

SELECT COUNT(*) AS TongSoBanDoc
FROM dbo.BanDoc;
GO

SELECT 
    tl.MaTaiLieu,
    tl.ISBN,
    tl.TenTaiLieu,
    tl.NamXuatBan,
    tl.SoTrang,
    tl.AnhBia,
    COUNT(bs.MaBanSao) AS SoLuongBanSao
FROM dbo.TaiLieu tl
LEFT JOIN dbo.BanSao bs ON tl.MaTaiLieu = bs.MaTaiLieu
GROUP BY tl.MaTaiLieu, tl.ISBN, tl.TenTaiLieu, tl.NamXuatBan, tl.SoTrang, tl.AnhBia
ORDER BY tl.MaTaiLieu;

SELECT COUNT(*) AS TongSoDauSach FROM dbo.TaiLieu;
SELECT COUNT(*) AS TongSoBanSaoVatLy FROM dbo.BanSao;

GO

/*
    HUONG DAN TAO TAI KHOAN:
    1. Tao ho so NhanVien hoac su dung ho so BanDoc co san.
    2. Tao TaiKhoan voi MatKhauHash la chuoi hash do ASP.NET Core tao ra.
    3. Gan vai tro trong TaiKhoan_VaiTro.
    4. Tuyet doi khong INSERT mat khau dang ro vao MatKhauHash.

    Vi du truy van sinh vien theo lop:
    SELECT * FROM dbo.BanDoc WHERE MaLop = 'K65.CLCCNTT1';

    Vi du dem so ban sao con san theo dau sach:
    SELECT tl.TenTaiLieu, COUNT(bs.MaBanSao) AS SoBanCoSan
    FROM dbo.TaiLieu tl
    LEFT JOIN dbo.BanSao bs
        ON tl.MaTaiLieu = bs.MaTaiLieu AND bs.TrangThai = N'Có sẵn'
    GROUP BY tl.TenTaiLieu;
*/


/* =========================================================
   11. DU LIEU MAU PHIEU MUON & CHI TIET MUON (QUY DINH MUON 1 NAM - PHAT 100K/QUYEN)
   - Do Van Dat (242630923) muon 5 quyen: 
     + 2 quyen con han (Muon nam 2026, han tra nam 2027) -> Tien nop: 0d
     + 3 quyen den han/qua han (Muon nam 2025, han tra nam 2026) -> Tien nop: 100.000d/quyen -> Tong: 300.000d
   - Cac sinh vien khac muon ngau nhien 1 - 3 quyen.
   ========================================================= */

-- Đảm bảo toàn bộ sinh viên đều có Thẻ Bạn Đọc:
INSERT INTO dbo.TheBanDoc (MaBanDoc, SoThe, NgayCap, NgayHetHan, TrangThai)
SELECT bd.MaBanDoc, 'THE-UTC-' + bd.MaSinhVien, '2024-09-01', '2028-06-30', N'Hoạt động'
FROM dbo.BanDoc bd
WHERE NOT EXISTS (SELECT 1 FROM dbo.TheBanDoc t WHERE t.MaBanDoc = bd.MaBanDoc);
GO

-- 1. TẠO DỮ LIỆU MƯỢN CHO ĐỖ VĂN ĐẠT (MSV: 242630923) - 5 QUYỂN: 2 CÒN HẠN, 3 QUÁ HẠN
DECLARE @MaNV INT = (SELECT TOP 1 MaNhanVien FROM dbo.NhanVien);
DECLARE @TheDat INT = (SELECT t.MaTheBanDoc FROM dbo.TheBanDoc t JOIN dbo.BanDoc b ON t.MaBanDoc = b.MaBanDoc WHERE b.MaSinhVien = '242630923');
DECLARE @MaBanDocDat INT = (SELECT MaBanDoc FROM dbo.BanDoc WHERE MaSinhVien = '242630923');

IF @TheDat IS NOT NULL
BEGIN
    -- Phiếu mượn 1 (Năm 2025) cho 3 cuốn quá hạn (quy định 1 năm):
    INSERT INTO dbo.PhieuMuon (MaTheBanDoc, MaNhanVien, NgayMuon, TrangThai, GhiChu)
    VALUES (@TheDat, @MaNV, '2025-08-25 08:30:00', N'Đang mượn', N'Mượn học phần kỳ 1 năm học 2025-2026');
    DECLARE @PM_Dat_1 INT = SCOPE_IDENTITY();

    -- Phiếu mượn 2 (Năm 2026) cho 2 cuốn còn hạn (quy định 1 năm):
    INSERT INTO dbo.PhieuMuon (MaTheBanDoc, MaNhanVien, NgayMuon, TrangThai, GhiChu)
    VALUES (@TheDat, @MaNV, '2026-03-15 10:00:00', N'Đang mượn', N'Mượn học phần kỳ 2 năm học 2026');
    DECLARE @PM_Dat_2 INT = SCOPE_IDENTITY();

    -- Cuốn 1 (Còn hạn): Cấu trúc dữ liệu và giải thuật (Bản sao 1)
    -- Mượn: 15/03/2026 -> Hạn trả: 15/03/2027 (Thời hạn 1 năm)
    INSERT INTO dbo.ChiTietMuon (MaPhieuMuon, MaBanSao, HanTra, TrangThai)
    VALUES (@PM_Dat_2, 1, '2027-03-15', N'Đang mượn');
    UPDATE dbo.BanSao SET TrangThai = N'Đang mượn' WHERE MaBanSao = 1;

    -- Cuốn 2 (Còn hạn): Giáo trình Lập trình Python (Bản sao 37)
    -- Mượn: 20/05/2026 -> Hạn trả: 20/05/2027 (Thời hạn 1 năm)
    INSERT INTO dbo.ChiTietMuon (MaPhieuMuon, MaBanSao, HanTra, TrangThai)
    VALUES (@PM_Dat_2, 37, '2027-05-20', N'Đang mượn');
    UPDATE dbo.BanSao SET TrangThai = N'Đang mượn' WHERE MaBanSao = 37;

    -- Cuốn 3 (QUÁ HẠN): Giáo trình Cơ sở dữ liệu quan hệ (Bản sao 5)
    -- Mượn: 10/10/2025 -> Hạn trả: 10/10/2026 (Quá hạn) -> Phạt 100k
    INSERT INTO dbo.ChiTietMuon (MaPhieuMuon, MaBanSao, HanTra, TrangThai)
    VALUES (@PM_Dat_1, 5, '2026-10-10', N'Quá hạn');
    DECLARE @CTM_Dat_3 INT = SCOPE_IDENTITY();
    UPDATE dbo.BanSao SET TrangThai = N'Đang mượn' WHERE MaBanSao = 5;

    INSERT INTO dbo.ViPham (MaBanDoc, MaChiTietMuon, LoaiViPham, MoTa, NgayViPham)
    VALUES (@MaBanDocDat, @CTM_Dat_3, N'Mượn quá hạn', N'Quá hạn trả sách Cơ sở dữ liệu quan hệ (thời hạn 1 năm)', '2026-10-11');
    INSERT INTO dbo.TienPhat (MaViPham, SoTien, LyDo, TrangThai)
    VALUES (SCOPE_IDENTITY(), 100000.00, N'Phạt mượn quá hạn 100.000đ/quyển', N'Chưa thu');

    -- Cuốn 4 (QUÁ HẠN): Giáo trình C++ và Lập trình hướng đối tượng (Bản sao 21)
    -- Mượn: 25/08/2025 -> Hạn trả: 25/08/2026 (Quá hạn) -> Phạt 100k
    INSERT INTO dbo.ChiTietMuon (MaPhieuMuon, MaBanSao, HanTra, TrangThai)
    VALUES (@PM_Dat_1, 21, '2026-08-25', N'Quá hạn');
    DECLARE @CTM_Dat_4 INT = SCOPE_IDENTITY();
    UPDATE dbo.BanSao SET TrangThai = N'Đang mượn' WHERE MaBanSao = 21;

    INSERT INTO dbo.ViPham (MaBanDoc, MaChiTietMuon, LoaiViPham, MoTa, NgayViPham)
    VALUES (@MaBanDocDat, @CTM_Dat_4, N'Mượn quá hạn', N'Quá hạn trả sách C++ và Lập trình hướng đối tượng (thời hạn 1 năm)', '2026-08-26');
    INSERT INTO dbo.TienPhat (MaViPham, SoTien, LyDo, TrangThai)
    VALUES (SCOPE_IDENTITY(), 100000.00, N'Phạt mượn quá hạn 100.000đ/quyển', N'Chưa thu');

    -- Cuốn 5 (QUÁ HẠN): Giáo trình Kỹ nghệ phần mềm (Bản sao 33)
    -- Mượn: 01/11/2025 -> Hạn trả: 01/11/2026 (Quá hạn) -> Phạt 100k
    INSERT INTO dbo.ChiTietMuon (MaPhieuMuon, MaBanSao, HanTra, TrangThai)
    VALUES (@PM_Dat_1, 33, '2026-11-01', N'Quá hạn');
    DECLARE @CTM_Dat_5 INT = SCOPE_IDENTITY();
    UPDATE dbo.BanSao SET TrangThai = N'Đang mượn' WHERE MaBanSao = 33;

    INSERT INTO dbo.ViPham (MaBanDoc, MaChiTietMuon, LoaiViPham, MoTa, NgayViPham)
    VALUES (@MaBanDocDat, @CTM_Dat_5, N'Mượn quá hạn', N'Quá hạn trả sách Kỹ nghệ phần mềm (thời hạn 1 năm)', '2026-11-02');
    INSERT INTO dbo.TienPhat (MaViPham, SoTien, LyDo, TrangThai)
    VALUES (SCOPE_IDENTITY(), 100000.00, N'Phạt mượn quá hạn 100.000đ/quyển', N'Chưa thu');
END
GO

-- 2. TẠO DỮ LIỆU MƯỢN MẪU CHO MỘT SỐ SINH VIÊN KHÁC
-- Nguyễn Văn An (242630899): 2 cuốn (1 còn hạn, 1 quá hạn)
DECLARE @TheAn INT = (SELECT t.MaTheBanDoc FROM dbo.TheBanDoc t JOIN dbo.BanDoc b ON t.MaBanDoc = b.MaBanDoc WHERE b.MaSinhVien = '242630899');
DECLARE @MaBanDocAn INT = (SELECT MaBanDoc FROM dbo.BanDoc WHERE MaSinhVien = '242630899');
DECLARE @MaNVAn INT = (SELECT TOP 1 MaNhanVien FROM dbo.NhanVien);

IF @TheAn IS NOT NULL
BEGIN
    INSERT INTO dbo.PhieuMuon (MaTheBanDoc, MaNhanVien, NgayMuon, TrangThai, GhiChu)
    VALUES (@TheAn, @MaNVAn, '2025-09-10 14:00:00', N'Đang mượn', N'Mượn sách chuyên ngành');
    DECLARE @PM_An INT = SCOPE_IDENTITY();

    -- Cuốn 1: Bản sao 9 (Mạng máy tính) - Hạn trả: 10/09/2026 (Quá hạn -> Phạt 100k)
    INSERT INTO dbo.ChiTietMuon (MaPhieuMuon, MaBanSao, HanTra, TrangThai) VALUES (@PM_An, 9, '2026-09-10', N'Quá hạn');
    DECLARE @CTM_An INT = SCOPE_IDENTITY();
    UPDATE dbo.BanSao SET TrangThai = N'Đang mượn' WHERE MaBanSao = 9;

    INSERT INTO dbo.ViPham (MaBanDoc, MaChiTietMuon, LoaiViPham, MoTa, NgayViPham)
    VALUES (@MaBanDocAn, @CTM_An, N'Mượn quá hạn', N'Quá hạn sách Mạng máy tính', '2026-09-11');
    INSERT INTO dbo.TienPhat (MaViPham, SoTien, LyDo, TrangThai)
    VALUES (SCOPE_IDENTITY(), 100000.00, N'Phạt quá hạn 100.000đ/quyển', N'Chưa thu');

    -- Cuốn 2: Bản sao 13 (Cơ sở An toàn thông tin) - Mượn: 10/04/2026 -> Hạn trả: 10/04/2027 (Còn hạn)
    INSERT INTO dbo.ChiTietMuon (MaPhieuMuon, MaBanSao, HanTra, TrangThai) VALUES (@PM_An, 13, '2027-04-10', N'Đang mượn');
    UPDATE dbo.BanSao SET TrangThai = N'Đang mượn' WHERE MaBanSao = 13;
END
GO

-- 3. TRUY VẤN KIỂM TRA SÁCH ĐANG MƯỢN & TÍNH TỔNG TIỀN PHẠT CHO ĐỖ VĂN ĐẠT (242630923):
SELECT 
    b.MaSinhVien,
    b.HoTen,
    tl.TenTaiLieu,
    bs.MaVach,
    pm.NgayMuon,
    ctm.HanTra,
    CASE WHEN GETDATE() > ctm.HanTra THEN N'Đang quá hạn' ELSE N'Đang mượn (Còn hạn)' END AS TrangThaiMuon,
    CASE WHEN GETDATE() > ctm.HanTra THEN 100000.00 ELSE 0.00 END AS TienPhatQuyDinh
FROM dbo.ChiTietMuon ctm
JOIN dbo.PhieuMuon pm ON ctm.MaPhieuMuon = pm.MaPhieuMuon
JOIN dbo.TheBanDoc t ON pm.MaTheBanDoc = t.MaTheBanDoc
JOIN dbo.BanDoc b ON t.MaBanDoc = b.MaBanDoc
JOIN dbo.BanSao bs ON ctm.MaBanSao = bs.MaBanSao
JOIN dbo.TaiLieu tl ON bs.MaTaiLieu = tl.MaTaiLieu
WHERE b.MaSinhVien = '242630923'
ORDER BY ctm.HanTra;

-- DÒNG TỔNG CỘNG PHẢI THANH TOÁN (KẾT QUẢ MONG ĐỢI: 5 CUỐN - 3 QUÁ HẠN - TỔNG 300.000 VNĐ):
SELECT 
    b.MaSinhVien,
    b.HoTen,
    COUNT(ctm.MaChiTietMuon) AS TongSoSachDangMuon,
    SUM(CASE WHEN GETDATE() > ctm.HanTra THEN 1 ELSE 0 END) AS SoSachQuaHan,
    SUM(CASE WHEN GETDATE() > ctm.HanTra THEN 100000.00 ELSE 0.00 END) AS TongTienPhaiThanhToan
FROM dbo.ChiTietMuon ctm
JOIN dbo.PhieuMuon pm ON ctm.MaPhieuMuon = pm.MaPhieuMuon
JOIN dbo.TheBanDoc t ON pm.MaTheBanDoc = t.MaTheBanDoc
JOIN dbo.BanDoc b ON t.MaBanDoc = b.MaBanDoc
WHERE b.MaSinhVien = '242630923'
GROUP BY b.MaSinhVien, b.HoTen;
GO
