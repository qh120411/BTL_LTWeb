USE [master];
GO

IF DB_ID(N'QLThuVienUTC') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [QLThuVienUTC] COLLATE SQL_Latin1_General_CP1_CI_AS;');
END;
GO

USE [QLThuVienUTC];
GO

SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    THROW 50001, N'Cơ sở dữ liệu đã có bảng. Chỉ chạy trên database mới.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    -- 01. TheLoai
    CREATE TABLE [dbo].[TheLoai]
    (
        [MaTheLoai] INT IDENTITY(1,1) NOT NULL,
        [TenTheLoai] NVARCHAR(100) NOT NULL,
        [MaTheLoaiCha] INT NULL,
        CONSTRAINT [PK_TheLoai] PRIMARY KEY CLUSTERED ([MaTheLoai])
    );

    SET IDENTITY_INSERT [dbo].[TheLoai] ON;

    INSERT INTO [dbo].[TheLoai] ([MaTheLoai], [TenTheLoai], [MaTheLoaiCha])
    VALUES
        (1, N'Công nghệ thông tin', NULL),
        (2, N'Khoa học cơ bản & Toán', NULL),
        (3, N'Kỹ thuật công trình & Giao thông', NULL),
        (4, N'Kinh tế & Logistics', NULL),
        (5, N'Lý luận chính trị & Pháp luật', NULL);

    SET IDENTITY_INSERT [dbo].[TheLoai] OFF;

    -- 02. TacGia
    CREATE TABLE [dbo].[TacGia]
    (
        [MaTacGia] INT IDENTITY(1,1) NOT NULL,
        [HoTen] NVARCHAR(100) NOT NULL,
        [QuocTich] NVARCHAR(100) NULL,
        CONSTRAINT [PK_TacGia] PRIMARY KEY CLUSTERED ([MaTacGia])
    );

    SET IDENTITY_INSERT [dbo].[TacGia] ON;

    INSERT INTO [dbo].[TacGia] ([MaTacGia], [HoTen], [QuocTich])
    VALUES
        (1, N'Bùi Việt Hà', N'Việt Nam'),
        (2, N'Phạm Minh Chuẩn', N'Việt Nam'),
        (3, N'Trịnh Thị Nhị', N'Việt Nam'),
        (4, N'Nguyễn Văn Quyết', N'Việt Nam'),
        (5, N'Nguyễn Thế Lộc', N'Việt Nam'),
        (6, N'Nguyễn Hồng Phương', N'Việt Nam'),
        (7, N'Học viện Công nghệ Bưu chính Viễn thông', N'Việt Nam'),
        (8, N'Phạm Văn Ất', N'Việt Nam'),
        (9, N'Nguyễn Hiếu Cường', N'Việt Nam'),
        (10, N'Lê Trường Thông', N'Việt Nam'),
        (11, N'Nguyễn Tuấn Đăng', N'Việt Nam'),
        (12, N'Phạm Văn Cường', N'Việt Nam'),
        (13, N'Đoàn Văn Ban', N'Việt Nam'),
        (14, N'Đậu Hải Long', N'Việt Nam'),
        (15, N'Nguyễn Đình Trí', N'Việt Nam'),
        (16, N'Trần Việt Dũng', N'Việt Nam'),
        (17, N'Trần Xuân Hiển', N'Việt Nam'),
        (18, N'Tống Đình Quỳ', N'Việt Nam'),
        (19, N'Lương Duyên Bình', N'Việt Nam'),
        (20, N'Nguyễn Văn Vượng', N'Việt Nam'),
        (21, N'Lê Ngọc Hồng', N'Việt Nam'),
        (22, N'Đỗ Sanh', N'Việt Nam'),
        (23, N'Vũ Công Ngữ', N'Việt Nam'),
        (24, N'Nguyễn Văn Quảng', N'Việt Nam'),
        (25, N'Dương Học Hải', N'Việt Nam'),
        (26, N'Nguyễn Quang Chiêu', N'Việt Nam'),
        (27, N'Nguyễn Đình Cống', N'Việt Nam'),
        (28, N'Bùi Hải', N'Việt Nam'),
        (29, N'Trần Thế Sơn', N'Việt Nam'),
        (30, N'Lê Cung', N'Việt Nam'),
        (31, N'Michael H. Hugos', N'Việt Nam'),
        (32, N'Đặng Đình Đào', N'Việt Nam'),
        (33, N'Vũ Trọng Phong', N'Việt Nam'),
        (34, N'Trần Thị Hoa', N'Việt Nam'),
        (35, N'Nguyễn Văn Công', N'Việt Nam'),
        (36, N'Nguyễn Thị Loan', N'Việt Nam'),
        (37, N'Bộ Giáo Dục Và Đào Tạo', N'Việt Nam'),
        (38, N'NCS. Trần Hoài Nam', N'Việt Nam'),
        (39, N'NCS. Lê Quốc Tuấn', N'Việt Nam'),
        (40, N'ThS. Vũ Đức Thịnh', N'Việt Nam'),
        (41, N'NCS. Phạm Văn Dũng', N'Việt Nam'),
        (42, N'NCS. Nguyễn Thị Mai Lan', N'Việt Nam'),
        (43, N'ThS. Đỗ Tuấn Anh', N'Việt Nam'),
        (44, N'ThS. Hoàng Minh Đức', N'Việt Nam'),
        (45, N'NCS. Nguyễn Mạnh Cường', N'Việt Nam'),
        (46, N'NCS. Bùi Văn Hưng', N'Việt Nam'),
        (47, N'ThS. Phạm Quang Huy', N'Việt Nam'),
        (48, N'ThS. Trịnh Văn Hải', N'Việt Nam'),
        (49, N'NCS. Chu Kim Oanh', N'Việt Nam'),
        (50, N'ThS. Lương Thu Thảo', N'Việt Nam'),
        (51, N'NCS. Đặng Xuân Bách', N'Việt Nam'),
        (52, N'ThS. Nguyễn Viết Sơn', N'Việt Nam'),
        (53, N'ThS. Tạ Ngọc Thành', N'Việt Nam'),
        (54, N'NCS. Phan Bích Ngọc', N'Việt Nam'),
        (55, N'ThS. Hồ Đình Cương', N'Việt Nam'),
        (56, N'NCS. Mai Văn Tuấn', N'Việt Nam'),
        (57, N'ThS. Đoàn Quốc Bảo', N'Việt Nam'),
        (58, N'ThS. Nguyễn Hoàng Long', N'Việt Nam'),
        (59, N'NCS. Vũ Thị Thoa', N'Việt Nam'),
        (60, N'ThS. Nguyễn Hữu Trọng', N'Việt Nam'),
        (61, N'ThS. Trần Phương Thảo', N'Việt Nam'),
        (62, N'NCS. Lê Văn Khánh', N'Việt Nam'),
        (63, N'ThS. Đỗ Thế Anh', N'Việt Nam'),
        (64, N'ThS. Nguyễn Minh Khang', N'Việt Nam'),
        (65, N'NCS. Phạm Thị Hồng Nhung', N'Việt Nam'),
        (66, N'ThS. Nguyễn Đức Thành', N'Việt Nam'),
        (67, N'NCS. Bùi Đình Quý', N'Việt Nam'),
        (68, N'Hội đồng Biên tập Tạp chí JST-UTC', N'Quá»‘c táº¿'),
        (69, N'Bộ Giao thông Vận tải', N'Việt Nam'),
        (70, N'Bộ Xây Dựng', N'Việt Nam'),
        (71, N'Hội Cầu đường Việt Nam', N'Việt Nam'),
        (72, N'Bộ Kế hoạch và Đầu tư', N'Việt Nam'),
        (73, N'Bộ Khoa học và Công nghệ', N'Việt Nam'),
        (74, N'Viện Hàn lâm KH&CN Việt Nam', N'Việt Nam'),
        (75, N'Tổng hội Cơ khí Việt Nam', N'Việt Nam'),
        (76, N'Hội Tự động hóa Việt Nam', N'Việt Nam'),
        (77, N'Viện Khoa học Địa chất', N'Việt Nam'),
        (78, N'Viện Nghiên cứu Quản lý Kinh tế TW', N'Việt Nam'),
        (79, N'Hiệp hội Năng lượng Việt Nam', N'Việt Nam'),
        (80, N'UTC Editorial Board', N'Quá»‘c táº¿'),
        (81, N'Tổng cục Tiêu chuẩn Đo lường', N'Việt Nam'),
        (82, N'Hiệp hội Môi trường Đô thị', N'Việt Nam'),
        (83, N'Ban Cơ yếu Chính phủ', N'Việt Nam'),
        (84, N'Cục Hàng hải Việt Nam', N'Việt Nam'),
        (85, N'Tổng công ty Đường sắt Việt Nam', N'Việt Nam'),
        (86, N'Viện KHCN Giao thông Vận tải', N'Việt Nam');

    SET IDENTITY_INSERT [dbo].[TacGia] OFF;

    -- 03. NhaXuatBan
    CREATE TABLE [dbo].[NhaXuatBan]
    (
        [MaNhaXuatBan] INT IDENTITY(1,1) NOT NULL,
        [TenNhaXuatBan] NVARCHAR(150) NOT NULL,
        [DiaChi] NVARCHAR(255) NULL,
        [SoDienThoai] VARCHAR(20) NULL,
        [QuocGia] NVARCHAR(100) NULL,
        CONSTRAINT [PK_NhaXuatBan] PRIMARY KEY CLUSTERED ([MaNhaXuatBan])
    );

    SET IDENTITY_INSERT [dbo].[NhaXuatBan] ON;

    INSERT INTO [dbo].[NhaXuatBan] ([MaNhaXuatBan], [TenNhaXuatBan], [DiaChi], [SoDienThoai], [QuocGia])
    VALUES
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
        (11, N'NXB Đại Học Sư Phạm', NULL, NULL, N'Việt Nam'),
        (12, N'Trường Đại học Giao thông Vận tải', NULL, NULL, N'Việt Nam'),
        (13, N'Tạp chí Khoa học GTVT (JST-UTC)', NULL, NULL, N'Việt Nam'),
        (14, N'Bộ Giao thông Vận tải', NULL, NULL, N'Việt Nam'),
        (15, N'Hội Cầu đường Việt Nam', NULL, NULL, N'Việt Nam'),
        (16, N'Bộ Khoa học và Công nghệ', NULL, NULL, N'Việt Nam');

    SET IDENTITY_INSERT [dbo].[NhaXuatBan] OFF;

    -- 04. NhanVien
    CREATE TABLE [dbo].[NhanVien]
    (
        [MaNhanVien] INT IDENTITY(1,1) NOT NULL,
        [MaSoNhanVien] VARCHAR(20) NOT NULL,
        [HoTen] NVARCHAR(100) NOT NULL,
        [ChucVu] NVARCHAR(50) NULL,
        [Email] VARCHAR(150) NULL,
        [SoDienThoai] VARCHAR(20) NULL,
        [GioiTinh] NVARCHAR(10) NULL,
        [TrangThai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_NhanVien_TrangThai] DEFAULT (N'Hoạt động'),
        [NgaySinh] DATE NULL,
        CONSTRAINT [PK_NhanVien] PRIMARY KEY CLUSTERED ([MaNhanVien]),
        CONSTRAINT [UQ_NhanVien_MaSoNhanVien] UNIQUE NONCLUSTERED ([MaSoNhanVien])
    );

    SET IDENTITY_INSERT [dbo].[NhanVien] ON;

    INSERT INTO [dbo].[NhanVien] ([MaNhanVien], [MaSoNhanVien], [HoTen], [ChucVu], [Email], [SoDienThoai], [GioiTinh], [TrangThai], [NgaySinh])
    VALUES
        (1, N'NV001', N'Trần Văn Quản Trị', N'Quản trị viên hệ thống', NULL, NULL, NULL, N'Đang làm việc', NULL),
        (2, N'NV002', N'Lê Thị Thủ Thư', N'Cán bộ nghiệp vụ lưu thông', NULL, NULL, NULL, N'Đang làm việc', NULL),
        (3, N'NV26-01', N'Nguyễn Thị Lan', N'Quản trị viên hệ thống', N'lan.demo@utc.edu.vn', NULL, NULL, N'Đang làm việc', NULL),
        (4, N'NV26-02', N'Trần Văn Minh', N'Cán bộ quản lý bạn đọc', N'bandoc.demo@utc.edu.vn', NULL, NULL, N'Đang làm việc', NULL),
        (5, N'NV26-03', N'Lê Thị Hà', N'Cán bộ quản lý học liệu', N'thuthu.demo@utc.edu.vn', NULL, NULL, N'Đang làm việc', NULL);

    SET IDENTITY_INSERT [dbo].[NhanVien] OFF;

    -- 05. Lop
    CREATE TABLE [dbo].[Lop]
    (
        [MaLop] VARCHAR(20) NOT NULL,
        [TenLop] NVARCHAR(100) NOT NULL,
        [KhoaHoc] NVARCHAR(30) NULL,
        [GhiChu] NVARCHAR(255) NULL,
        CONSTRAINT [PK_Lop] PRIMARY KEY CLUSTERED ([MaLop])
    );

    INSERT INTO [dbo].[Lop] ([MaLop], [TenLop], [KhoaHoc], [GhiChu])
    VALUES
        (N'K65.CLCCNTT1', N'K65 Chất lượng cao Công nghệ thông tin 1', N'K65', N'Lớp lấy từ danh sách thi người dùng cung cấp'),
        (N'KHOA.CK', N'Khoa Cơ khí', N'Cán bộ / Giảng viên', N'Khoa Cơ khí - ĐH Giao thông Vận tải'),
        (N'KHOA.CNTT', N'Khoa Công nghệ thông tin', N'Cán bộ / Giảng viên', N'Khoa Công nghệ thông tin - ĐH Giao thông Vận tải'),
        (N'KHOA.CT', N'Khoa Công trình', N'Cán bộ / Giảng viên', N'Khoa Công trình - ĐH Giao thông Vận tải'),
        (N'KHOA.DDT', N'Khoa Điện - Điện tử', N'Cán bộ / Giảng viên', N'Khoa Điện - Điện tử - ĐH Giao thông Vận tải'),
        (N'KHOA.KHCB', N'Khoa Khoa học Cơ bản', N'Cán bộ / Giảng viên', N'Khoa Khoa học Cơ bản - ĐH Giao thông Vận tải'),
        (N'KHOA.VTKT', N'Khoa Vận tải - Kinh tế', N'Cán bộ / Giảng viên', N'Khoa Vận tải - Kinh tế - ĐH Giao thông Vận tải');

    -- 06. BanDoc
    CREATE TABLE [dbo].[BanDoc]
    (
        [SoCCCD] NVARCHAR(20) NULL,
        [NganhHoc] NVARCHAR(150) NULL,
        [SoTaiKhoanNganHang] NVARCHAR(50) NULL,
        [TenNganHang] NVARCHAR(150) NULL,
        [AvatarUrl] NVARCHAR(255) NULL,
        [MaBanDoc] INT IDENTITY(1,1) NOT NULL,
        [MaSinhVien] VARCHAR(15) NOT NULL,
        [HoTen] NVARCHAR(100) NOT NULL,
        [NgaySinh] DATE NULL,
        [GioiTinh] NVARCHAR(10) NULL,
        [DiaChi] NVARCHAR(255) NULL,
        [SoDienThoai] VARCHAR(20) NULL,
        [Email] VARCHAR(150) NULL,
        [MaLop] VARCHAR(20) NULL,
        [TrangThai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_BanDoc_TrangThai] DEFAULT (N'Hoạt động'),
        [NgayTao] DATETIME2(7) NOT NULL CONSTRAINT [DF_BanDoc_NgayTao] DEFAULT (SYSDATETIME()),
        [SoThe] NVARCHAR(50) NULL,
        [NgayCapThe] DATE NULL,
        [NgayHetHanThe] DATE NULL,
        [TrangThaiThe] NVARCHAR(60) NULL,
        CONSTRAINT [PK_BanDoc] PRIMARY KEY CLUSTERED ([MaBanDoc]),
        CONSTRAINT [UQ_BanDoc_MaSinhVien] UNIQUE NONCLUSTERED ([MaSinhVien])
    );

    CREATE NONCLUSTERED INDEX [IX_BanDoc_HoTen]
        ON [dbo].[BanDoc] ([HoTen]);

    CREATE NONCLUSTERED INDEX [IX_BanDoc_MaLop]
        ON [dbo].[BanDoc] ([MaLop]);

    SET IDENTITY_INSERT [dbo].[BanDoc] ON;

    INSERT INTO [dbo].[BanDoc] ([MaBanDoc], [MaSinhVien], [HoTen], [NgaySinh], [GioiTinh], [DiaChi], [SoDienThoai], [Email], [MaLop], [TrangThai], [NgayTao], [SoThe], [NgayCapThe], [NgayHetHanThe], [TrangThaiThe])
    VALUES
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
        (40, N'242624501', N'Trần Đức Tài', NULL, NULL, NULL, NULL, NULL, N'K65.CLCCNTT1', N'Hoạt động', '2026-11-06 01:43:52.545', N'THE-UTC-242624501', '2024-09-01', '2028-06-30', N'Hoạt động'),
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
        (69, N'GV20', N'Lâm Hoài Nam', '1973-05-16', N'Nam', N'Số 136 Xuân Thủy, Dịch Vọng Hậu, Cầu Giấy, Hà Nội', N'0912111020', N'nam.lh2@utc.edu.vn', N'KHOA.KHCB', N'Hoạt động', '2026-11-06 17:02:39.356', N'THE-GV20', '2024-01-15', '2028-12-31', N'Hoạt động'),
        (68, N'GV19', N'GV19', NULL, NULL, NULL, NULL, N'nhung.dh@utc.edu.vn', NULL, N'Hoạt động', CAST(N'2026-11-11T00:00:00' AS DATETIME2), NULL, NULL, NULL, NULL);

    SET IDENTITY_INSERT [dbo].[BanDoc] OFF;

    -- 07. TaiLieu
    CREATE TABLE [dbo].[TaiLieu]
    (
        [MaTaiLieu] INT IDENTITY(1,1) NOT NULL,
        [ISBN] VARCHAR(20) NULL,
        [TenTaiLieu] NVARCHAR(255) NOT NULL,
        [MaNhaXuatBan] INT NULL,
        [MaTheLoai] INT NULL,
        [NamXuatBan] SMALLINT NULL,
        [LanXuatBan] INT NOT NULL CONSTRAINT [DF_TaiLieu_LanXuatBan] DEFAULT ((1)),
        [SoTrang] INT NULL,
        [MoTa] NVARCHAR(MAX) NULL,
        [AnhBia] NVARCHAR(255) NULL,
        [NgonNgu] NVARCHAR(100) NULL,
        [DinhDang] NVARCHAR(100) NULL,
        [NgayTao] DATETIME2(7) NOT NULL CONSTRAINT [DF_TaiLieu_NgayTao] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_TaiLieu] PRIMARY KEY CLUSTERED ([MaTaiLieu])
    );

    CREATE NONCLUSTERED INDEX [IX_TaiLieu_Ten]
        ON [dbo].[TaiLieu] ([TenTaiLieu]);

    SET IDENTITY_INSERT [dbo].[TaiLieu] ON;

    INSERT INTO [dbo].[TaiLieu] ([MaTaiLieu], [ISBN], [TenTaiLieu], [MaNhaXuatBan], [MaTheLoai], [NamXuatBan], [LanXuatBan], [SoTrang], [MoTa], [AnhBia], [NgonNgu], [DinhDang], [NgayTao])
    VALUES
        (1, N'978-604-0-32451-2', N'Cấu trúc dữ liệu và giải thuật (Python: Thuật toán & Cấu trúc dữ liệu)', 10, 1, 2023, 1, 506, N'Giáo trình trang bị nền tảng chuyên sâu về thuật toán và các cấu trúc dữ liệu kinh điển: mảng động, danh sách liên kết, ngăn xếp (Stack), hàng đợi (Queue), cây nhị phân, bảng băm (Hash Table) và đồ thị, ứng dụng trực tiếp bằng ngôn ngữ hiện đại.', N'/images/covers/sach_01.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (2, N'978-604-67-2103-1', N'Giáo trình Cơ sở dữ liệu quan hệ - Lý thuyết và Thực hành', 5, 1, 2022, 1, 328, N'Trình bày có hệ thống mô hình thực thể liên kết (ERD), đại số quan hệ, phụ thuộc hàm, các dạng chuẩn chuẩn hóa dữ liệu (1NF, 2NF, 3NF, BCNF) cùng kỹ thuật thiết kế và truy vấn SQL tối ưu trên hệ quản trị CSDL.', N'/images/covers/sach_02.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (3, N'978-604-54-8921-0', N'Giáo trình Mạng máy tính', 11, 1, 2022, 1, 272, N'Tài liệu giảng dạy chuẩn mực về kiến trúc mạng phân tầng OSI và TCP/IP, các giao thức tầng ứng dụng (HTTP, DNS), giao vận (TCP, UDP), mạng (IP, định tuyến OSPF/BGP) và kỹ thuật an toàn thông tin cơ bản trong môi trường truyền số liệu.', N'/images/covers/sach_03.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (4, N'978-604-80-4512-8', N'Giáo trình Cơ sở An toàn thông tin', 7, 1, 2021, 1, 350, N'Giáo trình cốt lõi trang bị các nguyên lý bảo mật hệ thống: mã hóa đối xứng (AES, DES), mã hóa bất đối xứng (RSA, ECC), chữ ký số, hàm băm mật mã (SHA), xác thực người dùng và các chính sách phòng chống tấn công mạng.', N'/images/covers/sach_04.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (5, N'978-604-67-1950-2', N'Giáo trình Kỹ thuật lập trình C căn bản và nâng cao', 3, 1, 2021, 1, 440, N'Tác phẩm kinh điển của GS. Phạm Văn Ất hướng dẫn kỹ thuật lập trình từ cơ bản đến nâng cao: cấu trúc điều khiển, mảng, con trỏ (pointer), cấp phát động bộ nhớ, xử lý tệp tin và các kỹ thuật giải thuật tối ưu trên ngôn ngữ C.', N'/images/covers/sach_05.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (6, N'978-604-67-1845-1', N'Giáo trình C++ và Lập trình hướng đối tượng', 5, 1, 2022, 1, 488, N'Cung cấp nền tảng vững chắc về tư duy hướng đối tượng (OOP): lớp và đối tượng (Class & Object), tính đóng gói, kế thừa, đa hình, hàm bạn, nạp chồng toán tử (operator overloading), khuôn hình (Template) và thư viện chuẩn STL.', N'/images/covers/sach_06.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (7, N'978-604-67-2415-5', N'Giáo trình Trí tuệ nhân tạo, Học máy và Học sâu', 5, 1, 2023, 1, 380, N'Tài liệu học thuật hiện đại trình bày các phương pháp tìm kiếm không gian trạng thái, thuật toán học máy có giám sát và không giám sát (SVM, Random Forest, K-Means), và mạng nơ-ron tích chập (CNN, RNN) trong xử lý hình ảnh và ngôn ngữ tự nhiên.', N'/images/covers/sach_07.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (8, N'978-604-938-634-3', N'Giáo trình Kiến trúc máy tính và Hợp ngữ', 1, 1, 2022, 1, 360, N'Nghiên cứu cấu trúc phần cứng máy tính: tập lệnh kiến trúc x86/ARM, ALU, đơn vị điều khiển (CU), hệ thống nhớ phân cấp (Cache, RAM), đường ống xử lý lệnh (Pipelining) và kỹ thuật lập trình hợp ngữ Assembly cơ bản.', N'/images/covers/sach_08.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (9, N'978-604-0-28731-2', N'Giáo trình Kỹ nghệ phần mềm (Công nghệ phần mềm)', 4, 1, 2021, 1, 310, N'Trang bị quy trình phát triển phần mềm chuyên nghiệp: phân tích yêu cầu (SRS), mô hình hóa UML, thiết kế kiến trúc hệ thống, quy trình Agile/Scrum, kiểm thử tự động (Unit Test, Integration Test) và quản trị dự án phần mềm.', N'/images/covers/sach_09.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (10, N'978-604-80-5120-4', N'Giáo trình Lập trình Python cơ bản và ứng dụng', 7, 1, 2023, 1, 320, N'Hướng dẫn thực hành ngôn ngữ Python từ cú pháp cơ bản, xử lý ngoại lệ, thao tác file, đến ứng dụng phân tích dữ liệu kỹ thuật với thư viện NumPy, Pandas, Matplotlib và phát triển web với Django/Flask.', N'/images/covers/sach_10.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (11, N'978-604-0-34120-5', N'Toán học cao cấp - Tập 1: Đại số và Hình học giải tích', 4, 2, 2024, 1, 296, N'Bộ giáo trình chuẩn quốc gia dành cho sinh viên kỹ thuật: lý thuyết định thức, ma trận, hệ phương trình đại số tuyến tính, không gian véc tơ, ánh xạ tuyến tính, dạng toàn phương và hình học giải tích trong không gian.', N'/images/covers/sach_11.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (12, N'978-604-0-34121-2', N'Toán học cao cấp - Tập 2: Giải tích một biến và nhiều biến', 4, 2, 2024, 1, 384, N'Nội dung giải tích nền tảng: giới hạn và liên tục của hàm số, phép tính vi phân hàm một biến và nhiều biến, tích phân bất định, tích phân xác định và ứng dụng hình học, vật lý trong kỹ thuật.', N'/images/covers/sach_12.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (13, N'978-604-0-34122-9', N'Toán học cao cấp - Tập 3: Chuỗi và Phương trình vi phân', 4, 2, 2024, 1, 340, N'Nghiên cứu chuỗi số, chuỗi hàm, chuỗi lũy thừa và chuỗi Fourier; phương trình vi phân cấp 1, phương trình vi phân tuyến tính cấp 2 hệ số hằng và hệ phương trình vi phân ứng dụng mô hình hóa dao động cơ học.', N'/images/covers/sach_13.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (14, N'978-604-938-890-3', N'Giáo trình Xác suất và Thống kê toán học', 1, 2, 2023, 1, 316, N'Cung cấp lý thuyết biến ngẫu nhiên rời rạc và liên tục, các quy luật phân phối chuẩn, Poisson, nhị thức; lý thuyết ước lượng tham số, kiểm định giả thuyết thống kê, phân tích phương sai và hồi quy tương quan.', N'/images/covers/sach_14.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (15, N'978-604-0-29105-0', N'Vật lý đại cương - Tập 1: Cơ học và Nhiệt học', 4, 2, 2023, 1, 280, N'Giáo trình vật lý đại cương của GS. Lương Duyên Bình: động học chất điểm, động lực học chất điểm và vật rắn, các định luật bảo toàn (cơ năng, động lượng), thuyết động học chất khí và hai nguyên lý nhiệt động lực học.', N'/images/covers/sach_15.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (16, N'978-604-0-29106-7', N'Vật lý đại cương - Tập 2: Điện học và Từ học', 4, 2, 2023, 1, 320, N'Trình bày trường tĩnh điện, định luật Gauss, điện thế, vật dẫn trong điện trường, từ trường của dòng điện không đổi, định luật Ampe, hiện tượng cảm ứng điện từ và hệ phương trình Maxwell về trường điện từ.', N'/images/covers/sach_16.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (17, N'978-604-67-1520-7', N'Giáo trình Sức bền vật liệu - Tập 1', 3, 3, 2022, 1, 364, N'Học phần then chốt của kỹ sư xây dựng, giao thông và cơ khí: trạng thái ứng suất và biến dạng, kéo nén đúng tâm, xoắn thuần túy thanh tiết diện tròn, uốn phẳng của dầm và các thuyết bền phá hủy vật liệu.', N'/images/covers/sach_17.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (18, N'978-604-0-31204-5', N'Cơ học kỹ thuật - Tập 1: Tĩnh học và Động học', 4, 3, 2023, 1, 312, N'Cung cấp các tiên đề tĩnh học, thu gọn hệ lực không gian, điều kiện cân bằng của hệ lực phẳng và không gian, ma sát; động học điểm và động học vật rắn, chuyển động phức tạp của điểm và cơ cấu truyền động.', N'/images/covers/sach_18.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (19, N'978-604-82-4102-1', N'Giáo trình Nền và Móng công trình', 9, 3, 2021, 1, 340, N'Hướng dẫn tính toán và thiết kế móng nông, móng cọc đài thấp, móng cọc đài cao chịu tải trọng lệch tâm, tính toán độ lún nền đất, sức chịu tải giới hạn của đất nền và các giải pháp gia cố nền đất yếu.', N'/images/covers/sach_19.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (20, N'978-604-67-1755-3', N'Giáo trình Thiết kế hình học đường ô tô và đường cao tốc', 3, 3, 2022, 1, 420, N'Tài liệu đào tạo chuyên sâu ngành Cầu đường UTC: nguyên lý vạch tuyến đường, thiết kế bình đồ, trắc dọc, trắc ngang, đường cong chuyển tiếp, tầm nhìn xe chạy và bố trí các nút giao thông giao cắt lập thể.', N'/images/covers/sach_20.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (21, N'978-604-82-5301-7', N'Kết cấu bê tông cốt thép - Nguyên lý thiết kế', 9, 3, 2023, 1, 450, N'Trang bị phương pháp tính toán cấu kiện bê tông cốt thép theo trạng thái giới hạn (TCVN 5574): tính toán chịu uốn trên tiết diện thẳng góc và nghiêng, chịu nén đúng tâm và lệch tâm, độ võng và bề rộng vết nứt.', N'/images/covers/sach_21.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (22, N'978-604-0-30514-6', N'Giáo trình Cơ sở Kỹ thuật nhiệt', 4, 3, 2022, 1, 368, N'Nghiên cứu quá trình nhiệt động của khí lý tưởng, chu trình nhiệt động của động cơ đốt trong, turbine khí và máy lạnh; lý thuyết truyền nhiệt: dẫn nhiệt ổn định, trao đổi nhiệt đối lưu và bức xạ nhiệt công nghiệp.', N'/images/covers/sach_22.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (23, N'978-604-67-2015-8', N'Giáo trình Nguyên lý máy', 5, 3, 2022, 1, 350, N'Cấu tạo và xếp loại cơ cấu, phân tích động học và lực học cơ cấu phẳng bằng phương pháp đồ họa và giải tích, ma sát trong khớp động, cân bằng cơ cấu và động lực học máy.', N'/images/covers/sach_23.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (24, N'978-604-59-9812-4', N'Nguyên lý Quản trị chuỗi cung ứng và Logistics', 6, 4, 2023, 1, 396, N'Cẩm nang toàn diện về 5 khâu cốt lõi của chuỗi cung ứng: Sản xuất, Tồn kho, Địa điểm, Vận tải và Thông tin. Phân tích tối ưu hóa logistics cảng biển, kho bãi và hệ thống phân phối đa phương thức.', N'/images/covers/sach_24.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (25, N'978-604-99-4215-0', N'Giáo trình Kinh tế vi mô', 10, 4, 2023, 1, 345, N'Trang bị lý thuyết kinh tế học cơ sở: cung - cầu và độ co giãn, lý thuyết hành vi người tiêu dùng, tối ưu hóa sản xuất và chi phí doanh nghiệp, cấu trúc thị trường cạnh tranh hoàn hảo, độc quyền và cạnh tranh độc quyền.', N'/images/covers/sach_25.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (26, N'978-604-99-4216-7', N'Giáo trình Kinh tế vĩ mô', 10, 4, 2023, 1, 380, N'Nghiên cứu hoạt động nền kinh tế tổng thể: đo lường sản lượng quốc gia (GDP/GNP), lạm phát, thất nghiệp, mô hình tổng cung - tổng cầu (AS-AD), thị trường tiền tệ, chính sách tài khóa và tiền tệ của Ngân hàng Trung ương.', N'/images/covers/sach_26.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (27, N'978-604-79-2810-1', N'Giáo trình Nguyên lý kế toán', 8, 4, 2023, 1, 410, N'Giáo trình nền tảng kế toán tài chính: đối tượng và phương pháp kế toán, chứng từ và kiểm kê tài sản, tài khoản và ghi sổ kép, tính giá các đối tượng kế toán, và quy trình lập Báo cáo tài chính chuẩn mực.', N'/images/covers/sach_27.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (28, N'978-604-57-7120-9', N'Giáo trình Triết học Mác - Lênin (Dành cho bậc đại học hệ không chuyên lý luận chính trị)', 2, 5, 2023, 1, 492, N'Giáo trình chuẩn quốc gia ban hành theo chương trình mới của Bộ GD&ĐT: Triết học và vai trò của triết học trong đời sống xã hội; Chủ nghĩa duy vật biện chứng (vật chất và ý thức, phép biện chứng duy vật); Chủ nghĩa duy vật lịch sử.', N'/images/covers/sach_28.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (29, N'978-604-57-7121-6', N'Giáo trình Kinh tế chính trị Mác – Lênin (Dành cho bậc đại học)', 2, 5, 2023, 1, 360, N'Cung cấp lý luận hàng hóa, thị trường và vai trò của các chủ thể tham gia thị trường; sản xuất giá trị thặng dư; cạnh tranh và độc quyền trong nền kinh tế thị trường; kinh tế thị trường định hướng XHCN tại Việt Nam.', N'/images/covers/sach_29.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (30, N'978-604-57-7122-3', N'Giáo trình Tư tưởng Hồ Chí Minh (Dành cho bậc đại học)', 2, 5, 2023, 1, 320, N'Hệ thống hóa nguồn gốc, quá trình hình thành và phát triển tư tưởng Hồ Chí Minh: tư tưởng về độc lập dân tộc và chủ nghĩa xã hội, đại đoàn kết toàn dân tộc, xây dựng Nhà nước của nhân dân, do nhân dân, vì nhân dân và đạo đức cách mạng.', N'/images/covers/sach_30.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (31, N'LV-UTC-2023-001', N'Nghiên cứu ứng dụng bê tông tính năng siêu cao (UHPC) trong dầm cầu nhịp lớn tại Việt Nam', 12, 3, 2023, 1, 220, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: NCS. Trần Hoài Nam, PGS.TS. Nguyễn Văn Hậu.', N'/images/covers/luanvan_01.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (32, N'LV-UTC-2024-002', N'Tối ưu hóa điều khiển đèn tín hiệu giao thông thông minh sử dụng học tăng cường sâu (Deep RL)', 12, 1, 2024, 1, 195, N'Luận văn / Luận án ngành Công nghệ thông tin, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: NCS. Lê Quốc Tuấn, PGS.TS. Bùi Thu Lâm.', N'/images/covers/luanvan_02.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (33, N'LV-UTC-2022-003', N'Nghiên cứu giải pháp ổn định mái dốc công trình giao thông bằng cọc neo ứng suất trước', 12, 3, 2022, 1, 145, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2022. Tác giả: ThS. Vũ Đức Thịnh, GS.TS. Nguyễn Ngọc Long.', N'/images/covers/luanvan_03.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (34, N'LV-UTC-2023-004', N'Đánh giá độ tin cậy và suy giảm khả năng chịu lực của cầu BTCT dự ứng lực chịu xâm thực mặn', 12, 3, 2023, 1, 210, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: NCS. Phạm Văn Dũng.', N'/images/covers/luanvan_04.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (35, N'LV-UTC-2023-005', N'Phát triển mô hình dự báo nhu cầu vận tải hành khách công cộng đa phương thức tại TP. Hà Nội', 12, 4, 2023, 1, 185, N'Luận văn / Luận án ngành Kinh tế & Logistics, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: NCS. Nguyễn Thị Mai Lan.', N'/images/covers/luanvan_05.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (36, N'LV-UTC-2024-006', N'Ứng dụng công nghệ xử lý ảnh và YOLOv8 trong nhận diện và đếm phương tiện giao thông thời gian thực', 12, 1, 2024, 1, 130, N'Luận văn / Luận án ngành Công nghệ thông tin, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: ThS. Đỗ Tuấn Anh.', N'/images/covers/luanvan_06.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (37, N'LV-UTC-2023-007', N'Tối ưu mạng lưới phân phối trung tâm Logistics kết nối cảng cạn (ICD) khu vực phía Bắc', 12, 4, 2023, 1, 140, N'Luận văn / Luận án ngành Kinh tế & Logistics, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Hoàng Minh Đức.', N'/images/covers/luanvan_07.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (38, N'LV-UTC-2022-008', N'Nghiên cứu tính toán kết cấu hầm giao thông đô thị đào bằng khiên đào TBM qua đất yếu Hà Nội', 12, 3, 2022, 1, 240, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2022. Tác giả: NCS. Nguyễn Mạnh Cường.', N'/images/covers/luanvan_08.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (39, N'LV-UTC-2023-009', N'Điều khiển thích nghi hệ thống treo chủ động trên ô tô khách nhằm nâng cao độ êm dịu', 12, 3, 2023, 1, 190, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: NCS. Bùi Văn Hưng.', N'/images/covers/luanvan_09.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (40, N'LV-UTC-2024-010', N'Nghiên cứu thiết kế trạm sạc xe điện nhanh tích hợp nguồn năng lượng tái tạo mặt trời', 12, 3, 2024, 1, 125, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: ThS. Phạm Quang Huy.', N'/images/covers/luanvan_10.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (41, N'LV-UTC-2023-011', N'Phân tích động lực học đoàn tàu cao tốc khi chạy qua cầu nhịp giản đơn chịu tải trọng động', 12, 3, 2023, 1, 150, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Trịnh Văn Hải.', N'/images/covers/luanvan_11.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (42, N'LV-UTC-2024-012', N'Mô hình hóa và dự báo tai nạn giao thông trên cao tốc sử dụng mạng nơ-ron hồi quy LSTM', 12, 1, 2024, 1, 205, N'Luận văn / Luận án ngành Công nghệ thông tin, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: NCS. Chu Kim Oanh.', N'/images/covers/luanvan_12.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (43, N'LV-UTC-2023-013', N'Đánh giá tác động của công nghệ Blockchain trong truy xuất nguồn gốc chuỗi cung ứng lạnh nông sản', 12, 4, 2023, 1, 135, N'Luận văn / Luận án ngành Kinh tế & Logistics, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Lương Thu Thảo.', N'/images/covers/luanvan_13.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (44, N'LV-UTC-2022-014', N'Nghiên cứu hành vi ứng xử động của kết cấu mố cầu chịu động đất trong điều kiện đất hóa lỏng', 12, 3, 2022, 1, 230, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2022. Tác giả: NCS. Đặng Xuân Bách.', N'/images/covers/luanvan_14.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (45, N'LV-UTC-2023-015', N'Xây dựng hệ thống bản đồ số GIS hỗ trợ cứu hộ khẩn cấp tai nạn giao thông trên Quốc lộ 1A', 12, 1, 2023, 1, 128, N'Luận văn / Luận án ngành Công nghệ thông tin, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Nguyễn Viết Sơn.', N'/images/covers/luanvan_15.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (46, N'LV-UTC-2024-016', N'Nghiên cứu nâng cao hiệu suất biến tần động cơ kéo trên đoàn tàu đường sắt đô thị Cát Linh - Hà Đông', 12, 3, 2024, 1, 138, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: ThS. Tạ Ngọc Thành.', N'/images/covers/luanvan_16.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (47, N'LV-UTC-2024-017', N'Tác động của chính sách phát triển giao thông xanh đến hành vi lựa chọn phương tiện tại đô thị', 12, 4, 2024, 1, 198, N'Luận văn / Luận án ngành Kinh tế & Logistics, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: NCS. Phan Bích Ngọc.', N'/images/covers/luanvan_17.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (48, N'LV-UTC-2023-018', N'Nghiên cứu thực nghiệm xác định đặc trưng biến dạng dẻo của bê tông nhựa ấm (WMA) phụ gia khoáng', 12, 3, 2023, 1, 142, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Hồ Đình Cương.', N'/images/covers/luanvan_18.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (49, N'LV-UTC-2023-019', N'Nghiên cứu phát triển thuật toán phát hiện bất thường bảo mật trong mạng cảm biến giám sát cầu đường', 12, 1, 2023, 1, 215, N'Luận văn / Luận án ngành Công nghệ thông tin, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: NCS. Mai Văn Tuấn.', N'/images/covers/luanvan_19.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (50, N'LV-UTC-2024-020', N'Tối ưu hóa kế hoạch bảo trì phòng ngừa mặt đường bê tông xi măng đường cất hạ cánh sân bay', 12, 3, 2024, 1, 132, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: ThS. Đoàn Quốc Bảo.', N'/images/covers/luanvan_20.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (51, N'LV-UTC-2023-021', N'Nghiên cứu mô phỏng chuyển động lắc ngang của sà lan chở container qua luồng hàng hải hạn chế', 12, 3, 2023, 1, 140, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Nguyễn Hoàng Long.', N'/images/covers/luanvan_21.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (52, N'LV-UTC-2023-022', N'Ứng dụng kỹ thuật Geopolymer từ tro bay nhiệt điện làm vật liệu gia cố nền đường giao thông', 12, 3, 2023, 1, 225, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: NCS. Vũ Thị Thoa.', N'/images/covers/luanvan_22.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (53, N'LV-UTC-2024-023', N'Xây dựng hệ thống giám sát sức khỏe kết cấu cầu (SHM) dựa trên điện toán đám mây và cảm biến MEMS', 12, 1, 2024, 1, 136, N'Luận văn / Luận án ngành Công nghệ thông tin, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: ThS. Nguyễn Hữu Trọng.', N'/images/covers/luanvan_23.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (54, N'LV-UTC-2023-024', N'Đánh giá hiệu quả kinh tế và giải pháp tài chính đầu tư dự án đường sắt đô thị theo mô hình TOD', 12, 4, 2023, 1, 148, N'Luận văn / Luận án ngành Kinh tế & Logistics, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Trần Phương Thảo.', N'/images/covers/luanvan_24.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (55, N'LV-UTC-2022-025', N'Nghiên cứu tương tác giữa lốp xe và mặt đường ướt nhằm chống hiện tượng trượt nước (Hydroplaning)', 12, 3, 2022, 1, 208, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2022. Tác giả: NCS. Lê Văn Khánh.', N'/images/covers/luanvan_25.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (56, N'LV-UTC-2024-026', N'Phân tích ứng suất cục bộ vùng liên kết dầm chủ và bản mặt cầu thép trực tiếp (Orthotropic Deck)', 12, 3, 2024, 1, 126, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: ThS. Đỗ Thế Anh.', N'/images/covers/luanvan_26.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (57, N'LV-UTC-2023-027', N'Phát triển ứng dụng Web GIS tra cứu thông tin quy hoạch hạ tầng giao thông TP. Đà Nẵng', 12, 1, 2023, 1, 118, N'Luận văn / Luận án ngành Công nghệ thông tin, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Nguyễn Minh Khang.', N'/images/covers/luanvan_27.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (58, N'LV-UTC-2024-028', N'Quản lý rủi ro gián đoạn chuỗi cung ứng container đường biển quốc tế trong bối cảnh khủng hoảng', 12, 4, 2024, 1, 212, N'Luận văn / Luận án ngành Kinh tế & Logistics, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2024. Tác giả: NCS. Phạm Thị Hồng Nhung.', N'/images/covers/luanvan_28.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (59, N'LV-UTC-2023-029', N'Nghiên cứu các giải pháp giảm phát thải khí nhà kính từ đội xe buýt công cộng TP. Hồ Chí Minh', 12, 4, 2023, 1, 134, N'Luận văn / Luận án ngành Kinh tế & Logistics, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: ThS. Nguyễn Đức Thành.', N'/images/covers/luanvan_29.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (60, N'LV-UTC-2023-030', N'Nghiên cứu độ bền mỏi của mối hàn dầm cầu thép chữ I dưới tác dụng của tải trọng đoàn xe nặng', 12, 3, 2023, 1, 235, N'Luận văn / Luận án ngành Kỹ thuật công trình & Giao thông, bảo vệ tại Trường Đại học Giao thông Vận tải năm 2023. Tác giả: NCS. Bùi Đình Quý.', N'/images/covers/luanvan_30.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (61, N'ISSN 1859-4581-85', N'Tạp chí Khoa học Giao thông vận tải (Số 85: Cầu đường và Công trình ngầm)', 14, 3, 2023, 1, 112, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Hội đồng Biên tập Tạp chí JST-UTC.', N'/images/covers/tapchi_01.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (62, N'ISSN 1859-4581-86', N'Tạp chí Khoa học Giao thông vận tải (Số 86: Hệ thống Giao thông Thông minh ITS & AI)', 14, 1, 2023, 1, 120, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Công nghệ thông tin. Cơ quan xuất bản: Hội đồng Biên tập Tạp chí JST-UTC.', N'/images/covers/tapchi_02.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (63, N'ISSN 1859-4581-87', N'Tạp chí Khoa học Giao thông vận tải (Số 87: Đường sắt tốc độ cao và Đô thị)', 14, 3, 2024, 1, 108, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Hội đồng Biên tập Tạp chí JST-UTC.', N'/images/covers/tapchi_03.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (64, N'ISSN 1859-4581-88', N'Tạp chí Khoa học Giao thông vận tải (Số 88: Logistics và Vận tải xanh)', 14, 4, 2024, 1, 116, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: Hội đồng Biên tập Tạp chí JST-UTC.', N'/images/covers/tapchi_04.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (65, N'ISSN 0866-7012-DB', N'Tạp chí Giao thông vận tải (Số đặc biệt: Hạ tầng kết nối vùng kinh tế trọng điểm)', 14, 3, 2023, 1, 96, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Bộ Giao thông Vận tải.', N'/images/covers/tapchi_05.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (66, N'ISSN 0866-7012-648', N'Tạp chí Giao thông vận tải (Số 648: Công nghệ xe điện và trạm sạc tại Việt Nam)', 14, 3, 2024, 1, 88, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Bộ Giao thông Vận tải.', N'/images/covers/tapchi_06.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (67, N'ISSN 0866-7012-649', N'Tạp chí Giao thông vận tải (Số 649: Quy hoạch cảng hàng không và logistics hàng không)', 14, 4, 2024, 1, 92, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: Bộ Giao thông Vận tải.', N'/images/covers/tapchi_07.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (68, N'ISSN 0866-7012-650', N'Tạp chí Giao thông vận tải (Số 650: Chuyển đổi số trong ngành GTVT)', 14, 1, 2024, 1, 104, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Công nghệ thông tin. Cơ quan xuất bản: Bộ Giao thông Vận tải.', N'/images/covers/tapchi_08.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (69, N'ISSN 0866-0762-04', N'Tạp chí Xây dựng (Số 04/2024: Ứng dụng mô hình thông tin công trình BIM trong quản lý dự án)', 16, 3, 2024, 1, 84, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Bộ Xây Dựng.', N'/images/covers/tapchi_09.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (70, N'ISSN 0866-0762-05', N'Tạp chí Xây dựng (Số 05/2024: Vật liệu xây dựng xanh và phát thải ròng bằng 0 Net Zero)', 16, 3, 2024, 1, 80, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Bộ Xây Dựng.', N'/images/covers/tapchi_10.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (71, N'ISSN 1859-1582-12', N'Tạp chí Cầu đường Việt Nam (Số 12/2023: Kỹ thuật kiểm định và bảo trì cầu dây văng)', 15, 3, 2023, 1, 76, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Hội Cầu đường Việt Nam.', N'/images/covers/tapchi_11.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (72, N'ISSN 1859-1582-02', N'Tạp chí Cầu đường Việt Nam (Số 02/2024: Giải pháp thi công hầm vượt sông và xuyên núi)', 15, 3, 2024, 1, 82, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Hội Cầu đường Việt Nam.', N'/images/covers/tapchi_12.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (73, N'ISSN 0866-7120-15', N'Tạp chí Kinh tế & Dự báo (Số 15/2023: Tác động của hành lang kinh tế Bắc - Nam)', 16, 4, 2023, 1, 70, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: Bộ Kế hoạch và Đầu tư.', N'/images/covers/tapchi_13.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (74, N'ISSN 0866-7120-22', N'Tạp chí Kinh tế & Dự báo (Số 22/2023: Quản trị chi phí logistics chuỗi cung ứng xuất khẩu)', 16, 4, 2023, 1, 74, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: Bộ Kế hoạch và Đầu tư.', N'/images/covers/tapchi_14.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (75, N'ISSN 1859-4794-06', N'Tạp chí Khoa học & Công nghệ Việt Nam (Số 06/2024: Bán dẫn và Vi mạch ứng dụng tự động hóa)', 16, 1, 2024, 1, 88, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Công nghệ thông tin. Cơ quan xuất bản: Bộ Khoa học và Công nghệ.', N'/images/covers/tapchi_15.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (76, N'ISSN 0866-7179-40', N'Tạp chí Tin học & Điều khiển học (Tập 40, Số 1: Trí tuệ nhân tạo tạo sinh và xử lý ngôn ngữ)', 16, 1, 2024, 1, 96, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Công nghệ thông tin. Cơ quan xuất bản: Viện Hàn lâm KH&CN Việt Nam.', N'/images/covers/tapchi_16.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (77, N'ISSN 0866-7056-285', N'Tạp chí Cơ khí Việt Nam (Số 285: Công nghệ ô tô hybrid và động cơ pin nhiên liệu Hydro)', 16, 3, 2023, 1, 85, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Tổng hội Cơ khí Việt Nam.', N'/images/covers/tapchi_17.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (78, N'ISSN 1859-204X-270', N'Tạp chí Tự động hóa ngày nay (Số 270: Ứng dụng SCADA và IoT trong giám sát lưới điện)', 16, 3, 2024, 1, 78, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Hội Tự động hóa Việt Nam.', N'/images/covers/tapchi_18.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (79, N'ISSN 0866-7381-45', N'Tạp chí Địa chất & Khoáng sản (Số 45: Địa kỹ thuật công trình ven biển và hải đảo)', 16, 2, 2023, 1, 90, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Khoa học cơ bản & Toán. Cơ quan xuất bản: Viện Khoa học Địa chất.', N'/images/covers/tapchi_19.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (80, N'ISSN 1859-4328-112', N'Tạp chí Quản lý Kinh tế (Số 112: Cơ chế đối tác công tư PPP trong phát triển đường cao tốc)', 16, 4, 2024, 1, 72, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: Viện Nghiên cứu Quản lý Kinh tế TW.', N'/images/covers/tapchi_20.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (81, N'ISSN 1859-3356-180', N'Tạp chí Năng lượng Việt Nam (Số 180: Xu hướng chuyển dịch năng lượng xanh trong ngành vận tải)', 16, 4, 2023, 1, 68, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: Hiệp hội Năng lượng Việt Nam.', N'/images/covers/tapchi_21.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (82, N'ISSN 2734-9888-01', N'Journal of Transport and Communications (Vol. 75, No. 1: Advances in Civil Infrastructure)', 13, 3, 2024, 1, 130, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: UTC Editorial Board.', N'/images/covers/tapchi_22.jpg', N'Tiếng Anh', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (83, N'ISSN 2734-9888-02', N'Journal of Transport and Communications (Vol. 75, No. 2: Intelligent Transportation and AI)', 13, 1, 2024, 1, 140, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Công nghệ thông tin. Cơ quan xuất bản: UTC Editorial Board.', N'/images/covers/tapchi_23.jpg', N'Tiếng Anh', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (84, N'ISSN 2734-9888-03', N'Journal of Transport and Communications (Vol. 75, No. 3: Sustainable Logistics and Maritime)', 13, 4, 2024, 1, 125, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: UTC Editorial Board.', N'/images/covers/tapchi_24.jpg', N'Tiếng Anh', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (85, N'ISSN 0866-7640-198', N'Tạp chí Tiêu chuẩn Đo lường Chất lượng (Số 198: Quy chuẩn an toàn kỹ thuật phương tiện cơ giới)', 16, 3, 2023, 1, 64, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Tổng cục Tiêu chuẩn Đo lường.', N'/images/covers/tapchi_25.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (86, N'ISSN 1859-4085-154', N'Tạp chí Môi trường & Đô thị (Số 154: Đánh giá tác động môi trường của đại dự án hạ tầng)', 16, 4, 2024, 1, 75, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: Hiệp hội Môi trường Đô thị.', N'/images/covers/tapchi_26.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (87, N'ISSN 1859-4077-03', N'Tạp chí An toàn Thông tin (Số 03/2024: Bảo vệ cơ sở hạ tầng thông tin trọng yếu quốc gia)', 16, 1, 2024, 1, 82, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Công nghệ thông tin. Cơ quan xuất bản: Ban Cơ yếu Chính phủ.', N'/images/covers/tapchi_27.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (88, N'ISSN 0866-8574-116', N'Tạp chí Hàng hải Việt Nam (Số 116: Nâng cao năng lực cụm cảng nước sâu Cái Mép - Thị Vải)', 16, 4, 2023, 1, 76, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kinh tế & Logistics. Cơ quan xuất bản: Cục Hàng hải Việt Nam.', N'/images/covers/tapchi_28.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (89, N'ISSN 1859-3313-92', N'Tạp chí Đường sắt Việt Nam (Số 92: Hiện đại hóa tuyến Thống Nhất và công nghệ tín hiệu ETCS)', 16, 3, 2024, 1, 70, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Tổng công ty Đường sắt Việt Nam.', N'/images/covers/tapchi_29.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2)),
        (90, N'ISSN 1859-4263-42', N'Tạp chí Khoa học Công nghệ GTVT (Số 42: Đổi mới sáng tạo và chuyển giao công nghệ giao thông)', 16, 3, 2024, 1, 88, N'Ấn phẩm báo, tạp chí khoa học chuyên ngành Kỹ thuật công trình & Giao thông. Cơ quan xuất bản: Viện KHCN Giao thông Vận tải.', N'/images/covers/tapchi_30.jpg', N'Tiếng Việt', N'Sách in', CAST(N'2026-11-11T00:00:00' AS DATETIME2));

    SET IDENTITY_INSERT [dbo].[TaiLieu] OFF;

    -- 08. TaiKhoan
    CREATE TABLE [dbo].[TaiKhoan]
    (
        [MaTaiKhoan] INT IDENTITY(1,1) NOT NULL,
        [TenDangNhap] VARCHAR(50) NOT NULL,
        [EmailDangNhap] VARCHAR(150) NULL,
        [MatKhauHash] VARCHAR(255) NOT NULL,
        [MaBanDoc] INT NULL,
        [MaNhanVien] INT NULL,
        [TrangThai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_TaiKhoan_TrangThai] DEFAULT (N'Hoạt động'),
        [NgayTao] DATETIME2(7) NOT NULL CONSTRAINT [DF_TaiKhoan_NgayTao] DEFAULT (SYSDATETIME()),
        [LanDangNhapCuoi] DATETIME2(7) NULL,
        [VaiTro] NVARCHAR(50) NULL,
        CONSTRAINT [PK_TaiKhoan] PRIMARY KEY CLUSTERED ([MaTaiKhoan]),
        CONSTRAINT [UQ_TaiKhoan_TenDangNhap] UNIQUE NONCLUSTERED ([TenDangNhap])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UX_TaiKhoan_MaBanDoc_NotNull]
        ON [dbo].[TaiKhoan] ([MaBanDoc])
        WHERE ([MaBanDoc] IS NOT NULL);

    CREATE UNIQUE NONCLUSTERED INDEX [UX_TaiKhoan_MaNhanVien_NotNull]
        ON [dbo].[TaiKhoan] ([MaNhanVien])
        WHERE ([MaNhanVien] IS NOT NULL);

    SET IDENTITY_INSERT [dbo].[TaiKhoan] ON;

    INSERT INTO [dbo].[TaiKhoan] ([MaTaiKhoan], [TenDangNhap], [EmailDangNhap], [MatKhauHash], [MaBanDoc], [MaNhanVien], [TrangThai], [NgayTao], [LanDangNhapCuoi], [VaiTro])
    VALUES
        (1, N'admin', N'admin@utc.edu.vn', N'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', NULL, 1, N'Hoạt động', '2026-11-06 01:43:53.868', '2026-11-08 17:35:17.224', N'ADMIN'),
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
        (16, N'242630923', N'242630923@utc.edu.vn', N'3F79DF17B5F2B19028B6CF39034E3D5E18D6E9896D1883229F7DD6B5946A4F1E', 15, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', '2026-11-08 17:35:07.204', N'READER'),
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
        (40, N'242630971', N'242630971@utc.edu.vn', N'F9351B2733AC02298D2B606707064625CAA4A9D7648949B4666CB48CC1EDA508', 39, NULL, N'Hoạt động', '2026-11-06 01:43:53.887', NULL, N'READER'),
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
        (54, N'GV01', N'tuan.nv@utc.edu.vn', N'0A187109F3D1B0907E92E249CFE90F99AEE7A4E247982008C0862F9D5E52EC32', 50, NULL, N'Đang hoạt động', '2026-11-06 17:02:39.333', '2026-11-08 12:30:33.875', N'Reader'),
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

    SET IDENTITY_INSERT [dbo].[TaiKhoan] OFF;

    -- 09. PhieuMuon
    CREATE TABLE [dbo].[PhieuMuon]
    (
        [MaPhieuMuon] INT IDENTITY(1,1) NOT NULL,
        [MaBanDoc] INT NULL,
        [MaNhanVien] INT NOT NULL,
        [NgayMuon] DATETIME2(7) NOT NULL CONSTRAINT [DF_PhieuMuon_NgayMuon] DEFAULT (SYSDATETIME()),
        [TrangThai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_PhieuMuon_TrangThai] DEFAULT (N'Đang mượn'),
        [GhiChu] NVARCHAR(255) NULL,
        CONSTRAINT [PK_PhieuMuon] PRIMARY KEY CLUSTERED ([MaPhieuMuon])
    );

    CREATE NONCLUSTERED INDEX [IX_PhieuMuon_BanDoc]
        ON [dbo].[PhieuMuon] ([MaBanDoc]);

    SET IDENTITY_INSERT [dbo].[PhieuMuon] ON;

    INSERT INTO [dbo].[PhieuMuon] ([MaPhieuMuon], [MaBanDoc], [MaNhanVien], [NgayMuon], [TrangThai], [GhiChu])
    VALUES
        (1, 15, 1, '2025-08-25 08:30:00.000', N'Đã trả', N'Mượn học phần kỳ 1 năm học 2025-2026'),
        (2, 15, 1, '2026-03-15 10:00:00.000', N'Đang mượn', N'Mượn học phần kỳ 2 năm học 2026'),
        (3, 1, 1, '2025-09-10 14:00:00.000', N'Đang mượn', N'Mượn sách chuyên ngành'),
        (4, 2, 1, '2026-11-06 14:13:15.929', N'Đang mượn', N'Mượn tại quầy lưu thông thư viện'),
        (5, 50, 3, '2026-09-01 08:30:00.000', N'Quá hạn', N'Mượn tài liệu phục vụ giảng dạy Học kỳ 1 (Đang quá hạn)'),
        (6, 51, 3, '2025-10-15 00:00:00.000', N'Đang mượn', N'Giảng viên mượn đề tài NCKH cấp Bộ'),
        (7, 52, 3, '2025-09-20 00:00:00.000', N'Đang mượn', N'Giảng viên mượn tối đa 10 cuốn (hạn mức cao nhất)'),
        (8, 53, 3, '2026-04-10 00:00:00.000', N'Đang mượn', N'Giảng viên mượn hướng dẫn luận văn thạc sĩ'),
        (9, 55, 3, '2025-10-01 00:00:00.000', N'Đang mượn', N'Giảng viên Khoa Vận tải - Kinh tế mượn tài liệu'),
        (10, 15, 1, '2026-11-08 12:12:32.167', N'Đã trả', N'Mượn tại quầy lưu thông thư viện UTC'),
        (11, 15, 1, '2026-11-08 12:26:44.449', N'Đang mượn', N'Mượn tại quầy lưu thông thư viện UTC'),
        (12, 15, 1, '2026-11-08 12:28:04.044', N'Đang mượn', N'Mượn tại quầy lưu thông thư viện UTC'),
        (13, 15, 1, '2026-11-08 12:28:44.868', N'Đang mượn', N'Mượn tại quầy lưu thông thư viện UTC'),
        (14, 1, 1, '2026-11-08 17:35:39.569', N'Đang mượn', N'Mượn tại quầy lưu thông thư viện UTC');

    SET IDENTITY_INSERT [dbo].[PhieuMuon] OFF;

    -- 10. ThanhToan
    CREATE TABLE [dbo].[ThanhToan]
    (
        [MaChiTietMuon] INT NULL,
        [TienNopMuon] DECIMAL(18,2) NULL,
        [TienDenBu] DECIMAL(18,2) NULL,
        [LoaiKhoanThu] NVARCHAR(50) NULL,
        [HinhThucXuLy] NVARCHAR(150) NULL,
        [GhiChu] NVARCHAR(500) NULL,
        [MaThanhToan] INT IDENTITY(1,1) NOT NULL,
        [MaBanDoc] INT NOT NULL,
        [MaNhanVien] INT NOT NULL,
        [NgayThanhToan] DATETIME2(7) NOT NULL CONSTRAINT [DF_ThanhToan_NgayThanhToan] DEFAULT (SYSDATETIME()),
        [TongTien] DECIMAL(18,2) NOT NULL,
        [PhuongThuc] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ThanhToan_PhuongThuc] DEFAULT (N'Tiền mặt'),
        [TrangThai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_ThanhToan_TrangThai] DEFAULT (N'Thành công'),
        [NoiDung] NVARCHAR(500) NULL,
        CONSTRAINT [PK_ThanhToan] PRIMARY KEY CLUSTERED ([MaThanhToan])
    );

    SET IDENTITY_INSERT [dbo].[ThanhToan] ON;

    INSERT INTO [dbo].[ThanhToan] ([MaThanhToan], [MaBanDoc], [MaNhanVien], [NgayThanhToan], [TongTien], [PhuongThuc], [TrangThai], [NoiDung])
    VALUES
        (1, 15, 1, '2026-11-06 01:55:45.556', 100000.00, N'Tiền mặt', N'Thành công', NULL),
        (2, 15, 1, '2026-11-06 15:03:40.727', 100000.00, N'Tiền mặt', N'Thành công', NULL),
        (3, 15, 1, '2026-11-08 12:12:32.586', 100000.00, N'Tiền mặt', N'Thành công', N'Thu phí lưu thông: GD250825-005 (Giáo trình Kỹ nghệ phần mềm (Công nghệ phần mềm))');

    SET IDENTITY_INSERT [dbo].[ThanhToan] OFF;

    -- 11. TaiLieu_TacGia
    CREATE TABLE [dbo].[TaiLieu_TacGia]
    (
        [MaTaiLieu] INT NOT NULL,
        [MaTacGia] INT NOT NULL,
        [VaiTroTacGia] NVARCHAR(50) NOT NULL CONSTRAINT [DF_TaiLieu_TacGia_VaiTroTacGia] DEFAULT (N'Tác giả'),
        CONSTRAINT [PK_TaiLieu_TacGia] PRIMARY KEY CLUSTERED ([MaTaiLieu], [MaTacGia])
    );

    INSERT INTO [dbo].[TaiLieu_TacGia] ([MaTaiLieu], [MaTacGia], [VaiTroTacGia])
    VALUES
        (1, 1, N'Ban biên tập'),
        (2, 2, N'Ban biên tập'),
        (2, 3, N'Tác giả'),
        (2, 4, N'Tác giả'),
        (3, 5, N'Ban biên tập'),
        (3, 6, N'Tác giả'),
        (4, 7, N'Ban biên tập'),
        (5, 8, N'Ban biên tập'),
        (5, 9, N'Tác giả'),
        (6, 8, N'Ban biên tập'),
        (6, 10, N'Tác giả'),
        (7, 11, N'Ban biên tập'),
        (8, 12, N'Ban biên tập'),
        (9, 13, N'Ban biên tập'),
        (10, 14, N'Ban biên tập'),
        (11, 15, N'Ban biên tập'),
        (11, 16, N'Tác giả'),
        (11, 17, N'Tác giả'),
        (12, 15, N'Ban biên tập'),
        (12, 16, N'Tác giả'),
        (12, 17, N'Tác giả'),
        (13, 15, N'Ban biên tập'),
        (13, 16, N'Tác giả'),
        (13, 17, N'Tác giả'),
        (14, 18, N'Ban biên tập'),
        (15, 19, N'Ban biên tập'),
        (16, 19, N'Ban biên tập'),
        (17, 20, N'Ban biên tập'),
        (17, 21, N'Tác giả'),
        (18, 22, N'Ban biên tập'),
        (19, 23, N'Ban biên tập'),
        (19, 24, N'Tác giả'),
        (20, 25, N'Ban biên tập'),
        (20, 26, N'Tác giả'),
        (21, 27, N'Ban biên tập'),
        (22, 28, N'Ban biên tập'),
        (22, 29, N'Tác giả'),
        (23, 30, N'Ban biên tập'),
        (24, 31, N'Ban biên tập'),
        (24, 32, N'Dịch giả'),
        (25, 33, N'Ban biên tập'),
        (25, 34, N'Tác giả'),
        (26, 35, N'Ban biên tập'),
        (27, 36, N'Ban biên tập'),
        (28, 37, N'Ban biên tập'),
        (29, 37, N'Ban biên tập'),
        (30, 37, N'Ban biên tập'),
        (31, 38, N'Tác giả'),
        (32, 39, N'Tác giả'),
        (33, 40, N'Tác giả'),
        (34, 41, N'Tác giả'),
        (35, 42, N'Tác giả'),
        (36, 43, N'Tác giả'),
        (37, 44, N'Tác giả'),
        (38, 45, N'Tác giả'),
        (39, 46, N'Tác giả'),
        (40, 47, N'Tác giả'),
        (41, 48, N'Tác giả'),
        (42, 49, N'Tác giả'),
        (43, 50, N'Tác giả'),
        (44, 51, N'Tác giả'),
        (45, 52, N'Tác giả'),
        (46, 53, N'Tác giả'),
        (47, 54, N'Tác giả'),
        (48, 55, N'Tác giả'),
        (49, 56, N'Tác giả'),
        (50, 57, N'Tác giả'),
        (51, 58, N'Tác giả'),
        (52, 59, N'Tác giả'),
        (53, 60, N'Tác giả'),
        (54, 61, N'Tác giả'),
        (55, 62, N'Tác giả'),
        (56, 63, N'Tác giả'),
        (57, 64, N'Tác giả'),
        (58, 65, N'Tác giả'),
        (59, 66, N'Tác giả'),
        (60, 67, N'Tác giả'),
        (61, 68, N'Ban biên tập'),
        (62, 68, N'Ban biên tập'),
        (63, 68, N'Ban biên tập'),
        (64, 68, N'Ban biên tập'),
        (65, 69, N'Ban biên tập'),
        (66, 69, N'Ban biên tập'),
        (67, 69, N'Ban biên tập'),
        (68, 69, N'Ban biên tập'),
        (69, 70, N'Ban biên tập'),
        (70, 70, N'Ban biên tập'),
        (71, 71, N'Ban biên tập'),
        (72, 71, N'Ban biên tập'),
        (73, 72, N'Ban biên tập'),
        (74, 72, N'Ban biên tập'),
        (75, 73, N'Ban biên tập'),
        (76, 74, N'Ban biên tập'),
        (77, 75, N'Ban biên tập'),
        (78, 76, N'Ban biên tập'),
        (79, 77, N'Ban biên tập'),
        (80, 78, N'Ban biên tập'),
        (81, 79, N'Ban biên tập'),
        (82, 80, N'Ban biên tập'),
        (83, 80, N'Ban biên tập'),
        (84, 80, N'Ban biên tập'),
        (85, 81, N'Ban biên tập'),
        (86, 82, N'Ban biên tập'),
        (87, 83, N'Ban biên tập'),
        (88, 84, N'Ban biên tập'),
        (89, 85, N'Ban biên tập'),
        (90, 86, N'Ban biên tập');

    -- 12. BanSao
    CREATE TABLE [dbo].[BanSao]
    (
        [MaBanSao] INT IDENTITY(1,1) NOT NULL,
        [MaTaiLieu] INT NOT NULL,
        [MaVach] VARCHAR(50) NOT NULL,
        [ViTriKe] NVARCHAR(100) NULL,
        [TinhTrang] NVARCHAR(30) NOT NULL CONSTRAINT [DF_BanSao_TinhTrang] DEFAULT (N'Tốt'),
        [TrangThai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_BanSao_TrangThai] DEFAULT (N'Có sẵn'),
        [NgayNhap] DATE NULL,
        CONSTRAINT [PK_BanSao] PRIMARY KEY CLUSTERED ([MaBanSao]),
        CONSTRAINT [UQ_BanSao_MaVach] UNIQUE NONCLUSTERED ([MaVach])
    );

    CREATE NONCLUSTERED INDEX [IX_BanSao_MaTaiLieu]
        ON [dbo].[BanSao] ([MaTaiLieu]);

    SET IDENTITY_INSERT [dbo].[BanSao] ON;

    INSERT INTO [dbo].[BanSao] ([MaBanSao], [MaTaiLieu], [MaVach], [ViTriKe], [TinhTrang], [TrangThai], [NgayNhap])
    VALUES
        (1, 1, N'UTC-01-01', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
        (2, 1, N'UTC-01-02', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-01-15'),
        (3, 1, N'UTC-01-03', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-02-20'),
        (4, 1, N'UTC-01-04', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
        (5, 2, N'UTC-02-01', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-01-15'),
        (6, 2, N'UTC-02-02', N'Kệ IT-01', N'Tốt', N'Đang mượn', '2024-01-15'),
        (7, 2, N'UTC-02-03', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
        (8, 2, N'UTC-02-04', N'Kệ IT-01', N'Tốt', N'Có sẵn', '2024-02-20'),
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
        (33, 9, N'UTC-09-01', N'Kệ IT-05', N'Tốt', N'Có sẵn', '2024-01-15'),
        (34, 9, N'UTC-09-02', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-01-15'),
        (35, 9, N'UTC-09-03', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-02-20'),
        (36, 9, N'UTC-09-04', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-02-20'),
        (37, 10, N'UTC-10-01', N'Kệ IT-05', N'Mới', N'Đang mượn', '2024-01-15'),
        (38, 10, N'UTC-10-02', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-01-15'),
        (39, 10, N'UTC-10-03', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-02-20'),
        (40, 10, N'UTC-10-04', N'Kệ IT-05', N'Tốt', N'Đang mượn', '2024-02-20'),
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
        (73, 19, N'UTC-19-01', N'Kệ GT-02', N'Mới', N'Đang mượn', '2024-01-15'),
        (74, 19, N'UTC-19-02', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
        (75, 19, N'UTC-19-03', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
        (76, 19, N'UTC-19-04', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
        (77, 20, N'UTC-20-01', N'Kệ GT-02', N'Mới', N'Có sẵn', '2024-01-15'),
        (78, 20, N'UTC-20-02', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-01-15'),
        (79, 20, N'UTC-20-03', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
        (80, 20, N'UTC-20-04', N'Kệ GT-02', N'Tốt', N'Có sẵn', '2024-02-20'),
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
        (97, 25, N'UTC-25-01', N'Kệ KT-01', N'Tốt', N'Có sẵn', '2024-01-15'),
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
        (120, 30, N'UTC-30-04', N'Kệ LL-02', N'Tốt', N'Có sẵn', '2024-02-20'),
        (121, 1, N'UTC-01-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (122, 1, N'UTC-01-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (123, 18, N'UTC-18-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (124, 18, N'UTC-18-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (125, 6, N'UTC-06-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Đang mượn', '2026-11-08'),
        (126, 6, N'UTC-06-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (127, 4, N'UTC-04-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Đang mượn', '2026-11-08'),
        (128, 4, N'UTC-04-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (129, 2, N'UTC-02-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Đang mượn', '2026-11-08'),
        (130, 2, N'UTC-02-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (131, 22, N'UTC-22-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (132, 22, N'UTC-22-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (133, 8, N'UTC-08-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (134, 8, N'UTC-08-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (135, 29, N'UTC-29-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (136, 29, N'UTC-29-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (137, 25, N'UTC-25-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (138, 25, N'UTC-25-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (139, 26, N'UTC-26-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (140, 26, N'UTC-26-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (141, 9, N'UTC-09-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (142, 9, N'UTC-09-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (143, 5, N'UTC-05-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Đang mượn', '2026-11-08'),
        (144, 5, N'UTC-05-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (145, 10, N'UTC-10-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (146, 10, N'UTC-10-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (147, 3, N'UTC-03-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Đang mượn', '2026-11-08'),
        (148, 3, N'UTC-03-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (149, 19, N'UTC-19-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (150, 19, N'UTC-19-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (151, 27, N'UTC-27-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (152, 27, N'UTC-27-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (153, 23, N'UTC-23-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (154, 23, N'UTC-23-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (155, 17, N'UTC-17-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (156, 17, N'UTC-17-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (157, 30, N'UTC-30-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (158, 30, N'UTC-30-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (159, 20, N'UTC-20-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (160, 20, N'UTC-20-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (161, 7, N'UTC-07-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Đang mượn', '2026-11-08'),
        (162, 7, N'UTC-07-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (163, 28, N'UTC-28-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (164, 28, N'UTC-28-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (165, 14, N'UTC-14-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (166, 14, N'UTC-14-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (167, 21, N'UTC-21-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (168, 21, N'UTC-21-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (169, 24, N'UTC-24-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (170, 24, N'UTC-24-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (171, 11, N'UTC-11-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (172, 11, N'UTC-11-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (173, 12, N'UTC-12-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (174, 12, N'UTC-12-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (175, 13, N'UTC-13-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (176, 13, N'UTC-13-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (177, 15, N'UTC-15-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (178, 15, N'UTC-15-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (179, 16, N'UTC-16-05', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (180, 16, N'UTC-16-06', N'Kệ lưu thông Tầng 2', N'Tốt', N'Có sẵn', '2026-11-08'),
        (181, 31, N'UTC-LV-31-01', N'Kệ LV-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (182, 31, N'UTC-LV-31-02', N'Kệ LV-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (183, 32, N'UTC-LV-32-01', N'Kệ LV-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (184, 32, N'UTC-LV-32-02', N'Kệ LV-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (185, 33, N'UTC-LV-33-01', N'Kệ LV-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (186, 33, N'UTC-LV-33-02', N'Kệ LV-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (187, 34, N'UTC-LV-34-01', N'Kệ LV-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (188, 34, N'UTC-LV-34-02', N'Kệ LV-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (189, 35, N'UTC-LV-35-01', N'Kệ LV-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (190, 35, N'UTC-LV-35-02', N'Kệ LV-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (191, 36, N'UTC-LV-36-01', N'Kệ LV-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (192, 36, N'UTC-LV-36-02', N'Kệ LV-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (193, 37, N'UTC-LV-37-01', N'Kệ LV-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (194, 37, N'UTC-LV-37-02', N'Kệ LV-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (195, 38, N'UTC-LV-38-01', N'Kệ LV-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (196, 38, N'UTC-LV-38-02', N'Kệ LV-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (197, 39, N'UTC-LV-39-01', N'Kệ LV-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (198, 39, N'UTC-LV-39-02', N'Kệ LV-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (199, 40, N'UTC-LV-40-01', N'Kệ LV-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (200, 40, N'UTC-LV-40-02', N'Kệ LV-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (201, 41, N'UTC-LV-41-01', N'Kệ LV-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (202, 41, N'UTC-LV-41-02', N'Kệ LV-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (203, 42, N'UTC-LV-42-01', N'Kệ LV-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (204, 42, N'UTC-LV-42-02', N'Kệ LV-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (205, 43, N'UTC-LV-43-01', N'Kệ LV-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (206, 43, N'UTC-LV-43-02', N'Kệ LV-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (207, 44, N'UTC-LV-44-01', N'Kệ LV-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (208, 44, N'UTC-LV-44-02', N'Kệ LV-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (209, 45, N'UTC-LV-45-01', N'Kệ LV-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (210, 45, N'UTC-LV-45-02', N'Kệ LV-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (211, 46, N'UTC-LV-46-01', N'Kệ LV-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (212, 46, N'UTC-LV-46-02', N'Kệ LV-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (213, 47, N'UTC-LV-47-01', N'Kệ LV-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (214, 47, N'UTC-LV-47-02', N'Kệ LV-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (215, 48, N'UTC-LV-48-01', N'Kệ LV-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (216, 48, N'UTC-LV-48-02', N'Kệ LV-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (217, 49, N'UTC-LV-49-01', N'Kệ LV-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (218, 49, N'UTC-LV-49-02', N'Kệ LV-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (219, 50, N'UTC-LV-50-01', N'Kệ LV-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (220, 50, N'UTC-LV-50-02', N'Kệ LV-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (221, 51, N'UTC-LV-51-01', N'Kệ LV-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (222, 51, N'UTC-LV-51-02', N'Kệ LV-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (223, 52, N'UTC-LV-52-01', N'Kệ LV-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (224, 52, N'UTC-LV-52-02', N'Kệ LV-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (225, 53, N'UTC-LV-53-01', N'Kệ LV-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (226, 53, N'UTC-LV-53-02', N'Kệ LV-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (227, 54, N'UTC-LV-54-01', N'Kệ LV-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (228, 54, N'UTC-LV-54-02', N'Kệ LV-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (229, 55, N'UTC-LV-55-01', N'Kệ LV-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (230, 55, N'UTC-LV-55-02', N'Kệ LV-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (231, 56, N'UTC-LV-56-01', N'Kệ LV-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (232, 56, N'UTC-LV-56-02', N'Kệ LV-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (233, 57, N'UTC-LV-57-01', N'Kệ LV-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (234, 57, N'UTC-LV-57-02', N'Kệ LV-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (235, 58, N'UTC-LV-58-01', N'Kệ LV-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (236, 58, N'UTC-LV-58-02', N'Kệ LV-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (237, 59, N'UTC-LV-59-01', N'Kệ LV-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (238, 59, N'UTC-LV-59-02', N'Kệ LV-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (239, 60, N'UTC-LV-60-01', N'Kệ LV-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (240, 60, N'UTC-LV-60-02', N'Kệ LV-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (241, 61, N'UTC-TC-61-01', N'Kệ TC-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (242, 61, N'UTC-TC-61-02', N'Kệ TC-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (243, 62, N'UTC-TC-62-01', N'Kệ TC-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (244, 62, N'UTC-TC-62-02', N'Kệ TC-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (245, 63, N'UTC-TC-63-01', N'Kệ TC-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (246, 63, N'UTC-TC-63-02', N'Kệ TC-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (247, 64, N'UTC-TC-64-01', N'Kệ TC-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (248, 64, N'UTC-TC-64-02', N'Kệ TC-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (249, 65, N'UTC-TC-65-01', N'Kệ TC-01', N'Mới', N'Có sẵn', '2026-11-08'),
        (250, 65, N'UTC-TC-65-02', N'Kệ TC-01', N'Tốt', N'Có sẵn', '2026-11-08'),
        (251, 66, N'UTC-TC-66-01', N'Kệ TC-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (252, 66, N'UTC-TC-66-02', N'Kệ TC-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (253, 67, N'UTC-TC-67-01', N'Kệ TC-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (254, 67, N'UTC-TC-67-02', N'Kệ TC-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (255, 68, N'UTC-TC-68-01', N'Kệ TC-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (256, 68, N'UTC-TC-68-02', N'Kệ TC-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (257, 69, N'UTC-TC-69-01', N'Kệ TC-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (258, 69, N'UTC-TC-69-02', N'Kệ TC-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (259, 70, N'UTC-TC-70-01', N'Kệ TC-02', N'Mới', N'Có sẵn', '2026-11-08'),
        (260, 70, N'UTC-TC-70-02', N'Kệ TC-02', N'Tốt', N'Có sẵn', '2026-11-08'),
        (261, 71, N'UTC-TC-71-01', N'Kệ TC-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (262, 71, N'UTC-TC-71-02', N'Kệ TC-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (263, 72, N'UTC-TC-72-01', N'Kệ TC-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (264, 72, N'UTC-TC-72-02', N'Kệ TC-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (265, 73, N'UTC-TC-73-01', N'Kệ TC-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (266, 73, N'UTC-TC-73-02', N'Kệ TC-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (267, 74, N'UTC-TC-74-01', N'Kệ TC-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (268, 74, N'UTC-TC-74-02', N'Kệ TC-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (269, 75, N'UTC-TC-75-01', N'Kệ TC-03', N'Mới', N'Có sẵn', '2026-11-08'),
        (270, 75, N'UTC-TC-75-02', N'Kệ TC-03', N'Tốt', N'Có sẵn', '2026-11-08'),
        (271, 76, N'UTC-TC-76-01', N'Kệ TC-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (272, 76, N'UTC-TC-76-02', N'Kệ TC-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (273, 77, N'UTC-TC-77-01', N'Kệ TC-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (274, 77, N'UTC-TC-77-02', N'Kệ TC-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (275, 78, N'UTC-TC-78-01', N'Kệ TC-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (276, 78, N'UTC-TC-78-02', N'Kệ TC-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (277, 79, N'UTC-TC-79-01', N'Kệ TC-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (278, 79, N'UTC-TC-79-02', N'Kệ TC-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (279, 80, N'UTC-TC-80-01', N'Kệ TC-04', N'Mới', N'Có sẵn', '2026-11-08'),
        (280, 80, N'UTC-TC-80-02', N'Kệ TC-04', N'Tốt', N'Có sẵn', '2026-11-08'),
        (281, 81, N'UTC-TC-81-01', N'Kệ TC-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (282, 81, N'UTC-TC-81-02', N'Kệ TC-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (283, 82, N'UTC-TC-82-01', N'Kệ TC-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (284, 82, N'UTC-TC-82-02', N'Kệ TC-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (285, 83, N'UTC-TC-83-01', N'Kệ TC-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (286, 83, N'UTC-TC-83-02', N'Kệ TC-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (287, 84, N'UTC-TC-84-01', N'Kệ TC-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (288, 84, N'UTC-TC-84-02', N'Kệ TC-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (289, 85, N'UTC-TC-85-01', N'Kệ TC-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (290, 85, N'UTC-TC-85-02', N'Kệ TC-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (291, 86, N'UTC-TC-86-01', N'Kệ TC-05', N'Mới', N'Có sẵn', '2026-11-08'),
        (292, 86, N'UTC-TC-86-02', N'Kệ TC-05', N'Tốt', N'Có sẵn', '2026-11-08'),
        (293, 87, N'UTC-TC-87-01', N'Kệ TC-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (294, 87, N'UTC-TC-87-02', N'Kệ TC-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (295, 88, N'UTC-TC-88-01', N'Kệ TC-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (296, 88, N'UTC-TC-88-02', N'Kệ TC-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (297, 89, N'UTC-TC-89-01', N'Kệ TC-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (298, 89, N'UTC-TC-89-02', N'Kệ TC-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (299, 90, N'UTC-TC-90-01', N'Kệ TC-06', N'Mới', N'Có sẵn', '2026-11-08'),
        (300, 90, N'UTC-TC-90-02', N'Kệ TC-06', N'Tốt', N'Có sẵn', '2026-11-08'),
        (301, 79, N'UTC-79-03', N'Kệ TC-04', N'Tốt', N'Đang mượn', '2026-11-08');

    SET IDENTITY_INSERT [dbo].[BanSao] OFF;

    -- 13. DatTruoc
    CREATE TABLE [dbo].[DatTruoc]
    (
        [MaDatTruoc] INT IDENTITY(1,1) NOT NULL,
        [MaBanDoc] INT NOT NULL,
        [MaTaiLieu] INT NOT NULL,
        [NgayDat] DATETIME2(7) NOT NULL CONSTRAINT [DF_DatTruoc_NgayDat] DEFAULT (SYSDATETIME()),
        [HanNhan] DATETIME2(7) NULL,
        [TrangThai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_DatTruoc_TrangThai] DEFAULT (N'Chờ xử lý'),
        CONSTRAINT [PK_DatTruoc] PRIMARY KEY CLUSTERED ([MaDatTruoc])
    );

    -- 14. ChiTietMuon
    CREATE TABLE [dbo].[ChiTietMuon]
    (
        [MaChiTietMuon] INT IDENTITY(1,1) NOT NULL,
        [MaPhieuMuon] INT NOT NULL,
        [MaBanSao] INT NOT NULL,
        [HanTra] DATE NOT NULL,
        [NgayTra] DATETIME2(7) NULL,
        [TinhTrangKhiTra] NVARCHAR(100) NULL,
        [TrangThai] NVARCHAR(30) NOT NULL CONSTRAINT [DF_ChiTietMuon_TrangThai] DEFAULT (N'Đang mượn'),
        CONSTRAINT [PK_ChiTietMuon] PRIMARY KEY CLUSTERED ([MaChiTietMuon])
    );

    CREATE NONCLUSTERED INDEX [IX_ChiTietMuon_BanSao]
        ON [dbo].[ChiTietMuon] ([MaBanSao]);

    SET IDENTITY_INSERT [dbo].[ChiTietMuon] ON;

    INSERT INTO [dbo].[ChiTietMuon] ([MaChiTietMuon], [MaPhieuMuon], [MaBanSao], [HanTra], [NgayTra], [TinhTrangKhiTra], [TrangThai])
    VALUES
        (1, 2, 1, '2027-03-15', '2026-11-08 12:53:41.615', N'Bình thường', N'Đã trả'),
        (2, 2, 37, '2027-05-20', NULL, NULL, N'Đang mượn'),
        (3, 1, 5, '2026-10-10', '2026-11-06 01:55:45.458', N'Bình thường', N'Đã trả'),
        (4, 1, 21, '2026-08-25', '2026-11-06 15:03:40.653', N'Bình thường', N'Đã trả'),
        (5, 1, 33, '2026-11-01', '2026-11-08 12:12:32.515', N'Bình thường', N'Đã trả'),
        (6, 3, 9, '2026-09-10', NULL, NULL, N'Quá hạn'),
        (7, 3, 13, '2027-04-10', '2026-11-06 15:25:21.694', N'Bình thường', N'Đã trả'),
        (8, 4, 22, '2027-11-06', NULL, NULL, N'Đang mượn'),
        (9, 4, 14, '2027-11-06', NULL, NULL, N'Đang mượn'),
        (10, 4, 17, '2027-11-06', NULL, NULL, N'Đang mượn'),
        (11, 3, 5, '2027-11-06', '2026-11-06 21:17:59.131', N'Bình thường', N'Đã trả'),
        (12, 3, 2, '2027-11-06', NULL, NULL, N'Đang mượn'),
        (13, 3, 18, '2027-11-06', NULL, NULL, N'Đang mượn'),
        (14, 5, 3, '2026-10-15', NULL, NULL, N'Quá hạn'),
        (15, 5, 6, '2026-10-18', NULL, NULL, N'Quá hạn'),
        (16, 5, 10, '2026-10-22', NULL, NULL, N'Quá hạn'),
        (17, 5, 127, '2026-10-25', NULL, NULL, N'Quá hạn'),
        (18, 5, 143, '2026-10-28', NULL, NULL, N'Quá hạn'),
        (19, 5, 125, '2026-11-01', NULL, NULL, N'Quá hạn'),
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
        (40, 8, 36, '2027-04-10', NULL, NULL, N'Đang mượn'),
        (41, 8, 38, '2027-04-10', NULL, NULL, N'Đang mượn'),
        (42, 8, 39, '2027-04-10', NULL, NULL, N'Đang mượn'),
        (43, 9, 40, '2026-10-01', NULL, NULL, N'Quá hạn'),
        (44, 9, 41, '2027-06-15', NULL, NULL, N'Đang mượn'),
        (45, 9, 42, '2027-06-15', NULL, NULL, N'Đang mượn'),
        (46, 9, 43, '2027-06-15', NULL, NULL, N'Đang mượn'),
        (47, 9, 44, '2027-06-15', NULL, NULL, N'Đang mượn'),
        (48, 9, 45, '2027-06-15', NULL, NULL, N'Đang mượn'),
        (49, 9, 46, '2027-06-15', NULL, NULL, N'Đang mượn'),
        (50, 3, 5, '2027-11-08', NULL, NULL, N'Đang mượn'),
        (51, 10, 97, '2027-11-08', '2026-11-08 12:15:26.109', N'Bình thường', N'Đã trả'),
        (52, 11, 147, '2027-11-08', NULL, NULL, N'Đang mượn'),
        (53, 12, 129, '2027-11-08', NULL, NULL, N'Đang mượn'),
        (54, 13, 73, '2027-11-08', NULL, NULL, N'Đang mượn'),
        (55, 5, 161, '2026-11-04', NULL, NULL, N'Quá hạn'),
        (56, 14, 301, '2027-11-08', NULL, NULL, N'Đang mượn');

    SET IDENTITY_INSERT [dbo].[ChiTietMuon] OFF;

    -- 15. ViPham
    CREATE TABLE [dbo].[ViPham]
    (
        [MaViPham] INT IDENTITY(1,1) NOT NULL,
        [MaBanDoc] INT NOT NULL,
        [MaChiTietMuon] INT NULL,
        [LoaiViPham] NVARCHAR(50) NOT NULL,
        [MoTa] NVARCHAR(255) NULL,
        [NgayViPham] DATETIME2(7) NOT NULL CONSTRAINT [DF_ViPham_NgayViPham] DEFAULT (SYSDATETIME()),
        [SoTienPhat] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_ViPham_SoTienPhat] DEFAULT ((0)),
        [TrangThaiNop] NVARCHAR(60) NOT NULL CONSTRAINT [DF_ViPham_TrangThaiNop] DEFAULT (N'Chưa nộp'),
        [NgayNop] DATETIME2(7) NULL,
        [MaNhanVienThu] INT NULL,
        CONSTRAINT [PK_ViPham] PRIMARY KEY CLUSTERED ([MaViPham])
    );

    SET IDENTITY_INSERT [dbo].[ViPham] ON;

    INSERT INTO [dbo].[ViPham] ([MaViPham], [MaBanDoc], [MaChiTietMuon], [LoaiViPham], [MoTa], [NgayViPham], [SoTienPhat], [TrangThaiNop], [NgayNop], [MaNhanVienThu])
    VALUES
        (1, 15, 3, N'Mượn quá hạn', N'Quá hạn trả sách Cơ sở dữ liệu quan hệ (thời hạn 1 năm)', '2026-10-11 00:00:00.000', 100000.00, N'Đã thanh toán', NULL, NULL),
        (2, 15, 4, N'Mượn quá hạn', N'Quá hạn trả sách C++ và Lập trình hướng đối tượng (thời hạn 1 năm)', '2026-08-26 00:00:00.000', 100000.00, N'Đã thanh toán', NULL, NULL),
        (3, 15, 5, N'Mượn quá hạn', N'Quá hạn trả sách Kỹ nghệ phần mềm (thời hạn 1 năm)', '2026-11-02 00:00:00.000', 100000.00, N'Đã thanh toán', '2026-11-08 12:12:32.586', NULL),
        (4, 1, 6, N'Mượn quá hạn', N'Quá hạn sách Mạng máy tính', '2026-09-11 00:00:00.000', 100000.00, N'Chưa thanh toán', NULL, NULL);

    SET IDENTITY_INSERT [dbo].[ViPham] OFF;

    -- 16. GiaHan
    CREATE TABLE [dbo].[GiaHan]
    (
        [MaGiaHan] INT IDENTITY(1,1) NOT NULL,
        [MaChiTietMuon] INT NOT NULL,
        [HanTraCu] DATE NOT NULL,
        [HanTraMoi] DATE NOT NULL,
        [MaNhanVienDuyet] INT NOT NULL,
        [NgayGiaHan] DATETIME2(7) NOT NULL CONSTRAINT [DF_GiaHan_NgayGiaHan] DEFAULT (SYSDATETIME()),
        [LyDo] NVARCHAR(255) NULL,
        CONSTRAINT [PK_GiaHan] PRIMARY KEY CLUSTERED ([MaGiaHan])
    );

    ALTER TABLE [dbo].[TheLoai] WITH CHECK
        ADD CONSTRAINT [FK_TheLoai_MaTheLoaiCha] FOREIGN KEY ([MaTheLoaiCha])
        REFERENCES [dbo].[TheLoai] ([MaTheLoai]);

    ALTER TABLE [dbo].[BanDoc] WITH CHECK
        ADD CONSTRAINT [FK_BanDoc_MaLop] FOREIGN KEY ([MaLop])
        REFERENCES [dbo].[Lop] ([MaLop]);

    ALTER TABLE [dbo].[TaiLieu] WITH CHECK
        ADD CONSTRAINT [FK_TaiLieu_MaNhaXuatBan] FOREIGN KEY ([MaNhaXuatBan])
        REFERENCES [dbo].[NhaXuatBan] ([MaNhaXuatBan]);

    ALTER TABLE [dbo].[TaiLieu] WITH CHECK
        ADD CONSTRAINT [FK_TaiLieu_MaTheLoai] FOREIGN KEY ([MaTheLoai])
        REFERENCES [dbo].[TheLoai] ([MaTheLoai]);

    ALTER TABLE [dbo].[TaiKhoan] WITH CHECK
        ADD CONSTRAINT [FK_TaiKhoan_MaBanDoc] FOREIGN KEY ([MaBanDoc])
        REFERENCES [dbo].[BanDoc] ([MaBanDoc]);

    ALTER TABLE [dbo].[TaiKhoan] WITH CHECK
        ADD CONSTRAINT [FK_TaiKhoan_MaNhanVien] FOREIGN KEY ([MaNhanVien])
        REFERENCES [dbo].[NhanVien] ([MaNhanVien]);

    ALTER TABLE [dbo].[PhieuMuon] WITH CHECK
        ADD CONSTRAINT [FK_PhieuMuon_MaBanDoc] FOREIGN KEY ([MaBanDoc])
        REFERENCES [dbo].[BanDoc] ([MaBanDoc]);

    ALTER TABLE [dbo].[PhieuMuon] WITH CHECK
        ADD CONSTRAINT [FK_PhieuMuon_MaNhanVien] FOREIGN KEY ([MaNhanVien])
        REFERENCES [dbo].[NhanVien] ([MaNhanVien]);

    ALTER TABLE [dbo].[ThanhToan] WITH CHECK
        ADD CONSTRAINT [FK_ThanhToan_MaBanDoc] FOREIGN KEY ([MaBanDoc])
        REFERENCES [dbo].[BanDoc] ([MaBanDoc]);

    ALTER TABLE [dbo].[ThanhToan] WITH CHECK
        ADD CONSTRAINT [FK_ThanhToan_MaNhanVien] FOREIGN KEY ([MaNhanVien])
        REFERENCES [dbo].[NhanVien] ([MaNhanVien]);

    ALTER TABLE [dbo].[TaiLieu_TacGia] WITH CHECK
        ADD CONSTRAINT [FK_TaiLieu_TacGia_MaTacGia] FOREIGN KEY ([MaTacGia])
        REFERENCES [dbo].[TacGia] ([MaTacGia]);

    ALTER TABLE [dbo].[TaiLieu_TacGia] WITH CHECK
        ADD CONSTRAINT [FK_TaiLieu_TacGia_MaTaiLieu] FOREIGN KEY ([MaTaiLieu])
        REFERENCES [dbo].[TaiLieu] ([MaTaiLieu]);

    ALTER TABLE [dbo].[BanSao] WITH CHECK
        ADD CONSTRAINT [FK_BanSao_MaTaiLieu] FOREIGN KEY ([MaTaiLieu])
        REFERENCES [dbo].[TaiLieu] ([MaTaiLieu]);

    ALTER TABLE [dbo].[DatTruoc] WITH CHECK
        ADD CONSTRAINT [FK_DatTruoc_MaBanDoc] FOREIGN KEY ([MaBanDoc])
        REFERENCES [dbo].[BanDoc] ([MaBanDoc]);

    ALTER TABLE [dbo].[DatTruoc] WITH CHECK
        ADD CONSTRAINT [FK_DatTruoc_MaTaiLieu] FOREIGN KEY ([MaTaiLieu])
        REFERENCES [dbo].[TaiLieu] ([MaTaiLieu]);

    ALTER TABLE [dbo].[ChiTietMuon] WITH CHECK
        ADD CONSTRAINT [FK_ChiTietMuon_MaBanSao] FOREIGN KEY ([MaBanSao])
        REFERENCES [dbo].[BanSao] ([MaBanSao]);

    ALTER TABLE [dbo].[ChiTietMuon] WITH CHECK
        ADD CONSTRAINT [FK_ChiTietMuon_MaPhieuMuon] FOREIGN KEY ([MaPhieuMuon])
        REFERENCES [dbo].[PhieuMuon] ([MaPhieuMuon]);

    ALTER TABLE [dbo].[ViPham] WITH CHECK
        ADD CONSTRAINT [FK_ViPham_MaBanDoc] FOREIGN KEY ([MaBanDoc])
        REFERENCES [dbo].[BanDoc] ([MaBanDoc]);

    ALTER TABLE [dbo].[ViPham] WITH CHECK
        ADD CONSTRAINT [FK_ViPham_MaChiTietMuon] FOREIGN KEY ([MaChiTietMuon])
        REFERENCES [dbo].[ChiTietMuon] ([MaChiTietMuon]);

    ALTER TABLE [dbo].[ViPham] WITH CHECK
        ADD CONSTRAINT [FK_ViPham_MaNhanVienThu] FOREIGN KEY ([MaNhanVienThu])
        REFERENCES [dbo].[NhanVien] ([MaNhanVien]);

    ALTER TABLE [dbo].[GiaHan] WITH CHECK
        ADD CONSTRAINT [FK_GiaHan_MaChiTietMuon] FOREIGN KEY ([MaChiTietMuon])
        REFERENCES [dbo].[ChiTietMuon] ([MaChiTietMuon]);

    ALTER TABLE [dbo].[GiaHan] WITH CHECK
        ADD CONSTRAINT [FK_GiaHan_MaNhanVienDuyet] FOREIGN KEY ([MaNhanVienDuyet])
        REFERENCES [dbo].[NhanVien] ([MaNhanVien]);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
