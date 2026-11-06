-- CHỈ dùng database demo vừa tạo ở bước 01. Không áp dụng database thật.
USE QLThuVienUTC_HuyDemo;
GO
BEGIN TRANSACTION;
UPDATE dbo.TaiKhoan SET MatKhauHash=N'AQAAAAIAAYagAAAAEHfab2oahb/8lJU5zXdHHtA2zr36mZx6bsdBShi58422Ic24UoGNSMK50/1aPYOE3w==',TrangThai=N'Hoạt động' WHERE TenDangNhap='admin';
UPDATE dbo.TaiKhoan SET MatKhauHash=N'AQAAAAIAAYagAAAAENM20RXJ0kq3YE5VSrQm7TedBnh7iYJRDj91U02rGk6sDDUlwR5qczVbOQVX9cjotw==',TrangThai=N'Hoạt động' WHERE TenDangNhap='242630944';
COMMIT;
GO
-- admin / Admin@123; 242630944 / UTC@123. Các tài khoản HASH_msv gốc chưa được kích hoạt.
