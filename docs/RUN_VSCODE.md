# Chạy dự án bằng VS Code

## Chạy và debug

1. Mở đúng thư mục chứa `BTL_LTWEB.csproj` bằng VS Code.
2. Nhấn **F5**, chọn **UTC: Debug web (localhost:5125)**. Trình duyệt tự mở khi ứng dụng sẵn sàng.
3. Hoặc chọn **Terminal → Run Task → UTC: run (localhost:5124)** để chạy không debug.

Có thể chạy từ terminal bằng `dotnet run --launch-profile http` tại http://localhost:5124.
Debug dùng cổng 5125 để không xung đột với bản chạy terminal ở cổng 5124. Dùng Shift+F5 để dừng debugger; Ctrl+C để dừng bản chạy terminal.

Extension cần dùng: **C#** của Microsoft (`ms-dotnettools.csharp`). .NET SDK cần hỗ trợ build `net8.0`; máy hiện tại đã có SDK và ASP.NET Core Runtime phù hợp.

## SQL Server trên máy này

- Instance đang chạy: `.\HUYPC` (dịch vụ `MSSQL$HUYPC`).
- Database hiện có: `QLThuVienUTC`.
- Xác thực: Windows Authentication, theo tài khoản Windows chạy VS Code.
- Chuỗi kết nối Development nằm trong `appsettings.Development.json`; không chứa mật khẩu.
- Giữ nguyên cấu hình các môi trường khác trong `appsettings.json`.
- Đã bổ sung cột `NhanVien.NgaySinh` kiểu `date NULL` để khớp entity EF hiện có. Bản vá lặp lại an toàn: `scripts/sql/20261109_AddNhanVienNgaySinh.sql`; không xóa bảng hoặc sửa bản ghi.

Kiểm tra kết nối chỉ đọc trong terminal:

```powershell
sqlcmd -S '.\HUYPC' -E -C -d QLThuVienUTC -l 5 -Q "SELECT DB_NAME() AS DatabaseName;"
```

Nếu chuyển sang máy khác, sửa instance trong cấu hình Development cho phù hợp. Không chạy lại các script SQL khởi tạo trên database đang dùng: các script hiện có chứa lệnh xóa bảng và có thể mất dữ liệu.

## Luồng truy cập

- `/` → `/Reader/Home`: trang chủ công khai.
- Tra cứu, chi tiết học liệu, dịch vụ và tin tức: không yêu cầu đăng nhập.
- Hồ sơ, thư viện cá nhân: yêu cầu đăng nhập và giữ kiểm tra quyền hiện có.
- `/Admin/Dashboard` và các nghiệp vụ quản trị: yêu cầu đăng nhập và giữ kiểm tra vai trò hiện có.
- `/Account/Login`: đăng nhập theo yêu cầu; đăng xuất quay lại trang chủ.
- `/preview/admin.html`: bản xem trước giao diện quản trị bằng dữ liệu minh họa, không ghi database.

Kết nối SQL Server chỉ cấu hình lại đích kết nối. Các service đang lưu một phần dữ liệu trong `App_Data/*.json` vẫn được giữ theo kiến trúc dự án; không tự chuyển dữ liệu JSON vào SQL.
