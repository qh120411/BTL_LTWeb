# Báo cáo rà soát UI và cấu trúc backend

Ngày thực hiện: **12/11/2026**. Dự án: **BTL_LTWEB — Thư viện UTC**.

> **Trạng thái mới nhất: SQL-only và đã sửa các mặc định không an toàn, xem mục 9 và 11.** Mục 1–8 là lịch sử trước chuyển đổi. Mục 10 là kết quả quét **trước khi sửa**; các đoạn mã sinh viên mẫu/số tiền mẫu được nêu ở đó đã được xử lý tại mục 11.

## 1. Phạm vi và cách đối chiếu

Rà soát mã nguồn tại `Controllers`, `Areas`, `Common`, `Models`, `Views`, `wwwroot/css`, `wwwroot/js`, cấu hình khởi động, script SQL, tài liệu và bộ kiểm tra hiện có. Trước khi dọn, phần mã ứng dụng gồm 62 file C#, 83 Razor view, 8 file JavaScript và 20 stylesheet trong `wwwroot/css`. Có 21 file controller, gồm 2 controller cơ sở trừu tượng. Không tính thư viện bên thứ ba, mã sinh trong `bin`/`obj`, `.git` và các bản build thử trong `.tmp`.

Mỗi chức năng được đối chiếu theo chuỗi:

`Menu / liên kết / form / JavaScript → route → controller → service → model / dữ liệu`.

Kiểm tra cả lời gọi qua `Url.Action`, Tag Helper, hộp thoại dùng chung, AJAX, kế thừa controller, dependency injection và quan hệ Entity Framework. Không kết luận một hàm là thừa chỉ vì không có một trang HTML riêng hoặc không xuất hiện trên menu.

Chỉ gỡ phần không có luồng sử dụng trong UI hiện tại và không cần cho API đã công bố hoặc nghiệp vụ đang hoạt động. Những phần còn dữ liệu, có tài liệu sử dụng hoặc mới triển khai một phần được ghi rõ bên dưới.

## 2. Kết quả đối chiếu chức năng

| Nhóm chức năng | UI / đường vào hiện tại | Backend tương ứng | Kết luận |
| --- | --- | --- | --- |
| Trang chủ | `/Reader/Home` | Reader `HomeController` | Giữ trang; bỏ tải hồ sơ không được view sử dụng |
| Tra cứu và chi tiết học liệu | `/Reader/Search`, `/Reader/Document/Details/{id}` | `SearchController`, `DocumentController`, `BookCatalogService` | Đang dùng, giữ |
| Dịch vụ, hướng dẫn | `/Reader/Guide` | `GuideController` và Razor view | Đang dùng, giữ |
| Tin tức & thông báo | `/Reader/News` | `NewsController` và Razor view | Đang dùng; nội dung tĩnh, chưa có nghiệp vụ quản trị tin |
| Hồ sơ, sách đang mượn, khoản phạt | `/Reader/Profile` | `ProfileController`, `UserProfileService` | Đang dùng, giữ |
| Đăng nhập thư viện | `/Account/Login` | `AccountController`, SQL `TaiKhoan`, cookie xác thực | Đang dùng, giữ |
| Tổng quan quản trị | `/Admin`, `/Admin/Dashboard` | `DashboardController`, `GetDashboardStatistics` | Đang dùng; hợp nhất đường vào `/Admin/Home` bằng redirect |
| Quản lý học liệu | `/Admin/Catalog` | CRUD, `GetBook`, `GetCopies`, dịch vụ catalog | Đang dùng; các action JSON phục vụ hộp thoại vẫn cần thiết |
| Quản lý bạn đọc | `/Admin/Reader`, `/Admin/Reader/Details/{id}` | Danh sách, hồ sơ, cập nhật, mượn sách | Đang dùng; bỏ action trả sách cũ không còn được UI gọi |
| Nhập QLĐT | `/Admin/Reader/ImportQldt` | `GetImportPreview`, `ProcessImport` | Có UI nhưng xử lý chưa hoàn chỉnh; giữ và ghi giới hạn |
| Mượn trả | `/Admin/Circulation` và hộp thoại dùng chung | `SearchReaders`, `CreateBorrow`, `ProcessReturnOrCompensation` | Đang dùng; bỏ action trả sách cũ trùng đường xử lý |
| Quá hạn | `/Admin/Overdue` | `OverdueController`, `GetAllOverdueLoans` | Đang dùng; bỏ tham số `level` không thực hiện lọc |
| Mất / hỏng & bồi thường | `/Admin/Incident`, hộp thoại trả / bồi thường | `ProcessReturnOrCompensation`, phiếu thu, `ThanhToan`, `ViPham` | Đang dùng, giữ |
| Báo cáo và xuất Excel | `/Admin/Report`, `/Admin/Report/ExportExcel` | `ReportController`, `ReportService` | Đang dùng, giữ |
| Tài khoản và phân quyền | `/Admin/User` | `Index`, `AddStaff`, `UpdateRole`, filter phân quyền | Đang dùng; UI hiển thị theo vai trò |
| API và tài liệu kỹ thuật | `/ApiDoc`, `/swagger.json`, `/api/...` | `ApiDocController`, `ApiStaffAuthorize` | Có giao diện Swagger và bộ kiểm tra sử dụng; giữ |
| Đặt trước tài liệu | Không có form, action hoặc service tương ứng | Entity `DatTruoc` và ánh xạ EF | Đã gỡ khỏi mô hình ứng dụng |
| Gia hạn mượn | Không có form, action hoặc service tương ứng | Entity `GiaHan` và ánh xạ EF | Đã gỡ khỏi mô hình ứng dụng |
| Microsoft SSO | Form đăng nhập hiện tại không có nút Microsoft | Action chỉ trả thông báo chưa cấu hình | Đã gỡ action chưa triển khai |

Các mục quản trị bị ẩn đối với một vai trò không phải mã thừa: sidebar và filter phía máy chủ đang cùng kiểm soát quyền truy cập.

## 3. Những thay đổi đã thực hiện trong đợt rà soát

### 3.1. Gỡ cấu trúc đặt trước và gia hạn chưa được sử dụng

Đã xóa:

- `Models/Entities/DatTruoc.cs`.
- `Models/Entities/GiaHan.cs`.

Đã bỏ 2 `DbSet`, 2 khối cấu hình entity trong `Models/Entities/QLThuVienUtcContext.cs` và 4 navigation property tương ứng tại `BanDoc`, `TaiLieu`, `ChiTietMuon`, `NhanVien`.

Không tìm thấy nghiệp vụ controller/service, lời gọi UI hoặc kiểm tra nghiệp vụ sử dụng hai entity này. Mô hình EF còn 14 entity thay vì 16.

**Không chạy migration hoặc lệnh xóa bảng.** Các bảng `DatTruoc` và `GiaHan` trong database đang dùng và khai báo trong `QLThuVienUTC.sql` được giữ nguyên. Việc này chỉ thu gọn mô hình ứng dụng; nếu cần loại bỏ cả phần schema thiết kế, phải lập thay đổi database riêng sau khi kiểm tra dữ liệu và khóa ngoại.

### 3.2. Gỡ ViewModel và DTO cũ không có nơi sử dụng

| File / lớp đã gỡ | Bằng chứng và thay thế hiện tại |
| --- | --- |
| `Areas/Reader/ViewModels/HomeIndexViewModel.cs` | Không được controller tạo hoặc view khai báo. Gỡ cả `HomeIndexViewModel`, `BookCardItem`, `QuickLinkItem`, `NewsArticleItem`, `AnnouncementItem`, `TopicItem`, `ServiceShortcutItem` chỉ phục vụ cấu trúc này |
| `Areas/Reader/ViewModels/SearchIndexViewModel.cs` | Search hiện dùng `BookSearchViewModel` trong `Areas/Reader/Models/BookCatalogModels.cs` |
| `Models/LoginViewModel.cs` | Login hiện bind tham số action và render partial dùng `ViewBag` / dữ liệu form; không dùng lớp này |
| `ReaderRecord` trong `Areas/Admin/Services/ReaderService.cs` | Chỉ có khai báo; các luồng quản trị đang dùng `ReaderListItemViewModel` và `ReaderDetailViewModel` |

Đồng thời bỏ namespace import `BTL_LTWEB.Areas.Reader.ViewModels` khỏi `Areas/Reader/Views/_ViewImports.cshtml`.

