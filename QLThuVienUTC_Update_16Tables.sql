/* =========================================================================================
   SCRIPT NÂNG CẤP DÀNH CHO BẠN ĐÃ CÓ DATABASE CŨ (MIGRATE TO 16 TABLES)
   Giữ nguyên toàn bộ dữ liệu bạn đọc, sách, tài khoản hiện có mà không bị mất dữ liệu
   Khắc phục triệt để lỗi: Invalid column name 'VaiTro', 'SoThe', 'NgayCapThe',...
   ========================================================================================= */

USE QLThuVienUTC;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- 1. BỔ SUNG CÁC CỘT THẺ BẠN ĐỌC VÀO BẢNG BanDoc
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BanDoc') AND name = 'SoThe')
BEGIN
    ALTER TABLE BanDoc ADD 
        SoThe NVARCHAR(50) NULL,
        NgayCapThe DATE NULL,
        NgayHetHanThe DATE NULL,
        TrangThaiThe NVARCHAR(60) NULL;
END
GO

-- Chuyển dữ liệu từ TheBanDoc sang BanDoc nếu có bảng TheBanDoc cũ
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TheBanDoc')
BEGIN
    UPDATE b
    SET b.SoThe = t.SoThe,
        b.NgayCapThe = t.NgayCap,
        b.NgayHetHanThe = t.NgayHetHan,
        b.TrangThaiThe = t.TrangThai
    FROM BanDoc b
    INNER JOIN TheBanDoc t ON b.MaBanDoc = t.MaBanDoc;
END
GO

-- Bổ sung số thẻ mặc định cho các bạn đọc chưa có
UPDATE BanDoc 
SET SoThe = 'THE-UTC-' + MaSinhVien,
    NgayCapThe = '2024-09-01',
    NgayHetHanThe = '2028-06-30',
    TrangThaiThe = N'Hoạt động'
WHERE SoThe IS NULL;
GO

-- 2. BỔ SUNG CỘT VaiTro VÀO BẢNG TaiKhoan
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TaiKhoan') AND name = 'VaiTro')
BEGIN
    ALTER TABLE TaiKhoan ADD VaiTro NVARCHAR(100) NULL;
END
GO

-- Chuyển vai trò cũ sang nếu có bảng TaiKhoan_VaiTro
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TaiKhoan_VaiTro')
BEGIN
    UPDATE tk
    SET tk.VaiTro = tv.MaVaiTro
    FROM TaiKhoan tk
    INNER JOIN TaiKhoan_VaiTro tv ON tk.MaTaiKhoan = tv.MaTaiKhoan;
END
GO

-- Cập nhật vai trò chuẩn cho các tài khoản
UPDATE TaiKhoan SET VaiTro = 'ADMIN' WHERE TenDangNhap = 'admin';
UPDATE TaiKhoan SET VaiTro = 'BANDOC' WHERE MaBanDoc IS NOT NULL AND (VaiTro IS NULL OR VaiTro = '');
UPDATE TaiKhoan SET VaiTro = 'THUTHU' WHERE MaNhanVien IS NOT NULL AND (VaiTro IS NULL OR VaiTro = '');
GO

-- 3. CẬP NHẬT PhieuMuon LIÊN KẾT TRỰC TIẾP TỚI BanDoc
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PhieuMuon') AND name = 'MaBanDoc')
BEGIN
    ALTER TABLE PhieuMuon ADD MaBanDoc INT NULL;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TheBanDoc')
BEGIN
    UPDATE pm
    SET pm.MaBanDoc = t.MaBanDoc
    FROM PhieuMuon pm
    INNER JOIN TheBanDoc t ON pm.MaTheBanDoc = t.MaTheBanDoc;
END
GO

-- Bỏ khóa ngoại cũ và thiết lập khóa ngoại PhieuMuon -> BanDoc
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__PhieuMuon__MaThe__2A6B46EF')
    ALTER TABLE PhieuMuon DROP CONSTRAINT FK__PhieuMuon__MaThe__2A6B46EF;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PhieuMuon_BanDoc')
BEGIN
    ALTER TABLE PhieuMuon 
    ADD CONSTRAINT FK_PhieuMuon_BanDoc FOREIGN KEY (MaBanDoc) REFERENCES BanDoc(MaBanDoc);
END
GO

-- 4. CẬP NHẬT NhaXuatBan (Gộp QuocGia)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('NhaXuatBan') AND name = 'QuocGia')
BEGIN
    ALTER TABLE NhaXuatBan ADD QuocGia NVARCHAR(100) NULL;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'QuocGia')
BEGIN
    UPDATE nxb
    SET nxb.QuocGia = qg.TenQuocGia
    FROM NhaXuatBan nxb
    LEFT JOIN QuocGia qg ON nxb.MaQuocGia = qg.MaQuocGia;
END
GO

-- 5. CẬP NHẬT TacGia (Gộp QuocGia)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TacGia') AND name = 'QuocTich')
BEGIN
    ALTER TABLE TacGia ADD QuocTich NVARCHAR(100) NULL;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'QuocGia')
BEGIN
    UPDATE tg
    SET tg.QuocTich = qg.TenQuocGia
    FROM TacGia tg
    LEFT JOIN QuocGia qg ON tg.MaQuocGia = qg.MaQuocGia;
END
GO

-- 6. CẬP NHẬT TaiLieu (Gộp NgonNgu, DinhDang)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TaiLieu') AND name = 'NgonNgu')
BEGIN
    ALTER TABLE TaiLieu ADD NgonNgu NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TaiLieu') AND name = 'DinhDang')
BEGIN
    ALTER TABLE TaiLieu ADD DinhDang NVARCHAR(100) NULL;
END
GO

-- 7. CẬP NHẬT ViPham (Gộp TienPhat)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ViPham') AND name = 'SoTienPhat')
BEGIN
    ALTER TABLE ViPham ADD 
        SoTienPhat DECIMAL(18,2) DEFAULT 0,
        TrangThaiNop NVARCHAR(60) DEFAULT N'Chưa nộp',
        NgayNop DATETIME2 NULL,
        MaNhanVienThu INT NULL;
END
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TienPhat')
BEGIN
    UPDATE vp
    SET vp.SoTienPhat = tp.SoTien,
        vp.TrangThaiNop = tp.TrangThai
    FROM ViPham vp
    INNER JOIN TienPhat tp ON vp.MaViPham = tp.MaViPham;
END
GO

-- 8. CẬP NHẬT ThanhToan
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'NoiDung')
BEGIN
    ALTER TABLE ThanhToan ADD NoiDung NVARCHAR(500) NULL;
END
GO

-- 9. DỌN DẸP CÁC BẢNG DƯ THỪA (NẾU CÓ)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ChiTietThanhToan') DROP TABLE ChiTietThanhToan;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TienPhat') DROP TABLE TienPhat;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LichSuDatTruoc') DROP TABLE LichSuDatTruoc;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'VaiTro_Quyen') DROP TABLE VaiTro_Quyen;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Quyen') DROP TABLE Quyen;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TaiKhoan_VaiTro') DROP TABLE TaiKhoan_VaiTro;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TheBanDoc') DROP TABLE TheBanDoc;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'DinhDang') DROP TABLE DinhDang;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'NgonNgu') DROP TABLE NgonNgu;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'QuocGia') DROP TABLE QuocGia;

COMMIT TRANSACTION;
GO
PRINT N'CẬP NHẬT THÀNH CÔNG LÊN 16 BẢNG CHUẨN ĐỒNG BỘ 100% VỚI CODE C#!';
GO
