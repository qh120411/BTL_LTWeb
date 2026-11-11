- Danh mục học liệu, health và phiên đăng nhập được đọc công khai. Danh sách bạn đọc, hồ sơ, giao dịch và thống kê yêu cầu tài khoản cán bộ; chưa đăng nhập trả `401`, sai quyền trả `403`. POST mượn/trả còn yêu cầu cookie anti-forgery và header `RequestVerificationToken`; token lấy từ trang `/ApiDoc` hoặc một form cùng phiên. Swagger UI tự thêm header này. Phiên đăng nhập không cấp quyền cho bạn đọc tự mượn/trả qua API.

Tài liệu mô tả 11 endpoint JSON đang có trong `Controllers/ApiDocController.cs`, đối chiếu với các service và model hiện tại. Các route MVC trả về HTML không thuộc phạm vi này.

Ô chọn bạn đọc trong quầy mượn trả dùng endpoint riêng `GET /Admin/Circulation/SearchReaders?query=...`, yêu cầu phiên đăng nhập cán bộ có quyền vào trang mượn trả. Tên có/không dấu hoặc mã bạn đọc được lọc tại SQL, tối đa 10 kết quả; từ khóa sau khi bỏ khoảng trắng đầu/cuối phải dài 2–100 ký tự. Phản hồi `{ readers: [...] }` gồm `maBD`, `hoTen`, `lopKhoa`, `soSachDangMuon`, `gioiHanSach`, `borrowedTitles`; không cache. Đây là endpoint nội bộ của giao diện Admin, chưa đưa vào Swagger của `ApiDocController`.

## Chạy và mở tài liệu

```powershell
dotnet run --launch-profile http
```