Tổng cộng xóa **5 file C#**, loại **12 lớp không cần cho luồng hiện tại**, tính cả 2 entity ở mục 3.1. Số file C# ứng dụng giảm từ **62 xuống 57**.

Việc bỏ `RememberMe` trong ViewModel cũ không thay đổi chính sách cookie đang hoạt động. Action đăng nhập vẫn đặt cookie lưu phiên theo cấu hình hiện có; UI chưa có tùy chọn ghi nhớ đăng nhập.

### 3.3. Gỡ các route không được UI sử dụng

| Route | Trước thay đổi | Sau thay đổi |
| --- | --- | --- |
| `POST /Account/LoginWithMicrosoft` | Trả lại trang login và thông báo chưa cấu hình, không có nút gọi trong form hiện tại | `404`; bỏ action giả chức năng SSO |
| `GET /Home/Privacy` | Gọi `View()` nhưng repo không có view Privacy tương ứng | `404`; không còn action dẫn tới lỗi thiếu view |
| `POST /Admin/Circulation/ReturnBook` | Đường trả sách riêng không còn được form hoặc JavaScript hiện tại gọi | `404`; UI tiếp tục dùng `ProcessReturnOrCompensation` |
| `POST /Admin/Reader/ReturnBook` | Đường trả sách cũ không còn được form hoặc JavaScript hiện tại gọi | `404`; UI tiếp tục dùng hộp thoại trả / bồi thường dùng chung |

Hàm nghiệp vụ `IReaderService.ReturnBook` và endpoint `POST /api/circulation/return` vẫn được giữ: chúng còn được API đã công bố và bộ kiểm tra nghiệp vụ sử dụng. Việc gỡ hai action MVC không loại bỏ khả năng trả sách trên UI.

### 3.4. Thu gọn xử lý bị dư

- **Trang chủ bạn đọc:** bỏ dependency `IUserProfileService` và lời gọi `GetProfile` trong Reader `HomeController`. View chỉ dùng `User` để xác định vai trò và không đọc `Model`; bỏ khai báo model ở hai bản view trang chủ. Thông tin sách đang mượn và hồ sơ vẫn được tải tại `/Reader/Profile`.
- **Tổng quan quản trị:** Admin `HomeController.Index` chỉ redirect sang `DashboardController.Index`; bỏ dependency và đoạn tải dashboard trùng. URL cũ `/Admin/Home` vẫn hoạt động. MVC có thể tạo đích redirect là `/Admin` vì Dashboard là controller mặc định của area.
- **Danh sách tên sách theo bạn đọc:** bỏ `GetReaderBorrowedTitlesMap` khỏi `IReaderService` và `ReaderService`, do không có caller. Ô tìm bạn đọc hiện dùng `SearchBorrowReadersAsync` với truy vấn SQL giới hạn kết quả.
- **Mức độ quá hạn:** bỏ tham số `level`, thuộc tính `SelectedLevel`, hidden input và việc đưa `level` vào phân trang. Trước đó tham số được truyền qua lại nhưng không hề lọc kết quả; UI chỉ có ô tìm kiếm. Đã sửa cả hai bản view quá hạn và trường hợp kiểm tra tương ứng. Query cũ có thêm `level` được MVC bỏ qua.
- **Điền nhanh tài khoản demo:** bỏ listener `[data-login-username]` trong `wwwroot/js/utc-login.js`, vì các nút tương ứng đã được xóa ở lần chỉnh login trước. Nút hiện / ẩn mật khẩu vẫn giữ nguyên.

### 3.5. Cập nhật kiểm tra và tài liệu liên quan

- `tests/Smoke/check_security.py`: xác nhận các route đã gỡ trả `404`, tiếp tục kiểm tra quyền và anti-forgery cho API đang dùng.
- `tests/Smoke/check_circulation_pagination.py`: bỏ trường hợp yêu cầu giữ bộ lọc `level` không tồn tại trên UI.
- `README.md`: bỏ mô tả trang Privacy; thêm liên kết tới báo cáo này.
- `docs/API.md`: sửa đoạn mô tả cũ đang mâu thuẫn với mã nguồn về `ApiStaffAuthorize`, anti-forgery và hành vi không ghi mượn/trả dự phòng khi SQL lỗi.
- `docs/QA.md`: cập nhật trạng thái endpoint Microsoft đã được gỡ.
- `.gitignore`: bỏ qua `/.tmp/`, nơi chứa bản build và dữ liệu kiểm tra cục bộ.

## 4. Backend không có mục menu riêng nhưng vẫn cần giữ

### 4.1. Endpoint phục vụ tương tác UI

| Endpoint / xử lý | Nơi dùng |
| --- | --- |
| `GET /Admin/Circulation/SearchReaders` | Ô chọn bạn đọc, qua `wwwroot/js/circulation-borrow.js` |
| `GET /Admin/Catalog/GetBook/{id}` | Hộp thoại sửa học liệu |
| `GET /Admin/Catalog/GetCopies/{id}` | Hộp thoại danh sách bản sách |
| `POST /Admin/User/AddStaff`, `/Admin/User/UpdateRole` | Form / JavaScript quản lý cán bộ và vai trò |
| `POST /Admin/Circulation/ProcessReturnOrCompensation` | Hộp thoại dùng chung trên các trang nghiệp vụ |
| Fragment của `/Admin/Report`, `ExportExcel` | Phân trang AJAX và xuất báo cáo |
| `CatalogSqlBridge`, `LoanTermPolicy` | Đồng bộ tồn kho, đối chiếu sách SQL/catalog và kiểm tra học kỳ mượn |
| Filter xác thực, anti-forgery, băm mật khẩu | Được framework gọi khi xử lý request |

### 4.2. API được giữ dù không có trên thanh điều hướng nghiệp vụ

`ApiDocController` có 11 endpoint JSON, được mô tả trong Swagger và `docs/API.md`:

- `GET /api/system/health`.
- `GET /api/auth/profile`.
- `GET /api/catalog/books`.
- `GET /api/catalog/books/{id}`.
- `GET /api/catalog/books/{id}/copies`.
- `GET /api/circulation/transactions`.
- `POST /api/circulation/borrow`.
- `POST /api/circulation/return`.
- `GET /api/readers`.
- `GET /api/readers/{id}`.
- `GET /api/dashboard/stats`.

UI nghiệp vụ chủ yếu dùng action MVC; Swagger có giao diện gọi thử các API này và các kiểm tra HTTP dùng một số API làm nguồn đối chiếu. Vì vậy, không xóa cả controller API chỉ dựa vào việc không có mục menu.

`ThanhToan`, phiếu thu, `ViPham` và các service tài chính còn tham gia trả sách / bồi thường và số liệu tổng quan. Chúng không phải cấu trúc thừa chỉ vì chưa có trang mang tên “Thanh toán”.

## 5. Phần được phát hiện nhưng chưa loại bỏ

| Hạng mục | Hiện trạng / lý do giữ |
| --- | --- |
| Nhập QLĐT | Có màn hình thật. `GetImportPreview` đọc tối đa 15 bạn đọc hiện có; `ProcessImport` chỉ đếm bạn đọc, trả `insertedCount = 0` rồi báo thành công. Chưa đọc Excel hoặc kết nối QLĐT. Cần triển khai nhập thật hoặc điều chỉnh thông báo ở một thay đổi chức năng riêng |
| Tin tức và dịch vụ | Nội dung viết trực tiếp trong Razor; chưa có CRUD tin tức / dịch vụ để đánh giá là backend thừa |
| Email nhắc trả, Microsoft SSO | Chưa có tích hợp hoạt động. Endpoint SSO mẫu đã gỡ; không coi nội dung nhắc tới tính năng trong view cũ hoặc comment là một chức năng đã triển khai |
| `/Reader/MyLibrary` và các action lịch sử / thông báo / hồ sơ | Hiện chỉ redirect về `/Reader/Profile`; không phải các màn hình nghiệp vụ độc lập. Giữ tương thích URL cũ và bộ kiểm tra hiện có |
| View trùng ở `Views` và `Areas/.../Views` | `Program.cs` cấu hình nhiều vị trí tìm view; một số action chỉ định đường dẫn view trực tiếp. Không xóa hàng loạt trước khi chuẩn hóa cơ chế tìm view. Các view legacy còn nhắc tới tính năng cũ không chứng minh chức năng ấy đang hoạt động |
| `Views/Home/Login.cshtml`, `wwwroot/preview/*` | Trang login cũ và bản xem trước tĩnh; form login đang hoạt động dùng `Views/Account/Login.cshtml` và partial `_LoginContent`. Các bản này không phải một cơ chế xác thực backend khác |
| `App_Data/readers_data.json` | Không tìm thấy nơi đọc theo tên file trong mã hiện tại. Giữ dữ liệu hiện có; ghi nhận là dữ liệu legacy cần xem xét lưu trữ riêng |
| Bảng SQL `DatTruoc`, `GiaHan` | Giữ nguyên schema và dữ liệu; chỉ đã bỏ cấu trúc EF chưa được ứng dụng sử dụng |

