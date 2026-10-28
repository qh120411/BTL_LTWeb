
USE [master];
GO
CREATE DATABASE [QLThuVienUTC];
GO
USE [QLThuVienUTC];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

CREATE TABLE dbo.[tQuocGia] (
    [MaQG] char(2) NOT NULL,
    [TenQG] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_tQuocGia] PRIMARY KEY ([MaQG])
);

CREATE TABLE dbo.[tNgonNgu] (
    [MaNN] char(2) NOT NULL,
    [TenNN] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_tNgonNgu] PRIMARY KEY ([MaNN])
);

CREATE TABLE dbo.[tTheLoai] (
    [MaThL] char(6) NOT NULL,
    [TenThL] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_tTheLoai] PRIMARY KEY ([MaThL])
);

CREATE TABLE dbo.[tDinhDang] (
    [MaDD] char(5) NOT NULL,
    [TenDD] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_tDinhDang] PRIMARY KEY ([MaDD])
);

CREATE TABLE dbo.[tVaiTro] (
    [MaVT] char(3) NOT NULL,
    [TenVT] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_tVaiTro] PRIMARY KEY ([MaVT])
);

CREATE TABLE dbo.[tNhanVien] (
    [MaNV] char(7) NOT NULL,
    [HoDem] nvarchar(50) NOT NULL,
    [Ten] nvarchar(30) NOT NULL,
    [NgaySinh] date NOT NULL,
    [GioiTinh] char(1) NOT NULL,
    [DiaChi] nvarchar(200) NULL,
    [SDT] nvarchar(20) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [PhuTrach] nvarchar(100) NULL,
    CONSTRAINT [PK_tNhanVien] PRIMARY KEY ([MaNV])
);

CREATE TABLE dbo.[tTaiKhoan] (
    [MaTK] char(7) NOT NULL,
    [MaNV] char(7) NOT NULL,
    [MaVT] char(3) NOT NULL,
    [TenDangNhap] nvarchar(255) NOT NULL,
    [MatKhau] nvarchar(255) NOT NULL,
    [TrangThai] nvarchar(30) NOT NULL,
    [NgayTao] date NOT NULL,
    CONSTRAINT [PK_tTaiKhoan] PRIMARY KEY ([MaTK]),
    CONSTRAINT UQ_tTaiKhoan_MaNV UNIQUE (MaNV),
    CONSTRAINT UQ_tTaiKhoan_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT [FK_tTaiKhoan_MaNV] FOREIGN KEY ([MaNV]) REFERENCES dbo.[tNhanVien] ([MaNV]),
    CONSTRAINT [FK_tTaiKhoan_MaVT] FOREIGN KEY ([MaVT]) REFERENCES dbo.[tVaiTro] ([MaVT])
);

CREATE TABLE dbo.[tTacGia] (
    [MaTG] char(10) NOT NULL,
    [MaQG] char(2) NOT NULL,
    [HoDem] nvarchar(50) NOT NULL,
    [Ten] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_tTacGia] PRIMARY KEY ([MaTG]),
    CONSTRAINT [FK_tTacGia_MaQG] FOREIGN KEY ([MaQG]) REFERENCES dbo.[tQuocGia] ([MaQG])
);

CREATE TABLE dbo.[tNhaXuatBan] (
    [MaNXB] char(9) NOT NULL,
    [MaQG] char(2) NOT NULL,
    [TenNXB] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_tNhaXuatBan] PRIMARY KEY ([MaNXB]),
    CONSTRAINT [FK_tNhaXuatBan_MaQG] FOREIGN KEY ([MaQG]) REFERENCES dbo.[tQuocGia] ([MaQG])
);

