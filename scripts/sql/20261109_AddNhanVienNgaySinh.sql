-- Compatibility repair for the existing QLThuVienUTC schema.
-- Adds the nullable column already used by Models/Entities/NhanVien.cs.
-- Does not drop tables, recreate the database, or change existing records.
USE [QLThuVienUTC];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NULL
    THROW 50001, 'The existing dbo.NhanVien table was not found. No schema changes applied.', 1;
IF COL_LENGTH(N'dbo.NhanVien', N'NgaySinh') IS NULL
    ALTER TABLE dbo.NhanVien ADD NgaySinh date NULL;
COMMIT TRANSACTION;
GO