Các thuộc tính quan hệ như `Lop`, `TheLoai` vẫn cần cho truy vấn và ánh xạ EF dù tên `DbSet` không xuất hiện trong lệnh truy vấn trực tiếp.

## 6. Kiểm tra đã thực hiện

Kiểm tra HTTP trên **bản build mới tại `http://localhost:5137`**, không dựa vào tiến trình cũ ở cổng 5124. Runtime kiểm tra dùng bản sao `App_Data` trong `.tmp/backend-audit-runtime`; tài nguyên tĩnh lấy từ `wwwroot` của repo.

| Kiểm tra | Kết quả |
| --- | --- |
| Build ứng dụng và hai project kiểm tra SQL | Thành công, 0 lỗi, 0 cảnh báo |
| `check_pages.py` | Đạt 53 lượt render qua 3 phiên khách / sinh viên / admin; 118 tài nguyên nội bộ; 548 đoạn JavaScript nội tuyến |
| `check_security.py` | Đạt kiểm tra quyền API, anti-forgery, dữ liệu không hợp lệ, route đã gỡ trả `404` |
| `check_catalog.py` | Đạt 14 tổ hợp tìm kiếm / bộ lọc; 90 học liệu qua 8 trang; giữ liên kết thao tác và dữ liệu hộp thoại |
| `check_circulation_pagination.py` | Đạt đối chiếu 49 giao dịch / 5 trang và 14 lượt quá hạn / 2 trang, bộ lọc và tổng phạt |
| `check_reader_pagination.py` | Đạt đối chiếu 69 bạn đọc; phân trang hai danh sách độc lập và các bộ lọc |
| `tests/Reports/check_report.py` | Đạt báo cáo HTML / fragment / Excel, tìm kiếm, khoảng ngày, trang ngoài phạm vi và ngày không hợp lệ |
| Hai bộ Node.js hiện có | 11/11 kiểm tra tìm bạn đọc và phân trang báo cáo đạt; script login hợp lệ cú pháp |
| `BorrowVerification` trên SQL thử riêng | Đạt mượn, trả, giới hạn mượn, học kỳ, chặn trùng, rollback lỗi SQL, khoản thu, tài khoản và vai trò |
| `ReportVerification` trên SQL thử riêng | Đạt tổng kho, trạng thái bản sách, thời gian, phân trang, xuất Excel và xử lý thiếu database |
| Kiểm tra bổ sung | Login biên dịch mới không còn khối hỗ trợ/demo/preview; `/Admin/Home` redirect tới Dashboard; form/phân trang quá hạn không còn tạo tham số `level` |
| Kiểm tra dữ liệu file | Hash SHA-256 của cả 4 file JSON gốc trong `App_Data` giữ nguyên so với đầu đợt rà soát |
| `git diff --check` | Đạt, không có lỗi khoảng trắng trong patch |

Hai bộ SQL tạo database kiểm tra riêng và đã tự xóa sau khi chạy. Không tạo phiếu mượn/trả hợp lệ, thêm nhân viên hoặc cập nhật vai trò trên database `QLThuVienUTC` đang dùng. Kiểm tra đăng nhập có thể cập nhật `LanDangNhapCuoi` theo hành vi đăng nhập bình thường.

Đây là kiểm tra mã nguồn, HTTP, JavaScript và nghiệp vụ SQL; chưa phải kiểm tra trực quan bố cục ở mọi kích thước màn hình.

### Chạy lại kiểm tra

Sau khi khởi động bản mã mới bằng profile `http` tại cổng 5124:

```powershell
dotnet build BTL_LTWEB.csproj --no-restore
python -X utf8 tests/Smoke/check_pages.py
python -X utf8 tests/Smoke/check_security.py
python -X utf8 tests/Smoke/check_catalog.py
python -X utf8 tests/Smoke/check_circulation_pagination.py
python -X utf8 tests/Smoke/check_reader_pagination.py
python -X utf8 tests/Reports/check_report.py
node --test tests/Borrowing/reader-search.test.cjs tests/Reports/pagination.test.cjs
```

Hướng dẫn chạy SQL kiểm tra riêng có tại `tests/Borrowing/README.md` và `docs/QA.md`.

## 7. Phân biệt với thay đổi đã có trước đợt rà soát

Workspace đã có các thay đổi thu gọn khung nội dung Reader về tối đa 1320px, đồng bộ nền trắng và bỏ khối hỗ trợ / tài khoản thử / xem trước quản trị ở login. Đợt này giữ các thay đổi ấy, đồng thời dọn listener điền nhanh tài khoản không còn dùng.

`App_Data/catalog_books.json` và `App_Data/student_profiles.json` đã có diff trước khi bắt đầu rà soát. Các diff đó không được tính là kết quả dọn backend; không hoàn tác hoặc ghi đè chúng.

Mã Razor và route được biên dịch cùng ứng dụng. Nếu trình duyệt vẫn mở tiến trình khởi động từ bản build cũ, chỉ refresh trang sẽ không cập nhật các thay đổi backend/Razor. Cần dừng và khởi động lại ứng dụng phát triển để sử dụng bản mới. Các file `.tmp` là sản phẩm kiểm tra cục bộ, đã được bỏ qua trong `.gitignore` và không thuộc mã nguồn cần đưa vào commit.

## 8. Bổ sung: đối chiếu SQL và sửa hộp thoại Bản sách

### 8.1. Số liệu đọc trực tiếp từ SQL và JSON

Đã chạy công cụ chỉ đọc `tests/SqlAudit` trên kết nối trong `appsettings.json` và `appsettings.Development.json`. Không gọi migration, khởi tạo hoặc ghi dữ liệu. Kết quả tại thời điểm kiểm tra:

| Thông số | SQL Server | JSON | Nhận xét |
| --- | ---: | ---: | --- |
| Đầu học liệu | 90 | 90 | Đối chiếu theo ISBN hoặc tên: tất cả 90 đầu học liệu ghép được duy nhất với SQL |
| Tổng bản sách | 301 | 240 | Hai nguồn lệch 61 bản |
| Bản sẵn có | 252 | 231 | Hai nguồn lệch 21 bản |
| Bạn đọc | 69 | Không dùng JSON làm nguồn chính cho danh sách quản trị | Đọc SQL |
| Tài khoản | 73 | Không dùng JSON để xác thực | Đọc SQL |
| Giao dịch đang mượn | 49 | Hồ sơ JSON có thể chứa danh sách đệm | Nghiệp vụ mượn/trả đọc và ghi SQL |
| Khoản thanh toán / phiếu thu | 3 thanh toán, 300.000 đ | 6 phiếu, 550.000 đ | Hai sổ không đồng nhất; phiếu JSON còn có dữ liệu lịch sử/mẫu và nghiệp vụ bồi thường bằng sách, không thể tự cộng nhập hai nguồn |

Có **31 đầu học liệu** lệch ít nhất một trong các thông tin: tổng bản, bản sẵn có hoặc tập mã vạch. Ví dụ học liệu catalog ID 1 có JSON 4 bản / 3 sẵn có, trong khi SQL có 6 bản / 4 sẵn có. ID 5 có JSON 4 bản / 4 sẵn có, SQL có 6 bản / 1 sẵn có. Khóa catalog và khóa SQL được đối chiếu theo dữ liệu, không giả định hai ID luôn trùng nhau.

API dashboard trên bản build mới trả: **90 đầu sách, 301 bản, 252 sẵn có, 49 đang mượn, 69 bạn đọc, tổng quỹ 550.000 đ**. Tổng quỹ đang lấy từ JSON; các con số kho/bạn đọc nêu trên lấy hoặc tính từ SQL.

### 8.2. Trạng thái kết nối theo từng nhóm dữ liệu