CREATE TABLE dbo.[tTaiLieu] (
    [MaTL] char(10) NOT NULL,
    [MaNXB] char(9) NOT NULL,
    [MaNN] char(2) NOT NULL,
    [MaThL] char(6) NOT NULL,
    [MaDD] char(5) NOT NULL,
    [MaTK] char(7) NOT NULL,
    [TenTL] nvarchar(200) NOT NULL,
    [LanXuatBan] int NULL,
    [NamXuatBan] int NULL,
    [SoTrang] int NULL,
    [KhoCo] char(10) NULL,
    [Anh] nvarchar(255) NULL,
    CONSTRAINT [PK_tTaiLieu] PRIMARY KEY ([MaTL]),
    CONSTRAINT CK_tTaiLieu_LanXB CHECK (LanXuatBan IS NULL OR LanXuatBan > 0),
    CONSTRAINT CK_tTaiLieu_NamXB CHECK (NamXuatBan IS NULL OR NamXuatBan BETWEEN 1000 AND 9999),
    CONSTRAINT CK_tTaiLieu_SoTrang CHECK (SoTrang IS NULL OR SoTrang > 0),
    CONSTRAINT [FK_tTaiLieu_MaNXB] FOREIGN KEY ([MaNXB]) REFERENCES dbo.[tNhaXuatBan] ([MaNXB]),
    CONSTRAINT [FK_tTaiLieu_MaNN] FOREIGN KEY ([MaNN]) REFERENCES dbo.[tNgonNgu] ([MaNN]),
    CONSTRAINT [FK_tTaiLieu_MaThL] FOREIGN KEY ([MaThL]) REFERENCES dbo.[tTheLoai] ([MaThL]),
    CONSTRAINT [FK_tTaiLieu_MaDD] FOREIGN KEY ([MaDD]) REFERENCES dbo.[tDinhDang] ([MaDD]),
    CONSTRAINT [FK_tTaiLieu_MaTK] FOREIGN KEY ([MaTK]) REFERENCES dbo.[tTaiKhoan] ([MaTK])
);

CREATE TABLE dbo.[tTaiLieu_TacGia] (
    [MaTL] char(10) NOT NULL,
    [MaTG] char(10) NOT NULL,
    [VaiTro] nvarchar(50) NULL,
    CONSTRAINT [PK_tTaiLieu_TacGia] PRIMARY KEY ([MaTL], [MaTG]),
    CONSTRAINT [FK_tTaiLieu_TacGia_MaTL] FOREIGN KEY ([MaTL]) REFERENCES dbo.[tTaiLieu] ([MaTL]),
    CONSTRAINT [FK_tTaiLieu_TacGia_MaTG] FOREIGN KEY ([MaTG]) REFERENCES dbo.[tTacGia] ([MaTG])
);

CREATE TABLE dbo.[tBanSao] (
    [MaBS] char(14) NOT NULL,
    [MaTL] char(10) NOT NULL,
    [TrangThai] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_tBanSao] PRIMARY KEY ([MaBS]),
    CONSTRAINT [FK_tBanSao_MaTL] FOREIGN KEY ([MaTL]) REFERENCES dbo.[tTaiLieu] ([MaTL])
);

CREATE TABLE dbo.[tBanDoc] (
    [MaBD] char(9) NOT NULL,
    [HoDem] nvarchar(50) NOT NULL,
    [Ten] nvarchar(30) NOT NULL,
    [NgaySinh] date NOT NULL,
    [GioiTinh] char(1) NOT NULL,
    [DiaChi] nvarchar(200) NULL,
    [SDT] nvarchar(20) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [GhiChuViPham] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_tBanDoc] PRIMARY KEY ([MaBD])
);

CREATE TABLE dbo.[tTheBanDoc] (
    [MaTBD] char(12) NOT NULL,
    [MaBD] char(9) NOT NULL,
    [MaTK] char(7) NOT NULL,
    [NgayCap] date NOT NULL,
    [NgayHetHan] date NULL,
    [TrangThai] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_tTheBanDoc] PRIMARY KEY ([MaTBD]),
    CONSTRAINT UQ_tTheBanDoc_MaBD UNIQUE (MaBD),
    CONSTRAINT CK_tTheBanDoc_Ngay CHECK (NgayHetHan IS NULL OR NgayHetHan >= NgayCap),
    CONSTRAINT [FK_tTheBanDoc_MaBD] FOREIGN KEY ([MaBD]) REFERENCES dbo.[tBanDoc] ([MaBD]),
    CONSTRAINT [FK_tTheBanDoc_MaTK] FOREIGN KEY ([MaTK]) REFERENCES dbo.[tTaiKhoan] ([MaTK])
);

CREATE TABLE dbo.[tGiaoDichMuonTra] (
    [MaGD] char(12) NOT NULL,
    [MaTBD] char(12) NOT NULL,
    [MaTK] char(7) NOT NULL,
    [NgayMuon] date NOT NULL,
    [NgayHenTra] date NOT NULL,
    [NgayTra] date NULL,
    [TrangThai] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_tGiaoDichMuonTra] PRIMARY KEY ([MaGD]),
    CONSTRAINT CK_tGiaoDichMuonTra_NgayHenTra CHECK (NgayHenTra >= NgayMuon),
    CONSTRAINT CK_tGiaoDichMuonTra_NgayTra CHECK (NgayTra IS NULL OR NgayTra >= NgayMuon),
    CONSTRAINT [FK_tGiaoDichMuonTra_MaTBD] FOREIGN KEY ([MaTBD]) REFERENCES dbo.[tTheBanDoc] ([MaTBD]),
    CONSTRAINT [FK_tGiaoDichMuonTra_MaTK] FOREIGN KEY ([MaTK]) REFERENCES dbo.[tTaiKhoan] ([MaTK])
);