- Base URL: `http://localhost:5124` (HTTPS: `https://localhost:7223` với profile `https`).
- Swagger UI: [http://localhost:5124/ApiDoc](http://localhost:5124/ApiDoc).
- Các đường dẫn tương đương: `/api-docs`, `/swagger`, `/swagger/index.html`, `/Docs/Api`.
- OpenAPI JSON: `/swagger.json`, nguồn trong [`wwwroot/swagger.json`](../wwwroot/swagger.json).
- Swagger UI tải CSS/JavaScript từ CDN `unpkg.com`, nên cần mạng để hiển thị. File OpenAPI vẫn được phục vụ từ ứng dụng.

## Quy ước và xác thực

- Request POST dùng `Content-Type: application/json`; phản hồi thành công dùng JSON, tên thuộc tính theo `camelCase`.
- `bookId`/`id` sách là số nguyên trong catalog; `maBD` thường là mã sinh viên dạng chuỗi, không phải khóa số `MaBanDoc` của SQL.
- `maGD` là mã giao dịch, ví dụ `GD261110-001`. Lấy mã thực tế từ danh sách giao dịch thay vì tự tạo.
- Các chuỗi ngày trong danh sách mượn trả thường có dạng `dd/MM/yyyy`; ngày trong hồ sơ fallback (`BorrowedBookItem`) được serialize theo ISO 8601. `health.time` có dạng `yyyy-MM-dd HH:mm:ss`, theo giờ máy chủ và không kèm múi giờ.
- Chưa có một envelope chung: danh sách thường trả `{ total, data }`, chi tiết sách trả `{ book, copies }`, thống kê trả trực tiếp đối tượng.
- Ứng dụng dùng cookie `UTC_Library_Auth_v3` để nhận diện phiên. Đăng nhập qua biểu mẫu `/Account/Login` với anti-forgery token; hiện chưa có endpoint đăng nhập JSON hoặc Bearer/JWT.
- **Các endpoint trong `ApiDocController` hiện chưa có kiểm tra đăng nhập/vai trò bằng `[Authorize]`; các POST mượn/trả cũng chưa có `[ValidateAntiForgeryToken]`.** Đăng nhập chỉ ảnh hưởng nội dung `/api/auth/profile`. OpenAPI không khai báo yêu cầu bảo mật cho những route này. Quyền truy cập các màn hình Admin không đồng nghĩa API được bảo vệ.
- `Try it out` cho POST thực sự ghi dữ liệu SQL/JSON. Dùng dữ liệu thử khi chạy các ví dụ mượn/trả.

## Danh sách endpoint

| Method | Đường dẫn | Chức năng |
| --- | --- | --- |
| GET | `/api/system/health` | Kiểm tra kết nối và số liệu tổng quát |
| GET | `/api/auth/profile` | Xem phiên đăng nhập hiện tại |
| GET | `/api/catalog/books` | Tìm kiếm, phân trang học liệu |
| GET | `/api/catalog/books/{id}` | Chi tiết học liệu và bản sao |
| GET | `/api/catalog/books/{id}/copies` | Danh sách bản sao |
| GET | `/api/circulation/transactions` | Các giao dịch đang mượn |
| POST | `/api/circulation/borrow` | Mượn một đầu sách |
| POST | `/api/circulation/return` | Trả một giao dịch |
| GET | `/api/readers` | Danh sách bạn đọc |
| GET | `/api/readers/{id}` | Chi tiết bạn đọc |
| GET | `/api/dashboard/stats` | Thống kê tổng hợp |

## 1. Kiểm tra hệ thống

`GET /api/system/health` — không có tham số. Ví dụ phản hồi `200`:

```json
{
  "status": "Healthy",
  "time": "2026-11-10 10:00:00",
  "databaseConnected": true,
  "totalBooks": 30,
  "totalReaders": 50,
  "activeLoans": 18
}
```

`status` hiện luôn là `Healthy` nếu action hoàn tất, kể cả khi `databaseConnected` là `false`. `totalBooks` lấy từ catalog, `totalReaders` và `activeLoans` lấy từ ReaderService; các số này có thể đến từ những nguồn khác nhau. Đây chưa phải readiness check trả `503` khi SQL mất kết nối.

## 2. Phiên đăng nhập

`GET /api/auth/profile` — luôn trả `200` cho hai trường hợp:

```json
{ "isAuthenticated": false, "user": null }
```

```json
{
  "isAuthenticated": true,
  "username": "tai-khoan-thu-nghiem",
  "fullName": "Bạn đọc thử nghiệm",
  "roles": ["Reader"]
}
```

`username` lấy từ claim `NameIdentifier`, nếu thiếu thì dùng `Identity.Name`; `fullName` lấy từ claim `Name`. `roles` là các claim vai trò không trùng nhau. Ví dụ chỉ minh họa cấu trúc, không phải tài khoản đăng nhập có sẵn.

## 3. Tìm kiếm học liệu

`GET /api/catalog/books`

| Query | Kiểu | Mặc định | Ý nghĩa |
| --- | --- | --- | --- |
| `keyword` | string | Không lọc | Tìm trong tiêu đề, tác giả, NXB, ISBN, mô tả và tên chuyên ngành; không phân biệt hoa/thường |
| `page` | integer | `1` | Trang cần lấy |
| `pageSize` | integer | `10` | Số phần tử mỗi trang |

```powershell
curl.exe "http://localhost:5124/api/catalog/books?keyword=Python&page=1&pageSize=10"
```

Ví dụ phản hồi `200` (dữ liệu minh họa):

```json
{
  "total": 1,
  "page": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 1,
      "title": "Cấu trúc dữ liệu và giải thuật",
      "author": "Bùi Việt Hà",
      "major": "cntt",
      "year": 2023,
      "shelf": "Kệ IT-01",
      "availableCopies": 3,
      "totalCopies": 4,
      "status": "Có sẵn"
    }
  ]
}
```

`total` là tổng kết quả trước phân trang; không có kết quả hoặc vượt trang trả `data: []`. `status` là `Có sẵn` khi `availableCopies > 0`, ngược lại là `Hết sách`. Tên trường tác giả tại đây là `author`, trong chi tiết sách là `authors`.

API chỉ nhận ba query trên. Server chuẩn hóa `page >= 1`, `pageSize` trong khoảng 1–100; metadata phản ánh giá trị sau chuẩn hóa. Offset dùng phép tính không tràn số.

## 4. Chi tiết học liệu

`GET /api/catalog/books/{id}` — `id` là integer.

- `200`: `{ "book": BookItem, "copies": BookCopyItem[] }`.
- `404`: `{ "message": "Không tìm thấy sách ID: 999999" }` nếu không có học liệu trong catalog.
- Route chỉ khớp `id` dạng số nguyên; chuỗi không hợp lệ không đi vào action.

`BookItem` gồm `id`, `title`, `authors`, `publisher`, `category`, `categoryName`, `major`, `majorName`, `language`, `languageName`, `year`, `pages`, `isbn`, `coverImage`, `availableCopies`, `totalCopies`, `shelf`, `description`, `isOnline`, `documentCode`, `stockBadgeText`, `copies`. Hai trường cuối trước `copies` là giá trị tính toán từ model. Cấu trúc và kiểu dữ liệu chi tiết có trong schema Swagger.

## 5. Bản sao học liệu

`GET /api/catalog/books/{id}/copies` — `id` là integer. Ví dụ `200`:

```json
{
  "bookId": 1,
  "copies": [
    {
      "id": 1,
      "bookId": 1,
      "barcode": "UTC-01-01",
      "shelf": "Kệ IT-01",
      "condition": "Tốt",
      "status": "Có sẵn",
      "importDate": "15/01/2024",
      "isAvailable": true
    }
  ]
}
```

Không có học liệu hoặc bản sao vẫn trả `200` với `copies: []`, khác với endpoint chi tiết sách. Danh sách lấy từ catalog JSON/bộ nhớ, không truy vấn trực tiếp bảng `BanSao`.

## 6. Giao dịch đang mượn

`GET /api/circulation/transactions?maBD=242630923`

`maBD` tùy chọn; bỏ trống để lấy tất cả, có giá trị thì trim và so khớp mã bạn đọc không phân biệt hoa/thường. Không phân trang. Phản hồi `200`:

```json
{
  "total": 1,
  "data": [
    {
      "maGD": "GD261110-001",
      "maBD": "242630923",
      "hoTen": "Bạn đọc thử nghiệm",
      "maBanSao": "UTC-01-01",
      "tenSach": "Cấu trúc dữ liệu và giải thuật",
      "tacGia": "Bùi Việt Hà",
      "coverImage": "/images/covers/sach_01.jpg",
      "ngayMuon": "10/11/2026",
      "hanTra": "31/12/2026",
      "trangThai": "Còn hạn",
      "isQuaHan": false
    }
  ]
}
```

Service chỉ lấy giao dịch chưa trả, loại các trạng thái `Đã trả`, `Đã đền sách mới`, `Đã đền tiền`. Đây không phải lịch sử toàn bộ mượn/trả. Khi truy vấn SQL lỗi, service trả danh sách rỗng; dữ liệu mượn fallback trong hồ sơ JSON không được hợp nhất vào endpoint này.

## 7. Mượn sách

`POST /api/circulation/borrow`

| Body | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `maBD` | string | Có | Không rỗng/khoảng trắng; được trim |
| `bookId` | integer | Có | Lớn hơn `0` |

Ví dụ PowerShell, có ghi dữ liệu:

```powershell
$body = @{ maBD = "242630923"; bookId = 1 } | ConvertTo-Json
# $session là WebRequestSession đã đăng nhập cán bộ; $token lấy từ form của cùng phiên.
Invoke-RestMethod -WebSession $session -Headers @{ RequestVerificationToken = $token } -Method Post -Uri "http://localhost:5124/api/circulation/borrow" -ContentType "application/json" -Body $body
```

Ví dụ `200`:

```json
{
  "success": true,
  "mode": "SQL_Server",
  "message": "Mượn thành công 'Cấu trúc dữ liệu và giải thuật' cho 242630923"
}
```

- `200`: `mode` là `SQL_Server`; giao dịch đã được ghi SQL.
- `400`: thiếu `maBD`/`bookId <= 0` trả `{ "message": "Vui lòng nhập maBD và bookId." }`; vượt giới hạn hoặc trùng đầu sách trả `{ "success": false, "message": "..." }`.
- `400`: sách không hợp lệ, hết bản sẵn có, học liệu điện tử hoặc mã bạn đọc không tồn tại. Không có nhánh ghi mượn dự phòng vào JSON.

Nhánh SQL giới hạn 5 cuốn cho sinh viên, 10 cuốn cho mã bắt đầu bằng `GV`/`CB`, chặn mượn trùng đầu sách, hạn trả cuối học kỳ (kỳ 1: mượn 11/08–31/12, hạn 31/12 cùng năm; kỳ 2: mượn 02/01–31/05, hạn 31/05 cùng năm; từ chối tạo phiếu mới ngoài hai khoảng này). Khi xác nhận mượn một đầu sách có trong catalog nhưng chưa có ở SQL, service ghi đầu sách và các bản sao theo tồn kho catalog vào SQL trong cùng transaction với phiếu mượn. Học liệu được đối chiếu theo ISBN/tên, không giả định ID JSON trùng khóa SQL. Nếu hết bản sẵn có, service từ chối mượn; không tự tăng tồn kho. Hồ sơ cán bộ còn thiếu được liên kết với tài khoản quản trị đang lập phiếu.

Mọi lỗi nghiệp vụ hoặc SQL đều từ chối tạo phiếu; không chuyển sang ghi hồ sơ JSON để bỏ qua kiểm tra. Không tự gửi lại POST sau timeout vì endpoint chưa có khóa idempotency.

## 8. Trả sách

`POST /api/circulation/return`

| Body | Kiểu | Bắt buộc | Quy tắc |
| --- | --- | --- | --- |
| `maBD` | string | Có | Không rỗng/khoảng trắng; được trim |
| `maGD` | string | Có | Mã giao dịch đang mượn của đúng bạn đọc; được trim |

```powershell
$body = @{ maBD = "242630923"; maGD = "GD261110-001" } | ConvertTo-Json
# $session là WebRequestSession đã đăng nhập cán bộ; $token lấy từ form của cùng phiên.
Invoke-RestMethod -WebSession $session -Headers @{ RequestVerificationToken = $token } -Method Post -Uri "http://localhost:5124/api/circulation/return" -ContentType "application/json" -Body $body
```

- `200`: `{ "success": true, "mode": "SQL_Server", "message": "..." }`.
- `400`: thiếu mã bạn đọc/giao dịch, giao dịch không thuộc bạn đọc, đã trả hoặc chưa lưu được.
- Không tự chọn cuốn khác khi mã không đúng. Trả lặp không thu phí lần nữa. Khoản thu dùng mã cán bộ thật và được lưu cùng giao dịch SQL; phiếu thu JSON chỉ ghi sau khi SQL thành công.

## 9. Danh sách bạn đọc

`GET /api/readers`

| Query | Kiểu | Ý nghĩa |
| --- | --- | --- |
| `keyword` | string, tùy chọn | Tìm theo mã, họ tên hoặc lớp/khoa; hỗ trợ tìm không dấu ở tên và lớp/khoa |
| `group` | string, tùy chọn | `Sinh viên`, `Giảng viên`; bỏ trống hoặc `Tất cả nhóm` để không lọc |

Không phân trang. `Ngoài trường` có trong danh sách lựa chọn của model, nhưng service hiện phân nhóm từ mã bạn đọc thành `Sinh viên` hoặc `Giảng viên`.

Phản hồi `200` là `{ "total": 1, "data": [ReaderListItem] }`, mỗi phần tử gồm `maBD`, `hoTen`, `nhom`, `lopKhoa`, `quyenSuDung`, `soSachDangMuon`, `soSachQuaHan`. Các bộ đếm là integer, những trường còn lại là string. SQL lỗi trả `total: 0`, `data: []`.

## 10. Chi tiết bạn đọc

`GET /api/readers/{id}` — ưu tiên truyền mã sinh viên; controller còn thử khóa số `MaBanDoc` nếu chuỗi là số, không tra cứu trực tiếp theo số thẻ `SoThe`.

- `200`, `mode: "SQL_Server"`: `data` là `ReaderDetailViewModel`, gồm `maBD`, `hoTen`, `lopKhoa`, `trangThaiHocTap`, `nhom`, `quyenSuDung`, `gioiHanSach`, `hieuLucQuyen`, `soSachDangMuon`, `soSachQuaHan`, `tongTienPhat`, `borrowTransactions`, `availableBooks`.
- `400` nếu `id` chỉ có khoảng trắng; `404` nếu không có bạn đọc. Không tạo hồ sơ mẫu cho mã không tồn tại.

Nhánh tra cứu khóa số vẫn có thể trả `500` nếu SQL mất kết nối. API không dùng hồ sơ JSON để khẳng định mã bạn đọc có tồn tại.

## 11. Thống kê dashboard

`GET /api/dashboard/stats` — không có tham số, trả `200` với `DashboardViewModel` trực tiếp:

| Trường | Kiểu | Ý nghĩa |
| --- | --- | --- |
| `tongDauSach`, `tongBanSao`, `tongBanDoc` | integer | Tổng học liệu, bản sao, bạn đọc |
| `banSanCo`, `dangMuon`, `matChoBoiThuong`, `luotQuaHan` | integer | Bộ đếm lưu thông; `matChoBoiThuong` hiện gán `0` |
| `tongQuyTaiChinh`, `tienThuNopMuon`, `tienThuDenBu` | number | Số tiền tổng hợp (VND) |
| `recentTransactions` | array | Tối đa 8 giao dịch từ danh sách đang mượn |
| `recentReceipts` | array | Phiếu thu từ service tài chính |

Nếu truy vấn đếm SQL lỗi, service giữ giá trị mặc định 30 đầu sách, 120 bản sao, 49 bạn đọc. Các số mặc định không phải dữ liệu thực được xác nhận. Chi tiết schema giao dịch và phiếu thu có trong Swagger.

## Xử lý lỗi và nguồn dữ liệu

| HTTP | Hành vi |
| --- | --- |
| `200` | Thành công theo action; một số danh sách trả rỗng khi SQL lỗi |
| `401` / `403` | Chưa đăng nhập cán bộ / không đủ quyền |
| `400` | Dữ liệu/nghiệp vụ không hợp lệ theo từng POST hoặc ID bạn đọc chỉ chứa khoảng trắng |
| `404` | Không khớp route hoặc không tìm thấy tài nguyên theo các action có kiểm tra |
| `415` | POST dùng kiểu nội dung không được JSON input formatter hỗ trợ |
| `500` | Lỗi chưa xử lý; không đảm bảo JSON, tùy môi trường Development/Production |

Controller kế thừa `Controller`, chưa có `[ApiController]` và không kiểm tra `ModelState` trong các POST. Vì vậy không nên coi mọi JSON sai kiểu/cú pháp là luôn được chuẩn hóa thành cùng một lỗi validation; các schema mô tả request hợp lệ và lỗi nghiệp vụ đã được action xử lý.

Catalog sử dụng `App_Data/catalog_books.json`/bộ nhớ và dữ liệu mẫu; ReaderService chủ yếu truy vấn SQL; hồ sơ dùng SQL kết hợp `App_Data/student_profiles.json`. Các đường đọc hiện có thể tạo/lưu cache hồ sơ hoặc khởi tạo dữ liệu service. Các tập dữ liệu này không được đảm bảo đồng bộ tuyệt đối. Tài liệu phản ánh hành vi đang có, không thay đổi quyền truy cập hoặc nghiệp vụ.

Khi thêm/sửa API, cập nhật đồng thời route/action, schema và ví dụ trong `wwwroot/swagger.json`, cùng tài liệu này. Đối chiếu tên trường với các model ở `Areas/Reader/Models` và `Areas/Admin/ViewModels`.