| Nhóm | Nguồn thực tế trong mã hiện tại | Đã hoàn toàn dùng SQL? |
| --- | --- | --- |
| Đăng nhập, tài khoản cán bộ và vai trò | `TaiKhoan`, `NhanVien`, các thao tác lưu EF | Có đối với xác thực và quản lý tài khoản |
| Danh sách bạn đọc quản trị | `BanDoc`, `Lop`, quan hệ phiếu/chi tiết mượn | Có đối với danh sách và giao dịch |
| Mượn, trả, bồi thường | `PhieuMuon`, `ChiTietMuon`, `BanSao`, `ThanhToan`, `ViPham`; sau commit mới cập nhật cache JSON | Giao dịch chính dùng SQL; metadata catalog và sổ phiếu thu vẫn có JSON |
| Quá hạn | Truy vấn SQL; số ngày và tiền phạt tính bằng code | Dữ liệu mượn dùng SQL; mức phạt 100.000 đ/cuốn là quy tắc cố định, chưa phải cấu hình SQL |
| Báo cáo, biểu đồ và Excel | `ReportService` truy vấn SQL và tính tổng trong transaction đọc | Có |
| Dashboard: tổng đầu sách, bản sách, bạn đọc | Đếm SQL; nếu lỗi thì code còn giữ số dự phòng 30 / 120 / 49 | Chưa xử lý minh bạch khi SQL lỗi |
| Dashboard: bản sẵn có | Tính `tổng bản - giao dịch đang mượn`; hiện trùng SQL 252 | Chưa đếm trực tiếp `BanSao.TrangThai`; có thể lệch nếu có bản mất/hỏng/trạng thái khác |
| Dashboard: mất chờ bồi thường | `MatChoBoiThuong = 0` trong service | Chưa nối số liệu thật |
| Tổng quỹ, danh sách phiếu thu, trang sự cố | Bộ nhớ nạp từ `financial_receipts.json`; có khởi tạo mẫu nếu thiếu file | Chưa |
| Tra cứu, chi tiết, thêm/sửa/xóa học liệu | `BookCatalogService` đọc/ghi `catalog_books.json` | Chưa; không thể kết luận CRUD này đã lưu SQL chỉ dựa vào comment “CSDL” |
| Danh sách bản sách trong hộp thoại | `CatalogController.GetCopies` trả `book.Copies` từ JSON | Chưa; sửa lỗi hiển thị không chuyển nguồn này sang SQL |
| Tồn kho tại quầy mượn / hồ sơ quản trị | `CatalogSqlBridge.RefreshAvailability` đối chiếu SQL; đồng bộ cache sau mượn/trả | Kết hợp, chưa thống nhất mọi trang |
| Hồ sơ bạn đọc | Định danh và sách đang mượn ưu tiên SQL; thông tin liên hệ còn ưu tiên giá trị JSON đã lưu | Kết hợp; lỗi SQL có thể rơi về hồ sơ đệm |
| Số tài khoản, ngân hàng, avatar | `student_profiles.json`; chưa có cột lưu tương ứng trong entity đang dùng | Chưa |
| CCCD và thông tin mặc định của hồ sơ | CCCD ghép từ mã sinh viên; ngày sinh có giá trị dự phòng khi SQL thiếu | Là dữ liệu suy diễn/mặc định, không phải tất cả đều đọc SQL |
| Nhập QLĐT | Xem SQL hiện có, nút xử lý chỉ đếm số bạn đọc | Chưa triển khai nhập thật |
| Nội dung trang chủ, tin tức, dịch vụ | Nội dung Razor tĩnh | Chưa có backend quản trị nội dung SQL |

**Kết luận:** kết nối SQL hoạt động, nhưng cấu hình connection string không đồng nghĩa toàn bộ website đã dùng SQL. Muốn thống nhất phải chuyển catalog/CRUD/bản sách, sổ thu và các trường hồ sơ bổ sung sang SQL, sau đó đối chiếu dữ liệu cũ trước khi bỏ JSON. Đợt này kiểm tra và ghi nhận các khác biệt; không tự nhập, gộp hoặc xóa dữ liệu hai nguồn.

### 8.3. Nguyên nhân và thay đổi sửa lỗi Bản sách

API `GetCopies` trả `200` và có danh sách bản sao. Tuy nhiên JavaScript gọi `document.getElementById("copiesModalBookTitle")`, trong khi markup cũ không có phần tử mang ID đó. Gán `textContent` vào `null` gây lỗi JavaScript và rơi vào nhánh `catch` chung, làm giao diện báo nhầm “Lỗi kết nối khi tải bản sao”. Con số 4/4 trong hình là nội dung mặc định viết sẵn, không chứng minh API đã tải được dữ liệu.

Đã sửa cả `Areas/Admin/Views/Catalog/Index.cshtml` và `Views/Catalog/Admin/Index.cshtml`:

- Thêm phần tử hiển thị tên học liệu đúng ID mà JavaScript sử dụng.
- Thay số mặc định 4/4 bằng trạng thái “Đang tải...”.
- Xóa tên/kệ/số liệu cũ trước mỗi lần tải, tránh giữ thông tin cuốn trước.
- Kiểm tra HTTP thất bại và chuyển hướng đăng nhập trước khi đọc JSON.

Thêm `tests/Smoke/check_copies_modal.py`: lấy HTML và dữ liệu API thực, chạy handler JavaScript với DOM mô phỏng từ các ID có trong HTML. Đã đạt 4 trường hợp: hiển thị thành công, danh sách rỗng, lỗi HTTP, hết phiên đăng nhập. Đây là kiểm tra hành vi JavaScript, không phải screenshot trình duyệt.

Build đạt 0 lỗi/cảnh báo. Kiểm tra lại 53 lượt render / 118 tài nguyên / 548 script và 14 tổ hợp lọc catalog đều đạt trên bản sửa mới ở cổng 5137.

### 8.4. Chạy lại đối chiếu

```powershell
dotnet build tests/SqlAudit/SqlAudit.csproj -p:UseAppHost=false -o .tmp/sql-audit-build
dotnet .tmp/sql-audit-build/SqlAudit.dll
python -X utf8 tests/Smoke/check_copies_modal.py
```

Công cụ SQL dùng kết nối phát triển và chỉ xuất số tổng hợp, ID học liệu lệch. Không xuất mật khẩu hoặc thông tin hồ sơ cá nhân. Nếu đang chạy app từ build trước, cần khởi động lại ứng dụng để Razor đã sửa có hiệu lực.

## 9. Triển khai SQL-only theo yêu cầu

### 9.1. Nguồn dữ liệu sau chuyển đổi

Đã thay thế các service dùng file, không còn đọc/ghi dữ liệu nghiệp vụ từ `App_Data` hoặc khởi tạo kho sách/phiếu thu/hồ sơ mẫu khi thiếu dữ liệu. SQL Server là nguồn chính thức; file JSON cũ được giữ nguyên để lưu trữ và bị loại khỏi nội dung publish. Không tự nhập số liệu JSON đang lệch vào SQL.

| Luồng | Cách xử lý mới |
| --- | --- |
| Tra cứu, danh mục Admin, API học liệu | `BookCatalogService` truy vấn `TaiLieu`, `BanSao`, `TheLoai`, `NhaXuatBan`, `TaiLieu_TacGia`, `TacGia` |
| Học liệu đáng chú ý / kệ sách trang chủ | Reader `HomeController` lấy sách từ service SQL, Razor render tên/tác giả/ID thực; bỏ các thẻ tên sách viết sẵn |
| Thêm học liệu | Lưu đầu sách, tác giả, nhà xuất bản, bản sao trong transaction SQL; ID do SQL cấp, không tự tạo ISBN giả |
| Sửa học liệu | Cập nhật metadata, liên kết tác giả và vị trí kệ trong SQL; giữ nguyên số bản, trạng thái và lịch sử mượn |
| Xóa học liệu | Chỉ xóa đầu sách không có lịch sử mượn và không có bản đang lưu hành; xóa trong transaction, bảo toàn tham chiếu SQL |
| Bản sách | `GetCopies` trả đúng khóa bản sao, mã vạch, vị trí, tình trạng, trạng thái và số lượng từ `BanSao` |
| Mượn | Tìm bằng `TaiLieu.MaTaiLieu`; không đối chiếu ID JSON, không tự sinh học liệu/bản sao khi xác nhận mượn |
| Trả / bồi thường | Ghi cập nhật giao dịch, trạng thái bản sách và phiếu thu `ThanhToan` trong cùng transaction SQL |
| Bồi thường bằng sách | Lưu được phiếu xử lý số tiền 0 trong SQL, không mất khỏi sổ do không có tiền mặt |
| Sổ thu, quỹ, trang sự cố | Đọc `ThanhToan`, liên kết bạn đọc và chi tiết mượn; bỏ danh sách tĩnh và phiếu thu mẫu trong bộ nhớ |
| Hồ sơ, liên hệ, ngân hàng | Đọc/ghi `BanDoc`; các giá trị trống trong SQL được giữ trống, không ghép CCCD/ngày sinh giả hoặc lấy tên từ danh sách sinh viên viết sẵn |
| Dashboard | Đếm tổng kho/bạn đọc từ SQL; bản sẵn có đếm theo `BanSao.TrangThai`, không lấy tổng trừ số đang mượn; bỏ số tổng dự phòng 30/120/49 |
| Lỗi SQL | Filter trả HTTP `503` cho lỗi database thay vì lấy JSON hoặc trả danh sách rỗng giả; các thao tác ghi trả lỗi khi transaction không thành công |