CREATE TABLE dbo.[tGiaoDich_BanSao] (
    [MaGD] char(12) NOT NULL,
    [MaBS] char(14) NOT NULL,
    [TinhTrang] bit NOT NULL,
    CONSTRAINT [PK_tGiaoDich_BanSao] PRIMARY KEY ([MaGD], [MaBS]),
    CONSTRAINT [FK_tGiaoDich_BanSao_MaGD] FOREIGN KEY ([MaGD]) REFERENCES dbo.[tGiaoDichMuonTra] ([MaGD]),
    CONSTRAINT [FK_tGiaoDich_BanSao_MaBS] FOREIGN KEY ([MaBS]) REFERENCES dbo.[tBanSao] ([MaBS])
);

CREATE INDEX [IX_tTaiKhoan_MaVT] ON dbo.[tTaiKhoan] ([MaVT]);

CREATE INDEX [IX_tTacGia_MaQG] ON dbo.[tTacGia] ([MaQG]);

CREATE INDEX [IX_tNhaXuatBan_MaQG] ON dbo.[tNhaXuatBan] ([MaQG]);

CREATE INDEX [IX_tTaiLieu_MaNXB] ON dbo.[tTaiLieu] ([MaNXB]);

CREATE INDEX [IX_tTaiLieu_MaNN] ON dbo.[tTaiLieu] ([MaNN]);

CREATE INDEX [IX_tTaiLieu_MaThL] ON dbo.[tTaiLieu] ([MaThL]);

CREATE INDEX [IX_tTaiLieu_MaDD] ON dbo.[tTaiLieu] ([MaDD]);

CREATE INDEX [IX_tTaiLieu_MaTK] ON dbo.[tTaiLieu] ([MaTK]);

CREATE INDEX [IX_tTaiLieu_TacGia_MaTG] ON dbo.[tTaiLieu_TacGia] ([MaTG]);

CREATE INDEX [IX_tBanSao_MaTL] ON dbo.[tBanSao] ([MaTL]);

CREATE INDEX [IX_tTheBanDoc_MaTK] ON dbo.[tTheBanDoc] ([MaTK]);

CREATE INDEX [IX_tGiaoDichMuonTra_MaTBD] ON dbo.[tGiaoDichMuonTra] ([MaTBD]);

CREATE INDEX [IX_tGiaoDichMuonTra_MaTK] ON dbo.[tGiaoDichMuonTra] ([MaTK]);

CREATE INDEX [IX_tGiaoDich_BanSao_MaBS] ON dbo.[tGiaoDich_BanSao] ([MaBS]);

GO

-- DỮ LIỆU MẪU: chèn theo thứ tự bảng cha trước, bảng con sau.
BEGIN TRANSACTION;
INSERT INTO dbo.[tQuocGia] ([MaQG], [TenQG])
VALUES
    (N'VN', N'Việt Nam'),
    (N'GB', N'Vương quốc Anh'),
    (N'US', N'Hoa Kỳ');

INSERT INTO dbo.[tNgonNgu] ([MaNN], [TenNN])
VALUES
    (N'VI', N'Tiếng Việt'),
    (N'EN', N'Tiếng Anh'),
    (N'FR', N'Tiếng Pháp');

INSERT INTO dbo.[tTheLoai] ([MaThL], [TenThL])
VALUES
    (N'THL001', N'Công nghệ thông tin'),
    (N'THL002', N'Kỹ thuật xây dựng'),
    (N'THL003', N'Khoa học cơ bản');

INSERT INTO dbo.[tDinhDang] ([MaDD], [TenDD])
VALUES
    (N'DD001', N'Sách in'),
    (N'DD002', N'Tài liệu điện tử'),
    (N'DD003', N'Luận văn bản in');

INSERT INTO dbo.[tVaiTro] ([MaVT], [TenVT])
VALUES
    (N'QTV', N'Quản trị viên'),
    (N'QLB', N'Quản lý bạn đọc'),
    (N'TTM', N'Thủ thư mượn trả');

