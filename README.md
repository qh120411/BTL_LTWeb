# BTL_LTWeb — Website Thư viện UTC

Bài tập lớn môn Lập trình Web, xây dựng website thư viện Trường Đại học Giao thông Vận tải (UTC) theo mô hình **ASP.NET Core MVC**. Project hướng đến hỗ trợ bạn đọc tra cứu học liệu, tiếp cận dịch vụ thư viện và hỗ trợ quản lý tài liệu, bạn đọc, hoạt động mượn trả.

## Thiết kế Figma

Link Figma: [UTC Library — Prototype](https://www.figma.com/proto/4ul02l3jmMTv1gssiJbQyc/UTC-Library)

## Tổng quan chức năng

### Đã có trong mã nguồn

- Khung ứng dụng MVC với `HomeController`, Razor Views và layout dùng chung.
- Giao diện header, footer, logo và thanh điều hướng của thư viện UTC.
- Menu có nút mở/đóng trên màn hình nhỏ.
- Khung trang chủ, trang Privacy và trang thông báo lỗi.
- Script SQL tạo cơ sở dữ liệu `QLThuVienUTC`, các bảng, ràng buộc và dữ liệu mẫu.

### Định hướng phát triển

- Tra cứu học liệu theo thông tin tài liệu, tác giả, thể loại và ngôn ngữ.
- Hiển thị dịch vụ, tin tức, thông báo và giới thiệu thư viện.
- Đăng nhập và phân quyền tài khoản.
- Quản lý tài liệu, bản sao, tác giả và nhà xuất bản.
- Quản lý bạn đọc, thẻ bạn đọc và giao dịch mượn trả.

**Trạng thái hiện tại:** project đang ở giai đoạn xây dựng giao diện và chuẩn bị cơ sở dữ liệu. Nội dung trang chủ còn là khung mẫu; các mục điều hướng và nút đăng nhập chưa có đầy đủ xử lý nghiệp vụ. Ứng dụng chưa cấu hình kết nối SQL Server hoặc tầng truy cập dữ liệu.

## Công nghệ sử dụng

| Thành phần | Công nghệ |
| --- | --- |
| Nền tảng | .NET 8, ASP.NET Core MVC |
| Ngôn ngữ phía máy chủ | C# |
| Giao diện | Razor Views (`.cshtml`), HTML, CSS, JavaScript |
| Thư viện giao diện | Bootstrap; có sẵn jQuery và các thư viện validation trong `wwwroot/lib/` |
| Cơ sở dữ liệu thiết kế | Microsoft SQL Server, T-SQL |
| Công cụ phát triển | Visual Studio hoặc .NET CLI |
| Container | Dockerfile sử dụng Windows Nano Server |

## Cây thư mục

```text
BTL_LTWeb/
├── Controllers/
│   └── HomeController.cs             # Xử lý trang chủ, Privacy và lỗi
├── Models/
│   └── ErrorViewModel.cs             # Model hiển thị thông tin lỗi
├── Properties/
│   └── launchSettings.json          # Profile chạy và cổng localhost
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml              # Trang chủ
│   │   └── Privacy.cshtml            # Trang Privacy
│   ├── Shared/
│   │   ├── _Header.cshtml            # Header và thanh điều hướng
│   │   ├── _Footer.cshtml            # Footer và thông tin liên hệ
│   │   ├── _Layout.cshtml            # Bố cục chung
│   │   ├── _Layout.cshtml.css        # CSS riêng của layout
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml              # Trang thông báo lỗi
│   ├── _ViewImports.cshtml           # Namespace và Tag Helpers
│   └── _ViewStart.cshtml             # Khai báo layout mặc định
├── wwwroot/
│   ├── css/
│   │   └── site.css                  # CSS của website
│   ├── js/
│   │   └── site.js                   # JavaScript của website
│   ├── lib/                         # Bootstrap, jQuery, validation
│   ├── logo/
│   │   └── utc.png                   # Logo UTC
│   └── favicon.ico
├── .dockerignore
├── .gitignore                       # Bỏ qua cache IDE, file build và cấu hình cá nhân
├── appsettings.json                 # Cấu hình ứng dụng
├── appsettings.Development.json     # Cấu hình môi trường phát triển
├── BTL_LTWEB.csproj                  # Cấu hình project và package
├── BTL_LTWEB.sln                     # Solution Visual Studio
├── Dockerfile                       # Cấu hình build container Windows
├── Program.cs                       # Khởi tạo ứng dụng, middleware, routing
├── QLThuVienUTC_TheoBaoCao-1.sql      # Script cơ sở dữ liệu và dữ liệu mẫu
└── README.md
```

Cây trên tập trung vào mã nguồn; không liệt kê `.git/`, `.vs/`, `.vs_backup/`, `bin/`, `obj/` và file cấu hình IDE cá nhân. Thư mục `wwwroot/images/` cũng đã được khai báo trong project để bổ sung hình ảnh.

## Cài đặt và chạy project

### Yêu cầu

- .NET SDK 8.0.
- Git để tải mã nguồn và làm việc với các nhánh.
- Visual Studio có workload **ASP.NET and web development** nếu chạy bằng IDE.
- SQL Server và SQL Server Management Studio (SSMS) nếu cần khởi tạo cơ sở dữ liệu. Chạy giao diện hiện tại chưa cần SQL Server.

### Chạy bằng .NET CLI

```bash
git clone https://github.com/qh120411/BTL_LTWeb.git
cd BTL_LTWeb
dotnet restore
dotnet build
dotnet run --launch-profile http
```

Truy cập: **http://localhost:5124**.

Để chạy bằng HTTPS:

```bash
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

Truy cập: **https://localhost:7223**. Các địa chỉ được cấu hình trong `Properties/launchSettings.json`.

### Chạy bằng Visual Studio

1. Mở file `BTL_LTWEB.sln`.
2. Chờ Visual Studio khôi phục các package NuGet.
3. Chọn profile `http` hoặc `https`.
4. Nhấn **F5** để chạy kèm debug hoặc **Ctrl + F5** để chạy không debug.

## Cơ sở dữ liệu

File `QLThuVienUTC_TheoBaoCao-1.sql` tạo database **`QLThuVienUTC`** với các nhóm dữ liệu:

| Nhóm | Các bảng |
| --- | --- |
| Danh mục | `tQuocGia`, `tNgonNgu`, `tTheLoai`, `tDinhDang` |
| Nhân sự và tài khoản | `tVaiTro`, `tNhanVien`, `tTaiKhoan` |
| Tài liệu | `tTacGia`, `tNhaXuatBan`, `tTaiLieu`, `tTaiLieu_TacGia`, `tBanSao` |
| Bạn đọc | `tBanDoc`, `tTheBanDoc` |
| Mượn trả | `tGiaoDichMuonTra`, `tGiaoDich_BanSao` |

Để khởi tạo dữ liệu:

1. Mở SSMS và kết nối đến SQL Server có quyền tạo database.
2. Mở file `QLThuVienUTC_TheoBaoCao-1.sql`.
3. Kiểm tra tên database, sau đó chạy script.

Script có lệnh `CREATE DATABASE` và các lệnh tạo bảng, chèn dữ liệu; chỉ chạy khởi tạo khi database `QLThuVienUTC` chưa tồn tại. Nếu đã có database, cần xem và điều chỉnh script trước khi chạy lại.

Sau bước này, database tồn tại độc lập với website. Để sử dụng dữ liệu trong ứng dụng, cần bổ sung thư viện truy cập dữ liệu, cấu hình kết nối, model nghiệp vụ và đăng ký dịch vụ trong `Program.cs`.

## Quy trình tạo nhánh và push code

**Mỗi thành viên tạo nhánh riêng trước khi sửa code và push lên đúng nhánh đó. Không push trực tiếp lên `main`; tạo Pull Request để nhóm review và hợp nhất.**

### 1. Kiểm tra thay đổi và cập nhật nhánh gốc

```bash
git status
```

Nếu đang có thay đổi chưa commit, hãy lưu chúng trên nhánh đang làm việc hoặc dùng `git stash` trước khi chuyển nhánh. Khi thư mục làm việc đã sạch:

```bash
git switch main
git pull --ff-only origin main
```

### 2. Tạo nhánh mới

Đặt tên nhánh rõ mục đích, ví dụ `feature/ten-thanh-vien-tra-cuu` hoặc `fix/ten-thanh-vien-header`.

```bash
git switch -c feature/ten-thanh-vien-tra-cuu
```

### 3. Thêm file và commit

Sau khi chỉnh sửa, kiểm tra diff và chỉ stage các file thuộc phần việc của mình. Ví dụ với thay đổi trang chủ:

```bash
git status
git diff
git add Views/Home/Index.cshtml
git diff --staged
git commit -m "feat: cap nhat giao dien trang chu"
```

Thay đường dẫn trong `git add` bằng các file thực tế đã sửa. Tránh dùng `git add .` khi chưa kiểm tra, vì có thể đưa cả file build và cấu hình cá nhân vào commit.

### 4. Push nhánh lên repository

```bash
git push -u origin feature/ten-thanh-vien-tra-cuu
```

Tên nhánh khi push phải trùng với nhánh vừa tạo. Sau lần đầu thiết lập upstream bằng `-u`, các lần tiếp theo trên cùng nhánh chỉ cần:

```bash
git push
```

### 5. Tạo Pull Request

Trên GitHub, tạo Pull Request từ nhánh vừa push vào `main`, mô tả nội dung thay đổi và cách kiểm tra. Chờ nhóm review trước khi merge.

### Lưu ý khi làm việc nhóm

- Kiểm tra nhánh hiện tại bằng `git branch --show-current` trước khi commit và push.
- Cập nhật `main` trước khi tạo nhánh cho công việc mới.
- Chỉ commit mã nguồn, tài nguyên và tài liệu cần thiết. `.gitignore` đã cấu hình bỏ qua `.vs/`, `.vs_backup/`, `bin/`, `obj/`, file cấu hình IDE cá nhân và log; vẫn cần kiểm tra kỹ danh sách file được stage và không commit thông tin kết nối riêng.
- Nếu gặp xung đột, xử lý và kiểm tra lại trước khi commit; không dùng force push lên nhánh chung.
- Chạy `dotnet build` và kiểm tra các trang liên quan trước khi gửi Pull Request.