Xóa `CatalogSqlBridge` vì không còn hai nguồn kho cần đồng bộ. Các giá trị phục vụ phân loại được suy ra từ thể loại/ISBN/tên/mô tả SQL hiện có: `LV-` cho luận văn, `ISSN`/tạp chí cho ấn phẩm; 5 nhóm chuyên ngành tương ứng `TheLoai`. Mức phạt và học kỳ vẫn là quy tắc nghiệp vụ trong code, không phải dữ liệu JSON.

`appsettings.json` vẫn là file cấu hình connection string; JSON trong phản hồi HTTP/Swagger vẫn là định dạng truyền dữ liệu. Chúng không phải nơi lưu dữ liệu nghiệp vụ.

### 9.2. Schema và cập nhật database đang dùng

Đã cập nhật định nghĩa tạo bảng trong **`QLThuVienUTC.sql`**:

- `BanDoc`: thêm `SoCCCD`, `NganhHoc`, `SoTaiKhoanNganHang`, `TenNganHang`, `AvatarUrl`, đều nullable.
- `ThanhToan`: thêm `MaChiTietMuon`, `TienNopMuon`, `TienDenBu`, `LoaiKhoanThu`, `HinhThucXuLy`, `GhiChu`, đều nullable để giữ tương thích các khoản thu cũ.

Script **`scripts/sql/20261112_SqlOnlyStorage.sql`** chỉ `ADD` các cột còn thiếu trong transaction, có kiểm tra `COL_LENGTH` nên chạy lại an toàn. Đã áp dụng script này cho database `QLThuVienUTC` đang dùng. Không chạy lại toàn bộ script khởi tạo trên database có dữ liệu; không xóa bảng hoặc dữ liệu.

Các khoản thanh toán SQL cũ chưa có thông tin tách tiền phạt/bồi thường được hiển thị là “Khoản thu khác”, giữ đúng tổng tiền; không tự đoán hoặc bổ sung số tiền từ JSON. Hồ sơ ngân hàng trước đây chỉ có trong JSON không được tự nhập vào cột SQL mới; người dùng có thể lưu lại qua form để dữ liệu được ghi vào SQL.

### 9.3. Bản sách đã sửa

Giữ bản sửa thiếu phần tử `copiesModalBookTitle` ở cả hai bản Razor, xóa số 4/4 mặc định, reset trạng thái khi mở và kiểm tra HTTP/phiên đăng nhập. Bổ sung escape nội dung trước khi chèn hàng bản sách vào HTML. Các trạng thái khác “Có sẵn” hiển thị đúng chuỗi trạng thái từ SQL, không gán tất cả thành “Đang mượn”.

Ví dụ trước chuyển đổi học liệu ID 1 hiển thị JSON 4 bản / 3 sẵn có; bản mới trả đúng SQL **6 bản / 4 sẵn có** và đúng ID/mã vạch của từng bản.

### 9.4. Số liệu đã xác nhận

Sau nâng cấp, công cụ `tests/SqlAudit` đọc SQL độc lập và đối chiếu với service:

- 90 đầu học liệu, 301 bản sách, 252 bản sẵn có.
- 69 bạn đọc, 49 lượt đang mượn.
- 3 khoản thanh toán, tổng **300.000 đ**, thay cho 6 phiếu / 550.000 đ của JSON cũ.
- Mọi đầu học liệu khớp số lượng và tập ID bản sao SQL.

### 9.5. Kiểm tra và chạy lại

- Build app và hai project kiểm tra SQL: 0 lỗi / cảnh báo.
- `BorrowVerification` đã chuyển sang seed SQL trong database thử riêng: kiểm tra CRUD và bản sao qua context mới, sửa tác giả lặp, giới hạn/trùng đầu sách, trả quá hạn, bồi thường bằng sách 0 đồng, hồ sơ ngân hàng/liên hệ, số liệu dashboard và rollback đồng thời phiếu thu/giao dịch khi lỗi SQL. Tất cả đạt; database thử đã tự xóa.
- `ReportVerification`: tổng kho, báo cáo thời gian, trạng thái bản sách, phân trang/Excel và thiếu database đều đạt.
- HTTP: 53 lượt render / 118 tài nguyên / 548 đoạn script; kiểm tra phân quyền/CSRF, 14 tổ hợp lọc catalog, phân trang bạn đọc/mượn trả/quá hạn và HTML/Excel đều đạt.
- Hộp thoại bản sách: 4 trường hợp thành công, rỗng, lỗi HTTP, hết phiên đều đạt. 11 kiểm tra Node.js hiện có đạt.
- Bộ kiểm tra catalog đã bỏ đọc file fixture JSON, lấy dữ liệu API SQL; `SqlAudit` đối chiếu service với bảng SQL trực tiếp.

Database mới: chạy `QLThuVienUTC.sql`. Database cũ: chỉ chạy script nâng cấp bổ sung. Có thể kiểm tra bằng:

```powershell
dotnet build tests/SqlAudit/SqlAudit.csproj -p:UseAppHost=false -o .tmp/sql-only-audit
dotnet .tmp/sql-only-audit/SqlAudit.dll --upgrade
dotnet .tmp/sql-only-audit/SqlAudit.dll
```

`--upgrade` là chế độ ghi schema được yêu cầu rõ ràng; khi không có tham số này công cụ chỉ đọc SQL. Không có mã nhập dữ liệu từ JSON.

Các trang tin tức/hướng dẫn vẫn là nội dung Razor tĩnh; nhập QLĐT vẫn chưa triển khai nhập file thật. Việc chuyển nguồn nghiệp vụ sang SQL không tự bổ sung một CMS hay hệ thống tích hợp QLĐT.

Đã khởi động lại tiến trình dev của repo tại **http://localhost:5124** bằng bản build cuối. Kiểm tra lại trên chính cổng này đạt: hộp thoại 4 trường hợp, 53 lượt render, phân quyền/CSRF; xác nhận API bản sách ID 1 là 6/4, dashboard 301/252 và quỹ 300.000 đ. Người dùng chỉ cần tải lại trang để nhận giao diện mới; không cần tự khởi động lại tiến trình cũ.

## 10. Rà soát giá trị mặc định khi thiếu dữ liệu trong controller

> Phần này lưu lại phát hiện ban đầu. Đợt sửa tiếp theo đã thực hiện, xem mục 11; không dùng các vị trí và đoạn mã cũ dưới đây để kết luận chúng vẫn còn trong source hiện tại.

### 10.1. Phạm vi và kết luận

Đã quét **21 file `*Controller.cs`** trong `Controllers`, `Areas/Admin/Controllers`, `Areas/Reader/Controllers`, rồi đọc các nhánh liên quan và đối chiếu service nhận dữ liệu. Trong các file này có **47 lần dùng `??`**, **38 lời gọi `IsNullOrWhiteSpace`**, **4 lời gọi `IsNullOrEmpty`**. Đồng thời kiểm tra toán tử điều kiện, nhánh mặc định của `switch`, giá trị mặc định của tham số và giới hạn phân trang.

Đây là số lượng mẫu cú pháp, **không phải số lỗi**: nhiều chỗ kiểm tra thiếu dữ liệu để từ chối, hoặc chỉ đặt mặc định cho bộ lọc. Các vị trí/lần xuất hiện lặp được gộp trong bảng bên dưới. Số dòng tính trên mã nguồn tại thời điểm rà soát, có thể thay đổi khi sửa code sau này.