INSERT INTO dbo.[tNhanVien] ([MaNV], [HoDem], [Ten], [NgaySinh], [GioiTinh], [DiaChi], [SDT], [Email], [PhuTrach])
VALUES
    (N'NV26-01', N'Nguyễn Thị', N'Lan', N'1990-03-12', N'F', N'Hà Nội', N'0900000001', N'lan.demo@utc.edu.vn', N'Quản trị hệ thống'),
    (N'NV26-02', N'Trần Văn', N'Minh', N'1993-07-20', N'M', N'Hà Nội', N'0900000002', N'minh.demo@utc.edu.vn', N'Quản lý bạn đọc'),
    (N'NV26-03', N'Lê Thị', N'Hà', N'1995-11-05', N'F', N'Hà Nội', N'0900000003', N'ha.demo@utc.edu.vn', N'Mượn trả');

INSERT INTO dbo.[tTaiKhoan] ([MaTK], [MaNV], [MaVT], [TenDangNhap], [MatKhau], [TrangThai], [NgayTao])
VALUES
    (N'TK26-01', N'NV26-01', N'QTV', N'admin.demo@utc.edu.vn', N'AQAAAAIAAYagAAAAECbYiR4oilvO7XkbrwNVLKuFvqnXszY1AXPDxWTMZIl7tVDUfT0OfNPIx73ucfNx+A==', N'Hoạt động', N'2026-09-01'),
    (N'TK26-02', N'NV26-02', N'QLB', N'bandoc.demo@utc.edu.vn', N'AQAAAAIAAYagAAAAENv+VjREzXhyrs8Aix4wzoqXCwrToaikkV7aMU+0ufARHREt66pBQ7IghcEZRm12bg==', N'Hoạt động', N'2026-09-01'),
    (N'TK26-03', N'NV26-03', N'TTM', N'thuthu.demo@utc.edu.vn', N'AQAAAAIAAYagAAAAECQ2QQ29rsXJycKtwHY4vsq6xS7V+4uyv3Vi5mp78c+x1d3hmqycO4p4VoLYdiG3mA==', N'Hoạt động', N'2026-09-01');

INSERT INTO dbo.[tTacGia] ([MaTG], [MaQG], [HoDem], [Ten])
VALUES
    (N'TGVN26-001', N'VN', N'Nguyễn Văn', N'An'),
    (N'TGVN26-002', N'VN', N'Trần Thị', N'Bình'),
    (N'TGGB26-001', N'GB', N'John', N'Smith');

INSERT INTO dbo.[tNhaXuatBan] ([MaNXB], [MaQG], [TenNXB])
VALUES
    (N'NXBVN-001', N'VN', N'Nhà xuất bản Giáo dục Việt Nam'),
    (N'NXBVN-002', N'VN', N'Nhà xuất bản Xây dựng'),
    (N'NXBGB-001', N'GB', N'Academic Press Demo');

INSERT INTO dbo.[tTaiLieu] ([MaTL], [MaNXB], [MaNN], [MaThL], [MaDD], [MaTK], [TenTL], [LanXuatBan], [NamXuatBan], [SoTrang], [KhoCo], [Anh])
VALUES
    (N'TLVN26-001', N'NXBVN-001', N'VI', N'THL001', N'DD001', N'TK26-01', N'Nhập môn lập trình Python', 1, 2024, 240, N'A5', N'python.jpg'),
    (N'TLVN26-002', N'NXBVN-002', N'VI', N'THL002', N'DD001', N'TK26-01', N'Cơ học kết cấu', 2, 2023, 320, N'A4', N'co_hoc.jpg'),
    (N'TLGB26-001', N'NXBGB-001', N'EN', N'THL001', N'DD001', N'TK26-01', N'Introduction to Data Science', 1, 2025, 280, N'A5', N'data_science.jpg'),
    (N'TLVN26-003', N'NXBVN-001', N'VI', N'THL003', N'DD002', N'TK26-01', N'Xác suất thống kê', 1, 2024, 200, N'A4', NULL);

INSERT INTO dbo.[tTaiLieu_TacGia] ([MaTL], [MaTG], [VaiTro])
VALUES
    (N'TLVN26-001', N'TGVN26-001', N'Chủ biên'),
    (N'TLVN26-001', N'TGVN26-002', N'Đồng tác giả'),
    (N'TLVN26-002', N'TGVN26-002', N'Tác giả'),
    (N'TLGB26-001', N'TGGB26-001', N'Tác giả'),
    (N'TLVN26-003', N'TGVN26-001', N'Tác giả');

