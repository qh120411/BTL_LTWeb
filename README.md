<<<<<<< Updated upstream
# BTL_LTWeb — Website Thư viện UTC
=======
# Thư viện UTC — bản thử ASP.NET Core MVC theo giao diện Huy chốt
>>>>>>> Stashed changes

Kế thừa nhánh `DoDat` của https://github.com/qh120411/BTL_LTWeb tại commit `469e7ce6c90ddaa0b5146596e1993de11d0c7f7f`. Trang chủ theo ảnh bố cục đã chốt: tìm kiếm, bốn tiện ích, ba cột học liệu/ảnh thư viện/thông báo, nguồn học liệu, sách và dịch vụ. Giao diện người dùng và quản trị dùng chung tông xanh UTC.

Đây là một bản MVC độc lập để nhóm thử và đối chiếu. Các file của nhánh gốc vẫn được giữ trong lịch sử Git; layout đang chạy sử dụng Razor và CSS trong `wwwroot/utc/`.

## Chạy nhanh trong Visual Studio

1. Giải nén toàn bộ thư mục, mở `BTL_LTWEB.sln`.
2. Cài workload **ASP.NET and web development** và .NET 8 SDK nếu máy chưa có.
3. Chọn profile **http**, chạy bằng Ctrl+F5.
4. Mở `http://localhost:5124`. Trang chủ và tra cứu cho khách xem trực tiếp.

Không chạy bản MVC bằng Live Server hoặc mở file `.cshtml` trực tiếp.

<<<<<<< Updated upstream
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
=======
Nếu dùng VS Code: mở thư mục gốc dự án, mở Terminal và chạy:
>>>>>>> Stashed changes

```bash
dotnet restore
dotnet run --launch-profile http
```

Trên Windows có thể chạy `Mo-MVC.bat`. Lần đầu cần Internet để tải NuGet. Khi website chạy, database SQLite riêng được tạo tại `App_Data/utc-huy-demo.db`. Database này chứa 4 đầu sách, 16 bản sách, 2 bạn đọc và tài khoản quản trị. Khởi động lại giữ dữ liệu đã nhập; seeder không ghi đè database có tài khoản.

| Vai trò | Tên đăng nhập | Mật khẩu demo |
| --- | --- | --- |
| Quản trị | `admin` | `Admin@123` |
| Bạn đọc | `242630944` | `UTC@123` |
| Bạn đọc thử thứ hai | `242630945` | `UTC@123` |

Tài khoản demo dành cho chạy thử cục bộ. Thông tin danh mục/tồn kho minh họa không phải thống kê thực tế của UTC.

## Những phần đã nối backend

- Trang chủ và tra cứu lấy danh mục từ `TaiLieu`, tồn kho từ `BanSao`; tìm không dấu, lọc loại và tình trạng. Chi tiết mã không tồn tại trả 404.
- Đăng nhập đọc `TaiKhoan`, hồ sơ và vai trò từ `TaiKhoan_VaiTro`. Cookie phía server, POST có antiforgery; tài khoản bị khóa không tiếp tục dùng phiên cũ.
- Mật khẩu mới dùng ASP.NET Core PasswordHasher. Hash SHA-256 hợp lệ của dữ liệu cũ chỉ được chuyển đổi sau khi xác minh đúng mật khẩu. Không nhận mật khẩu thường, `HASH_msv`, mật khẩu dự phòng hoặc nút SSO giả.
- Thêm/sửa/xóa học liệu bằng EF Core; không xóa học liệu có lịch sử mượn/đặt trước. Nhập học liệu tạo bản sách với mã riêng.
- Lập phiếu mượn, kiểm tra thẻ/hạn thẻ, trạng thái bạn đọc, tối đa 5 cuốn, không mượn trùng đầu sách, không có quá hạn hoặc khoản phạt chưa trả. Cấp bản sẵn có trong transaction và cập nhật có điều kiện để tránh cấp trùng.
- Trả sách đối chiếu đúng mã bản; quá hạn phải xác nhận thu phạt; mất sách đền tiền không cộng bản mất trở lại số sẵn có. Chi tiết mượn, tồn kho, khoản phạt và thanh toán ghi trong cùng transaction.
- Hồ sơ liên hệ và sách đang mượn/lịch sử lấy từ database. Bạn đọc chỉ sửa liên hệ của chính mình.
- Nhập bạn đọc mới bằng TSV: 3 cột MSV, họ tên, email; kiểm tra cả lô rồi ghi. Không giả lập đồng bộ QLĐT, không tự cấp tài khoản hoặc thẻ cho người vừa nhập.
- Dashboard, phiếu thu và báo cáo tồn kho lấy dữ liệu thực trong database; xuất CSV UTF-8 có BOM.

Các quy tắc 5 cuốn, hạn 1 năm, phí quá hạn 100.000 đ kế thừa ý tưởng nghiệp vụ nhánh gốc để thử luồng. Nhóm cần đối chiếu bản phỏng vấn/yêu cầu để chốt trước khi nộp.

## SQL Server

Bản mặc định dùng SQLite để chạy thử ngay. EF Core model được kế thừa từ nhánh DoDat và vẫn có provider SQL Server.

Để thử SQL Server với database riêng:

1. Trong SSMS, chạy `Database/01-create-sqlserver-demo.sql` **một lần** để tạo `QLThuVienUTC_HuyDemo`. Script không xóa database/bảng hiện có. Nếu tên đã tồn tại, dùng database thử mới hoặc giữ database và bỏ bước tạo; không chạy lại toàn bộ script.
2. Chạy `Database/02-demo-login.sql` trên đúng database demo vừa tạo để cấp hash hợp lệ cho `admin` và `242630944`. Các tài khoản `HASH_msv` trong dữ liệu gốc chưa có mật khẩu sử dụng được.
3. Sao chép cấu hình từ `appsettings.SqlServer.example.json` vào `appsettings.Development.json`. Sửa `Server` theo máy: `(localdb)\\MSSQLLocalDB`, `.\\SQLEXPRESS` hoặc tên instance thực tế. Giữ `Database:SeedDemo=false`.
4. Chạy lại ứng dụng. SQL Server demo có dữ liệu mẫu gốc của DoDat, nên danh mục/số lượng khác bản SQLite.

Hai file SQL ở thư mục gốc là tài liệu gốc của nhánh DoDat; script `QLThuVienUTC_TiengViet.sql` có lệnh DROP. Quy trình bản này sử dụng **hai file trong `Database/`** để thử trên database riêng.

**Đã chạy kiểm tra tích hợp trên SQLite. Chưa chạy trực tiếp với một SQL Server trong môi trường này.** Cần thử lại nghiệp vụ và đối chiếu schema khi chuyển sang SQL Server thật của nhóm. Không tự scaffold hoặc migrate đè model/database nhóm đang sử dụng.

## Cấu trúc chính và chỗ sửa

| Vị trí | Vai trò |
| --- | --- |
| `Program.cs` | DI, provider database, cookie, CSRF, route MVC và seeder demo |
| `Controllers/AccountController.cs` | Xác thực và cookie |
| `Areas/Reader/Controllers` | Trang công khai, hồ sơ và thư viện cá nhân |
| `Areas/Admin/Controllers` | Điều phối nghiệp vụ quản trị |
| `Areas/Reader/Services/BookCatalogService.cs` | Danh mục và bản sách qua EF Core |
| `Areas/Admin/Services/ReaderService.cs` | Mượn trả, tồn kho, thu phạt và bạn đọc |
| `Areas/Reader/Services/UserProfileService.cs` | Hồ sơ cá nhân từ database |
| `Models/Entities` | Entity/DbContext kế thừa từ DoDat |
| `Models/Entities/QLThuVienUtcContext.Portable.cs` | Điều chỉnh cho SQLite và dùng bảng liên kết role thay cột VaiTro dư |
| `Areas/Reader/Views/Home/Index.cshtml` | Bố cục trang chủ đã chốt |
| `Views/Shared/Reader` | Layout, header và footer người dùng |
| `Views/Shared/Admin` | Layout quản trị |
| `wwwroot/utc/css/home.css` | Chỉnh riêng trang chủ |
| `wwwroot/utc/css/site.css`, `admin.css`, `mvc.css` | CSS dùng chung và quản trị |
| `wwwroot/utc/js/mvc.js` | Menu mobile, tab tìm kiếm, xác nhận form; không chứa dữ liệu nghiệp vụ |
| `Database/` | Script SQL Server demo riêng |
| `Tests/` | Bộ kiểm tra tích hợp tự động |

Các controller sử dụng view chuẩn của Area; hai layout trong Area chuyển về layout dùng chung để chỉnh một chỗ. Một số view/CSS cũ vẫn nằm trong repo để đối chiếu, không được layout hiện tại nạp. `App_Data/*.json` của nhánh gốc không còn là nguồn cho danh mục, hồ sơ hoặc phiếu thu.

## Phạm vi bản thử

- Màn tài khoản nhân viên hiện hiển thị danh sách và vai trò; chưa có UI cấp/reset mật khẩu.
- Chưa tích hợp Microsoft SSO, API QLĐT, đặt trước, gia hạn hoặc CMS đăng thông báo. Không có nút giả đăng nhập thay người dùng.
- Phân loại chuyên ngành hiện ánh xạ từ tên thể loại của schema gốc; cần bổ sung mapping chính thức nếu nhóm có danh mục chuyên ngành riêng.
- Báo cáo là tồn kho hiện tại; không giả số liệu lịch sử theo các kỳ tùy chọn.
- Toàn bộ tra cứu hiện materialize danh mục để hỗ trợ tìm không dấu. Khi dữ liệu lớn cần phân trang và tìm kiếm phía SQL.
- Trước triển khai thật cần cấu hình HTTPS, bí mật kết nối, sao lưu, giới hạn đăng nhập, audit và rà lại quyền/quy chế theo yêu cầu đã phỏng vấn.

## Kiểm tra

Đã build .NET 8 và kiểm tra các luồng qua HTTP/browser trên database SQLite tạm độc lập: đăng nhập, từ chối mật khẩu dự phòng, quyền reader/admin, CSRF, sửa liên hệ, thêm học liệu, mượn trùng, sai mã bản, trả và bồi thường, khóa thẻ, nhập TSV nguyên lô, CSV; 16 route trên 5 kích thước 1920/1366/768/390/375px. Không có lỗi JavaScript, tài nguyên thiếu hoặc tràn ngang toàn trang ở các route được kiểm tra.

<<<<<<< Updated upstream
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
=======
Chạy lại bộ kiểm tra theo hướng dẫn trong `Tests/README.md`.
>>>>>>> Stashed changes