**Kết luận: vẫn còn mặc định tạo dữ liệu thay thế, kể cả sau chuyển nguồn sang SQL.** Quan trọng nhất là mã sinh viên mẫu trong `ProfileController`, số tiền bồi thường 150.000đ và các thông tin học liệu tự điền trước khi ghi SQL.

Lượt này **chỉ bổ sung báo cáo**, không sửa controller/service, không thay đổi database và không gửi yêu cầu ghi nghiệp vụ để thử các nhánh này. Những ảnh hưởng dưới đây được suy ra từ luồng mã, không phải khẳng định đã có giao dịch sai trên dữ liệu đang dùng.

### 10.2. Những vị trí cần ưu tiên xử lý

| Ưu tiên | Vị trí | Điều kiện và giá trị thay thế | Ảnh hưởng / đề xuất |
| --- | --- | --- | --- |
| Cao | `Areas/Reader/Controllers/ProfileController.cs:22,34` — GET và POST `Index` | Thiếu claim `NameIdentifier` → `"242630923"` | Vẫn dùng mã mẫu để tìm hồ sơ SQL. Nếu có phiên xác thực nhưng thiếu claim này, GET có thể đọc và POST có thể cập nhật hồ sơ của mã mẫu. Nên từ chối phiên thiếu định danh hoặc yêu cầu đăng nhập lại, không chọn một người khác |
| Thấp, nhưng cần dọn | `Areas/Reader/Controllers/ProfileController.cs:23,35` | `User.Identity?.Name == null` → `"Đỗ Văn Đạt"` | Chuỗi mẫu vẫn còn ở controller. Tuy nhiên `UserProfileService.GetProfile(..., fallbackName)` hiện **không dùng** tham số `fallbackName`; họ tên trả về lấy từ `BanDoc.HoTen`. Vì vậy hiện tại tên mẫu này không ghi đè họ tên SQL. Nên bỏ tham số/biến thừa, phân biệt với lỗi mã sinh viên ở dòng trên |
| Cao | `Areas/Admin/Controllers/CirculationController.cs:129` | `compensationAmount == null` → `150000m` | Giá trị được truyền xuống service. Nếu chọn bồi thường bằng tiền, service chấp nhận số tiền dương này và có thể lưu `ThanhToan` 150.000đ dù request không cung cấp số tiền. Nên bắt buộc số tiền khi chọn tiền mặt; không thay thiếu dữ liệu bằng một khoản thu |
| Cao | `Areas/Admin/Controllers/CirculationController.cs:127–128` | `actionType` trống → `"NormalReturn"`; `compensationType` trống → `"NewBook"` | Thiếu lựa chọn có thể chuyển thành nghiệp vụ trả thường hoặc đền sách mới. Nên kiểm tra giá trị thuộc danh sách hợp lệ, và yêu cầu đầy đủ lựa chọn tương ứng thay vì tự quyết định nghiệp vụ |
| Cao | `Areas/Admin/Controllers/UserController.cs:31,104,177` | Thiếu `NameIdentifier` → dùng `User.Identity.Name`; nếu tiếp tục thiếu → `""` | Chuỗi này được dùng cho `IsSuperAdmin`. Nhánh `""` không cấp quyền, nhưng dùng tên hiển thị thay định danh để xét tài khoản `admin` là không nên. Nên bắt buộc claim định danh hợp lệ, từ chối nếu thiếu |
| Trung bình | `Controllers/AccountController.cs:98` và nhánh `else` dòng 159 trở đi | Vai trò `null` → `"READER"`; vai trò không thuộc các nhánh Admin/QL bạn đọc/QL học liệu cũng đi vào nhánh Reader | Có thể che lỗi dữ liệu vai trò. Nên kiểm tra vai trò SQL nằm trong danh sách cho phép, thay vì coi thiếu/không nhận diện là bạn đọc. Không kết luận đây là tự nâng quyền Admin: nhánh này gán quyền Reader |
| Trung bình | `Areas/Admin/Controllers/UserController.cs:79,81` | `VaiTro == null` → `"ADMIN"`; `TrangThai == null` → `"Hoạt động"` | Đây là giá trị của ViewModel danh sách tài khoản, **không phải** câu lệnh cập nhật SQL hoặc trực tiếp cấp claim. Tuy nhiên UI có thể hiển thị sai quyền/trạng thái. Nên hiển thị “Chưa xác định” và không cho thao tác dựa vào giá trị thay thế |
| Trung bình | `Areas/Admin/Controllers/UserController.cs:56–57,78` | Thiếu mã nhân viên → tên đăng nhập nếu bắt đầu `NV`, nếu không → `"NV-QT01"`; thiếu họ tên → `"Quản trị viên Hệ thống"`; thiếu email → `"canbo@utc.edu.vn"` | Tạo thông tin cán bộ nhìn giống dữ liệu thật nhưng không lấy từ hồ sơ SQL. Các mặc định ở đây phục vụ danh sách, không tự ghi nhân viên mới. Nên dùng dấu “—”/“Chưa có hồ sơ”, giữ mã thật riêng |
| Trung bình | `Controllers/AccountController.cs:95` | Email SQL `null` → `$"{account.TenDangNhap}@utc.edu.vn"` | Tạo email suy diễn trong claim, không phải email đã xác nhận từ SQL; không ghi ngược vào cột email. Nên bỏ claim email khi chưa có email thực |
| Trung bình | `Controllers/AccountController.cs:103,123,142` | Thiếu hồ sơ/tên nhân viên → `"Quản trị viên Hệ thống"`, `"Cán bộ quản lý bạn đọc"`, `"Cán bộ quản lý tài liệu"` | Claim `Name` hiển thị chức danh như họ tên. Nên phân biệt tên tài khoản, họ tên thực và nhãn vai trò; không che việc thiếu hồ sơ liên kết |

Các biểu thức `??` trên chỉ thay **`null`**. Chúng không thay chuỗi `""` hoặc chỉ có khoảng trắng. Ví dụ claim tồn tại nhưng giá trị rỗng sẽ không tự đổi thành mã sinh viên mẫu; đó vẫn là đầu vào thiếu hợp lệ cần kiểm tra riêng.

Một số trường SQL như vai trò/trạng thái có ràng buộc không null trong schema. Do đó không phải nhánh nào trong bảng cũng xuất hiện với dữ liệu hiện tại; bảng ghi hành vi của mã khi điều kiện xảy ra, không khẳng định cột đang chứa null.

### 10.3. Mặc định học liệu có thể đi vào dữ liệu SQL

Nguồn lưu đã là SQL, nhưng controller vẫn có thể tự tạo giá trị trước khi gọi `BookCatalogService`.

| Vị trí trong `Areas/Admin/Controllers/CatalogController.cs` | Mặc định hiện có | Ảnh hưởng |
| --- | --- | --- |
| `Create:133` | Nhà xuất bản `null`/rỗng/khoảng trắng → `"NXB Đại học Giao thông Vận tải"` | Service có thể tìm hoặc tạo nhà xuất bản với tên này rồi liên kết học liệu; thiếu dữ liệu trở thành thông tin xuất bản cụ thể |
| `Create:134`, `Update:192` | `major == null` → `"cntt"` | Service chọn thể loại SQL theo mã này; có thể gán CNTT dù người dùng chưa chọn ngành |
| `Create:119–126`, `Update:176–183` | Nhánh `switch` không nhận diện mã ngành → nhãn `"Công nghệ thông tin"` | Đây là mặc định nhãn. Cần lưu ý service hiện tra thể loại theo `Major`, không dùng `MajorName` để lưu; mã ngành không hợp lệ không có nghĩa chắc chắn SQL được gán CNTT. Nên từ chối mã ngành ngoài danh sách |
| `Create:138` | `year <= 0` → năm hiện tại | Có thể lưu năm xuất bản tự đoán |
| `Create:139` | `pages <= 0` → `250` | Có thể lưu số trang không có căn cứ |
| `Create:141` | Kệ `null`/rỗng/khoảng trắng → `"Kệ IT-01 - Tầng 2 A8"` | Các bản sách mới được lưu vào vị trí kệ tự điền |
| `Update:197` | Kệ `null` → `"Kệ IT-01"` | Service cập nhật vị trí của tất cả bản sách thuộc đầu học liệu. Nếu chuỗi chỉ có khoảng trắng, `Trim()` thành `""` nhưng `??` không kích hoạt; có thể xóa trống vị trí thay vì dùng mặc định |
| `Create:147` | `copiesCount <= 0` → `4` | Có thể tạo 4 bản sách SQL dù request thiếu/sai số lượng. Nên bắt buộc số lượng hợp lệ hoặc quy định rõ cho phép 0 |
| `Create:142`, `Index:88` | Bìa trống → `/images/covers/sach_01.jpg` | Nhánh Create lưu đường dẫn vào SQL; nhánh Index chỉ thay bìa hiển thị. Đây là bìa của một cuốn cụ thể, nên dùng ảnh placeholder chung nếu cho phép thiếu bìa |