INSERT INTO dbo.[tBanSao] ([MaBS], [MaTL], [TrangThai])
VALUES
    (N'TLVN26-001-001', N'TLVN26-001', N'Đang mượn'),
    (N'TLVN26-001-002', N'TLVN26-001', N'Có sẵn'),
    (N'TLVN26-002-001', N'TLVN26-002', N'Đang mượn'),
    (N'TLVN26-002-002', N'TLVN26-002', N'Có sẵn'),
    (N'TLGB26-001-001', N'TLGB26-001', N'Có sẵn'),
    (N'TLGB26-001-002', N'TLGB26-001', N'Có sẵn');

INSERT INTO dbo.[tBanDoc] ([MaBD], [HoDem], [Ten], [NgaySinh], [GioiTinh], [DiaChi], [SDT], [Email], [GhiChuViPham])
VALUES
    (N'231230001', N'Nguyễn Văn', N'Huy', N'2005-05-12', N'M', N'Hà Nội', N'0910000001', N'sv001@example.com', N''),
    (N'231230002', N'Trần Thị', N'Mai', N'2005-08-21', N'F', N'Hà Nội', N'0910000002', N'sv002@example.com', N''),
    (N'231230003', N'Lê Văn', N'Nam', N'2004-12-02', N'M', N'Hà Nội', N'0910000003', N'sv003@example.com', N'');

INSERT INTO dbo.[tTheBanDoc] ([MaTBD], [MaBD], [MaTK], [NgayCap], [NgayHetHan], [TrangThai])
VALUES
    (N'TBD231230001', N'231230001', N'TK26-02', N'2026-09-01', N'2029-09-30', N'Hoạt động'),
    (N'TBD231230002', N'231230002', N'TK26-02', N'2026-09-01', N'2029-09-30', N'Hoạt động'),
    (N'TBD231230003', N'231230003', N'TK26-02', N'2026-09-01', N'2029-09-30', N'Hoạt động');

INSERT INTO dbo.[tGiaoDichMuonTra] ([MaGD], [MaTBD], [MaTK], [NgayMuon], [NgayHenTra], [NgayTra], [TrangThai])
VALUES
    (N'GD2610270001', N'TBD231230001', N'TK26-03', N'2026-10-27', N'2026-11-10', NULL, N'Đang mượn'),
    (N'GD2610010001', N'TBD231230002', N'TK26-03', N'2026-10-01', N'2026-10-15', N'2026-10-14', N'Đã trả'),
    (N'GD2609010001', N'TBD231230003', N'TK26-03', N'2026-09-01', N'2026-09-15', N'2026-09-16', N'Đã trả');

INSERT INTO dbo.[tGiaoDich_BanSao] ([MaGD], [MaBS], [TinhTrang])
VALUES
    (N'GD2610270001', N'TLVN26-001-001', 1),
    (N'GD2610270001', N'TLVN26-002-001', 1),
    (N'GD2610010001', N'TLVN26-001-002', 1),
    (N'GD2610010001', N'TLGB26-001-001', 1),
    (N'GD2609010001', N'TLVN26-002-002', 1);

COMMIT TRANSACTION;
GO

-- Kiểm tra số bản ghi từng bảng.
SELECT N'tQuocGia' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tQuocGia]
UNION ALL
SELECT N'tNgonNgu' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tNgonNgu]
UNION ALL
SELECT N'tTheLoai' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tTheLoai]
UNION ALL
SELECT N'tDinhDang' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tDinhDang]
UNION ALL
SELECT N'tVaiTro' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tVaiTro]
UNION ALL
SELECT N'tNhanVien' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tNhanVien]
UNION ALL
SELECT N'tTaiKhoan' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tTaiKhoan]
UNION ALL
SELECT N'tTacGia' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tTacGia]
UNION ALL
SELECT N'tNhaXuatBan' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tNhaXuatBan]
UNION ALL
SELECT N'tTaiLieu' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tTaiLieu]
UNION ALL
SELECT N'tTaiLieu_TacGia' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tTaiLieu_TacGia]
UNION ALL
SELECT N'tBanSao' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tBanSao]
UNION ALL
SELECT N'tBanDoc' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tBanDoc]
UNION ALL
SELECT N'tTheBanDoc' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tTheBanDoc]
UNION ALL
SELECT N'tGiaoDichMuonTra' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tGiaoDichMuonTra]
UNION ALL
SELECT N'tGiaoDich_BanSao' AS TenBang, COUNT(*) AS SoBanGhi FROM dbo.[tGiaoDich_BanSao];
GO