Nên xác thực các trường nghiệp vụ bắt buộc và trả lỗi; với trường tùy chọn, lưu `NULL`/rỗng có chủ đích, không đoán nhà xuất bản, năm, số trang, vị trí hoặc số bản.

**Lưu ý model binding:** `Create`, `Update` và action xử lý bồi thường chưa có nhánh từ chối tổng quát `!ModelState.IsValid`. Một giá trị số gửi sai định dạng có thể để lại giá trị mặc định kiểu số hoặc `null` cùng lỗi binding. Cần kiểm tra ModelState trước khi áp dụng mặc định; việc form HTML có `required` không thay thế xác thực request trên server.

### 10.4. Các mặc định/chuẩn hóa thường hợp lý

| Vị trí | Hành vi | Đánh giá |
| --- | --- | --- |
| `CatalogController.cs:25–26,72–73`; `SearchController.cs:27–31` | Bộ lọc thiếu → `all`/“Tất cả”; từ khóa thiếu → chuỗi rỗng | Mặc định truy vấn/hiển thị, không tự tạo hồ sơ hoặc ghi học liệu. Nên thống nhất cách xử lý null/rỗng/khoảng trắng giữa bộ lọc |
| `CatalogController.cs:19,98`; `CirculationController.cs:28`; `OverdueController.cs:18`; `ReportController.cs:22`; `ApiDocController.cs:76–79` | Trang mặc định 1; kích thước trang mặc định 10; chặn số trang/kích thước ngoài khoảng | Mặc định phân trang hợp lý; không phải dữ liệu SQL bị làm giả |
| `CatalogController.cs:131–132,140,143,189–191,196,198–199` | Các chuỗi null chuyển thành rỗng | Cần xem từng trường: tên/tác giả rỗng được service `Valid` từ chối; ISBN trống được đổi về SQL null; mô tả/bìa/nhà xuất bản tùy chọn có thể để trống. Không phải tất cả chuỗi rỗng đều dẫn tới một bản ghi mẫu |
| `CatalogController.cs:94` | `Copies == null` → danh sách rỗng | Chuẩn hóa collection cho view; nguồn SQL hiện dựng danh sách bản sao. Không tạo thêm bản sao mặc định ở dòng này |
| `CirculationController.cs:62`; `IncidentController.cs:31` | Trạng thái/loại bộ lọc null → “Tất cả…” | Chỉ hiển thị và điều kiện lọc |
| `CirculationController.cs:97`; `ReaderController.cs:105` | Không tính được hạn trả cho thông báo → “theo phiếu mượn” | Chỉ là câu thông báo. Service kiểm tra học kỳ và ngày trả thực trước khi tạo phiếu; dòng này không lưu một ngày giả |
| `CirculationController.cs:155–159`; `AccountController.cs:114,133,152,171` | URL quay lại thiếu/không hợp lệ → điều hướng về trang mặc định | Fallback điều hướng phù hợp, có kiểm tra URL nội bộ |
| `ApiDocController.cs:30` | Không truyền TimeProvider → `TimeProvider.System` | Mặc định dependency đồng hồ cho môi trường chạy thật, không phải hồ sơ/nghiệp vụ mẫu |
| `ApiDocController.cs:68` | Phiên hiện tại thiếu `NameIdentifier` → dùng `User.Identity.Name` trong JSON thông tin phiên | Chỉ phản hồi thông tin phiên, nhưng nên thống nhất quy định định danh với các controller nghiệp vụ; không dùng tên hiển thị làm khóa SQL |
| `ApiDocController.cs:164–166` | Mã giao dịch null → chuỗi rỗng, sau đó trả `BadRequest` | Có từ chối giá trị thiếu; không tự chọn một giao dịch khác |
| `HomeController.cs:30` | Thiếu `Activity.Current.Id` → `HttpContext.TraceIdentifier` | Mã theo dõi request lỗi, hợp lý |
| `UserController.cs:63,155,161` | Nhãn vai trò không nhận diện → chuỗi vai trò hiện có/“Nhân viên”; thiếu tên nhân viên trong thông báo cập nhật → tên đăng nhập | Nhãn trình bày/thông báo; không tự cập nhật vai trò hoặc tạo hồ sơ |
| `UserController.cs:215–219` | Nhánh còn lại của chức vụ khi tạo nhân viên → “Cán bộ quản lý học liệu” | Trước đó đã kiểm tra `vaiTro` thuộc 3 giá trị cho phép; nhánh này xử lý vai trò quản lý tài liệu hợp lệ, không mặc nhiên nhận vai trò tùy ý |

`ReportController.cs:28` truyền ngày `null` vào service khi bộ lọc ngày không hợp lệ để dựng lại trang cùng thông báo lỗi; action đồng thời đặt HTTP `400`. Đây không phải chấp nhận bộ lọc lỗi như một request thành công.

Những chỗ chỉ gọi `IsNullOrWhiteSpace`/`IsNullOrEmpty` để trả lỗi, bỏ bộ lọc không được chọn hoặc kiểm tra quyền không được tính là “tự điền dữ liệu”. Ví dụ `AccountController.Login` từ chối username/password trắng, `UserController.AddStaff` từ chối mã/tên/email trắng, `ReaderController` từ chối mã bạn đọc hoặc sách không hợp lệ.

### 10.5. Điểm liên quan cần hiểu đúng

- **SQL-only vẫn có thể truy vấn nhầm người:** mã sinh viên mẫu trong controller không đọc JSON, nhưng dùng chính mã đó để tìm `BanDoc` thật. Đây là vấn đề định danh, không phải mất kết nối SQL.
- `UserProfileService.cs:15–22` tìm theo `studentId`, trả lỗi khi không có hồ sơ và lấy họ tên từ SQL. Điều này không ngăn controller truyền vào mã mẫu đã thay thế trước đó.
- `ReaderService.cs:303–307,356–363` kiểm tra loại xử lý và số tiền, rồi lưu SQL. Nhưng 150.000đ đã được controller tự điền là số dương hợp lệ đối với service; kiểm tra ở service hiện không biết số này có do người dùng nhập hay không.
- `AccountController.cs:59` thử tra cứu bằng phần trước `@` nếu lần đầu không tìm thấy tài khoản là một nhánh tra cứu bổ sung, không phải mặc định null. Mật khẩu và trạng thái vẫn được kiểm tra sau đó; không nên mô tả nhánh này là tự đăng nhập thành tài khoản mẫu.
- `UserController.AddStaff` còn chính sách mật khẩu khởi tạo từ tên và mã nhân viên (`:227–228` tại thời điểm quét). Đây là giá trị khởi tạo tài khoản theo code, không phải fallback khi null; cần đánh giá riêng nếu thay đổi chính sách cấp mật khẩu.
- `CatalogController.cs:91` tính “Đang mượn” bằng `Math.Max(0, tổng bản - bản sẵn có)`. Đây là số suy ra, không phải fallback null; bản mất/hỏng cũng có thể nằm ngoài số sẵn có, nên công thức có thể khác số đang mượn thực từ SQL. Dashboard đã đếm riêng trạng thái, nhưng ViewModel catalog vẫn còn công thức này.

### 10.6. Thứ tự xử lý đề xuất cho lượt sửa tiếp theo

1. Bỏ mã sinh viên mẫu; yêu cầu định danh hợp lệ cho GET/POST hồ sơ. Khi thiếu claim, từ chối xử lý và không gọi service bằng một mã khác.
2. Bắt buộc số tiền/lựa chọn xử lý khi bồi thường, kiểm tra ModelState; không mặc định thu 150.000đ hoặc tự chọn nghiệp vụ.
3. Xác thực vai trò và định danh tài khoản; không dùng tên hiển thị để xác định super admin. UI không thay thiếu vai trò/trạng thái bằng Admin/Hoạt động.
4. Bỏ các dữ liệu học liệu tự đoán có thể ghi vào SQL; dùng quy tắc rõ ràng cho trường bắt buộc/tùy chọn. Dùng ảnh bìa placeholder chung.
5. Giữ các mặc định phân trang, bộ lọc, collection và mã theo dõi lỗi hợp lý; thống nhất cách xử lý null/rỗng/khoảng trắng.

Các mục trên là **đề xuất, chưa được áp dụng trong lượt quét này**. Chỉ `doc.md` được bổ sung; không cần build hoặc khởi động lại ứng dụng cho thay đổi tài liệu này.

## 11. Đã sửa trực tiếp các giá trị mặc định không an toàn

### 11.1. ProfileController — bỏ hoàn toàn định danh mẫu

Đã sửa cả GET và POST trong `Areas/Reader/Controllers/ProfileController.cs`:

- Bỏ `?? "242630923"`, `?? "Đỗ Văn Đạt"` và biến tên dự phòng.
- Chỉ lấy `NameIdentifier` từ phiên đăng nhập. Thiếu, rỗng hoặc chỉ khoảng trắng trả **401 trước khi gọi service**, không truy vấn một mã sinh viên khác.
- Định danh hợp lệ nhưng không có hồ sơ SQL trả **404**, không tạo hồ sơ mẫu.
- POST dùng `ProfileContactInputModel` chỉ chứa liên hệ/ngân hàng; định danh và danh sách mượn không nhận từ form. Dữ liệu form không hợp lệ trả **400**.
- Bỏ tham số `fallbackName` khỏi cả interface và implementation `UserProfileService`. Service cũng từ chối mã bạn đọc trắng.

### 11.2. Các điểm tương tự đã dọn khi rà sâu

| Phần mã | Thay đổi đã áp dụng |
| --- | --- |
| `AccountController` | Không dựng email `username@utc.edu.vn`; không dùng tên/chức danh mẫu làm họ tên; không coi vai trò null/không hợp lệ là Reader; bỏ nhánh nâng quyền Admin chỉ vì username là `admin`. Vai trò phải thuộc danh sách cho phép và tài khoản phải có hồ sơ SQL hợp lệ. Claim định danh bạn đọc lấy mã sinh viên từ hồ sơ SQL liên kết |
| `UserController` | Thiếu `NameIdentifier` trả 401, không dùng tên hiển thị để xét super admin. Bỏ mã `NV-QT01`, email mẫu, tên quản trị mẫu và mặc định vai trò Admin/trạng thái Hoạt động trong danh sách. Dữ liệu chưa có hiển thị trống hoặc “Chưa xác định” |
| Các filter Reader/Admin/API cán bộ | Ngoài xác thực/vai trò, còn yêu cầu claim định danh không rỗng; tránh tiếp tục xử lý phiên thiếu định danh |
| `CirculationController` | Thiếu/sai loại xử lý hoặc phương thức bồi thường trả 400. Bồi thường bằng tiền bắt buộc số tiền hợp lệ; không còn tự điền 150.000đ. Có kiểm tra ModelState trước khi gọi nghiệp vụ |
| Hộp thoại trả/bồi thường ở cả `Views` và `Areas` | Bỏ giá trị 150.000đ trong input và JavaScript reset; phải nhập số tiền khi chọn hình thức tiền mặt. Giá trị 0 nội bộ chỉ dùng cho nghiệp vụ không thu tiền bồi thường, không thay một khoản tiền bị thiếu |
| `CatalogController` | Từ chối thiếu/sai ngành, năm xuất bản, số trang, kệ, số lượng bản; không đoán năm hiện tại/250 trang/4 bản/kệ IT/NXB. Nhà xuất bản/ISBN/mô tả/bìa tùy chọn có thể trống. Bìa hiển thị thiếu dùng placeholder chung |
| `BookCatalogService` | Xác thực metadata bắt buộc và số bản vật lý ngay tại service. Không phân loại ngành/ngôn ngữ chưa nhận diện thành Toán lý/Tiếng Việt. Trường không có dữ liệu giữ trống; trạng thái “Có sẵn” vẫn được xác định đúng theo trạng thái bản sách SQL |
| View thêm học liệu | Bỏ các giá trị tự điền cho nhà xuất bản, năm, số trang, kệ và số bản ở cả hai bản view; người dùng cung cấp dữ liệu thực |
| `ReaderService` — cán bộ xử lý | Không chọn nhân viên đầu tiên khi thiếu định danh, không dùng người lập phiếu cũ làm người xử lý hiện tại, không tự tạo nhân viên để bù hồ sơ thiếu. Phải có tài khoản cán bộ đang hoạt động, vai trò hợp lệ và nhân viên SQL liên kết |
| `ReaderService` — danh sách giao dịch | Bỏ mã/tên sinh viên mẫu khi phiếu thiếu bạn đọc liên kết; hiển thị thiếu dữ liệu rõ ràng, không gán sang người khác |
| `ReaderService` — hồ sơ và QLĐT | Bỏ lớp K65/hạn quyền năm 2028 giả định; dùng lớp và hạn thẻ SQL. Màn xem trước QLĐT hiển thị đúng dữ liệu SQL hiện có, không giả tên file đã nhập hoặc “đã đồng bộ”. Khi importer chưa triển khai, nút xử lý báo chưa cấu hình, không báo thành công bằng số bạn đọc đang có |
| Model/ViewModel và view cũ liên quan | Bỏ các số kho mẫu, năm/số trang/kệ/ngày nhập giả định và mã/tên sinh viên dự phòng. Các view legacy liên quan cũng được dọn để tránh đưa lại giá trị mẫu |
| API thông tin phiên | Thiếu định danh trả giá trị null trong thông tin phiên; không dùng tên hiển thị thay khóa định danh |

Số “Đang mượn” ở catalog đã chuyển sang đếm các bản có trạng thái SQL “Đang mượn”, không lấy tổng bản trừ sẵn có để tránh cộng nhầm bản mất/hỏng.

Các mặc định điều hướng, phân trang, bộ lọc “Tất cả”, collection rỗng và chuỗi rỗng của trường tùy chọn vẫn giữ: chúng không đại diện cho một người, khoản tiền hoặc bản sách được tự tạo. Các trạng thái khởi tạo có chủ đích như “Có sẵn” cho bản sách vừa nhập và “Hoạt động” cho tài khoản mới cũng là quy tắc tạo bản ghi, không phải cách che dữ liệu thiếu. Chính sách mật khẩu khởi tạo cán bộ không thay đổi trong đợt này.

### 11.3. Kiểm tra sau sửa

- Build ứng dụng và bộ kiểm tra: **0 lỗi, 0 cảnh báo**.
- Thêm `tests/Borrowing/ControllerInputChecks.cs`: kiểm tra GET/POST hồ sơ với claim không có/null/rỗng/khoảng trắng; xác nhận service có **0 lần đọc, 0 lần ghi** trong các trường hợp này. Kiểm tra POST hợp lệ dùng đúng định danh phiên và dữ liệu liên hệ lỗi không được ghi.
- Kiểm tra tên hiển thị `admin` không thay được claim định danh; action đổi quyền trả 401 nếu thiếu định danh.
- Kiểm tra thiếu/sai số tiền, hình thức trả/bồi thường, lỗi binding và thiếu metadata/số bản đều bị từ chối trước khi gọi service ghi.
- Kiểm tra SQL thử riêng: vai trò null/rỗng/khoảng trắng/không hợp lệ không đăng nhập thành Reader; thiếu định danh hoặc hồ sơ cán bộ không tự chọn/tạo nhân viên. CRUD, hạn mức mượn, trả/bồi thường, hồ sơ ngân hàng và rollback tiếp tục đạt. Database thử đã xóa sau khi chạy.
- HTTP trên **localhost:5124**: đăng nhập/CSRF/phân quyền, các request thiếu hoặc sai số tiền/số bản, 53 lượt render, 118 tài nguyên, 548 script, 14 tổ hợp lọc catalog, phân trang và báo cáo/Excel đều đạt. Hộp thoại Bản sách đạt 4 trường hợp; 11 kiểm tra Node.js đạt.
- Đối chiếu SQL độc lập vẫn khớp: 90 học liệu, 301 bản, 252 sẵn có, 49 đang mượn, quỹ 300.000đ.

Đã khởi động lại dev server của repo tại **http://localhost:5124** bằng bản vừa sửa. Lượt này không đổi schema SQL và không gửi thao tác nghiệp vụ hợp lệ lên database đang dùng; các thử nghiệm ghi chỉ chạy trên database riêng. Các file JSON cũ vẫn không được ứng dụng đọc/ghi.
