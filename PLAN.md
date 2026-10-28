# PLAN.md — Kế hoạch Chi tiết 50 Task · UTC Library (ASP.NET Core MVC)

> **Quy ước Status:** `[ ]` Chưa làm · `[~]` Đang làm · `[x]` Đã xong  
> **Cập nhật:** Mỗi thành viên tự cập nhật cột Status trong file Excel hoặc trực tiếp tại đây.

---

## Sprint 1 — Tuần 1: Nền tảng Kiến trúc & Design System

> **Mục tiêu Sprint 1:** Solution chạy được, CSDL kết nối thành công, đăng nhập phân quyền hoạt động, layout Admin và Reader hoàn chỉnh.

---

### 👤 Dev 1 — Nguyễn (Team Lead & Backend Architect)

#### `[UTC-01]` 🔴 Cao · Sprint 1 · 12h
**Khởi tạo Solution ASP.NET Core 8 theo chuẩn N-Tier Architecture**

**Mô tả:**
Khởi tạo Solution phân tầng gồm 4 project:
- `UTC.Library.Web` — MVC Controllers, Razor Views, ViewModels
- `UTC.Library.Core` — Entities, Enums, Interfaces (không phụ thuộc Framework)
- `UTC.Library.Infrastructure` — EF Core DbContext, Repositories, Migrations
- `UTC.Library.Services` — Business Services, DTOs, AutoMapper Profiles

Cấu hình:
- Dependency Injection (IoC) đăng ký đầy đủ Services và Repositories
- AutoMapper 12+ cho ViewModel ↔ Entity mapping
- Serilog ghi log ra console + file rolling
- `appsettings.json` với connection string tới `QLThuVienUTC`

**Kết quả nghiệm thu:**
- `dotnet build` toàn solution không lỗi
- `dotnet run` khởi động ứng dụng trên `https://localhost:7xxx`
- Log Serilog xuất hiện trong console khi có request

**Phụ thuộc:** Không có (task đầu tiên)

---

#### `[UTC-02]` 🔴 Cao · Sprint 1 · 14h
**Scaffolding & Mapping EF Core DbContext từ SQL QLThuVienUTC**

**Mô tả:**
Ánh xạ 16 bảng từ `QLThuVienUTC_TheoBaoCao-1.sql` vào EF Core Fluent API:

```csharp
// Composite PK
modelBuilder.Entity<TaiLieu_TacGia>()
    .HasKey(x => new { x.MaTL, x.MaTG });

modelBuilder.Entity<GiaoDich_BanSao>()
    .HasKey(x => new { x.MaGD, x.MaBS });

// Unique constraint
modelBuilder.Entity<BanSao>()
    .HasIndex(x => x.MaBS).IsUnique();
```

Cấu hình quan hệ FK, Cascade Delete theo đúng SQL gốc. Tạo Migration `InitialCreate` và chạy `database update`.

**Kết quả nghiệm thu:**
- `dotnet ef migrations add InitialCreate` thành công, không lỗi
- `dotnet ef database update` tạo đủ 16 bảng trong SQL Server
- Kiểm tra bằng SQL Server Management Studio (SSMS): tất cả FK, Index, Constraint khớp với file SQL gốc

**Phụ thuộc:** `UTC-01`

---

#### `[UTC-03]` 🔴 Cao · Sprint 1 · 8h
**Xây dựng DbInitializer nạp dữ liệu mẫu từ SQL**

**Mô tả:**
Viết `DbInitializer.cs` tự động seed dữ liệu từ file SQL gốc khi chạy lần đầu:
- Quốc gia (VN, GB, US), Ngôn ngữ (VI, EN, FR)
- Thể loại, Định dạng, Vai trò (QTV, QLB, TTM)
- Nhân viên: `NV26-01` Nguyễn Thị Lan (QTV)
- Tài khoản: `TK26-01` / `admin.demo@utc.edu.vn` / password hash
- Bạn đọc: `231230001` Nguyễn Văn Huy, Thẻ `TBD231230001`
- Tài liệu mẫu `TLVN26-001`, Bản sao `TLVN26-001-001`

**Kết quả nghiệm thu:**
- Chạy app lần đầu → CSDL được seed tự động
- Đăng nhập bằng tài khoản demo thành công
- Tra cứu tài liệu mẫu hiển thị trên OPAC

**Phụ thuộc:** `UTC-02`

---

#### `[UTC-04]` 🔴 Cao · Sprint 1 · 8h
**Xây dựng Generic Repository, Unit of Work & Transaction Management**

**Mô tả:**
```csharp
public interface IGenericRepository<T> where T : class
{
    Task<T> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    IQueryable<T> Query();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TaiLieu> TaiLieu { get; }
    IGenericRepository<BanSao> BanSao { get; }
    IGenericRepository<GiaoDichMuonTra> GiaoDich { get; }
    // ... 16 repositories
    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
```

Xây dựng `ServiceResult<T>` chuẩn hóa kết quả: `IsSuccess`, `Message`, `Errors`, `Data`.

**Kết quả nghiệm thu:**
- Unit test `UnitOfWork` có thể thực hiện transaction: Begin → Insert → Rollback không để lại dữ liệu
- `ServiceResult<T>.Fail("error")` trả về `IsSuccess = false`

**Phụ thuộc:** `UTC-02`

---

#### `[UTC-05]` 🔴 Cao · Sprint 1 · 10h
**Cài đặt Cookie Authentication & Đăng nhập phân luồng Admin / Bạn đọc**

**Mô tả:**
```csharp
// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
```

Logic phân luồng sau đăng nhập:
- `MaVT ∈ {QTV, QLB, TTM}` → Redirect `/Admin/Dashboard`
- Bạn đọc (có `tTheBanDoc`) → Redirect `/Reader/MyLibrary/Current`

**Kết quả nghiệm thu:**
- Đăng nhập admin → chuyển sang `/Admin/Dashboard`
- Đăng nhập bạn đọc → chuyển sang `/Reader/MyLibrary/Current`
- Cookie `HttpOnly=true` xác nhận qua DevTools → Application → Cookies
- Truy cập `/Admin/*` khi chưa đăng nhập → redirect về `/Account/Login`

**Phụ thuộc:** `UTC-03`, `UTC-04`

---

### 👤 Dev 4 — Phạm (Lead Frontend Admin)

#### `[UTC-31]` 🔴 Cao · Sprint 1 · 12h
**Xây dựng Design System, Bảng màu UTC & Bộ thành phần dùng lại**

**Mô tả:**
Tạo file `wwwroot/css/utc-theme.css` với CSS Variables:
```css
:root {
  --utc-navy:    #0B3C68; /* Primary — Header, Sidebar */
  --utc-blue:    #1E5F99; /* Secondary — Hover, Link */
  --utc-gold:    #D99B26; /* Accent — Badge, Highlight */
  --utc-subtle:  #EBF3FA; /* Background card nhạt */
}
```
Component library tái sử dụng (Partial Views):
- `_StatusBadge.cshtml` — Trạng thái với màu conditional
- `_PaginationBar.cshtml` — Phân trang Bootstrap
- `_ConfirmModal.cshtml` — Modal xác nhận chung
- `_ToastNotification.cshtml` — Toast success/error/warning
- `_LoadingOverlay.cshtml` — Spinner overlay khi gửi AJAX

**Kết quả nghiệm thu:**
- Storybook-style: tạo trang `/Admin/UI/Components` render tất cả components
- Tất cả màu sắc khớp 100% với Figma (so sánh Figma Inspect ↔ Chrome DevTools)

**Phụ thuộc:** `UTC-01`

---

#### `[UTC-32]` 🔴 Cao · Sprint 1 · 10h
**Xây dựng Master Layout Quản trị Admin chuẩn Figma**

**Mô tả:**
Tạo `Views/Shared/_AdminLayout.cshtml`:
- Sidebar cố định trái với Navy `#0B3C68`, icon + label menu
- Navigation menu: Tổng quan, Tài liệu, Bản sao, Bạn đọc, Mượn/Trả, Quá hạn, Báo cáo, Cài đặt
- Header top với tên nhân viên đang đăng nhập + Avatar + nút Đăng xuất
- Breadcrumb tự động theo route
- Responsive: sidebar collapse thành icon-only trên tablet

**Kết quả nghiệm thu:**
- Sidebar hiển thị tên và vai trò nhân viên đăng nhập
- Active state menu item tự động highlight theo URL hiện tại
- Responsive kiểm tra tại width 768px (tablet)

**Phụ thuộc:** `UTC-31`, `UTC-05`

---

### 👤 Dev 5 — Hoàng (Reader Portal)

#### `[UTC-41]` 🔴 Cao · Sprint 1 · 12h
**Xây dựng Master Layout Reader Portal chuẩn nhận diện UTC**

**Mô tả:**
Tạo `Views/Shared/_ReaderLayout.cshtml`:
- Header UTC với logo Trường ĐHGTVT + thanh tìm kiếm nhanh tài liệu (OPAC)
- Navigation: Trang chủ, Tra cứu, Dịch vụ, Tin tức, Thư viện của tôi (hiện khi đã đăng nhập)
- Footer với thông tin liên hệ Thư viện UTC
- Palette: UTC Blue `#0B3C68` + UTC Gold `#D99B26` + White

**Kết quả nghiệm thu:**
- Header và Footer hiển thị đúng trên Chrome, Firefox, Edge
- "Thư viện của tôi" ẩn với khách vãng lai, hiển thị khi đã đăng nhập
- Thanh tìm kiếm header submit redirect đến `/Reader/Search?q={keyword}`

**Phụ thuộc:** `UTC-31`, `UTC-05`

---

## Sprint 2 — Tuần 2: Biên mục, Bạn đọc & OPAC

> **Mục tiêu Sprint 2:** Quản lý tài liệu đầy đủ CRUD, nhập Excel QLĐT, tra cứu OPAC thời gian thực, phân quyền RBAC hoạt động.

---

### 👤 Dev 1 — Nguyễn

#### `[UTC-06]` 🔴 Cao · Sprint 2 · 10h
**Xây dựng hệ thống Phân quyền RBAC theo vai trò SQL**

**Mô tả:**
Đọc `MaVT` từ `tVaiTro` và cài đặt Policy-based Authorization:
```csharp
builder.Services.AddAuthorization(options => {
    options.AddPolicy("QuanTriVien", p => p.RequireClaim("MaVT", "QTV"));
    options.AddPolicy("QuanLyBanDoc", p => p.RequireClaim("MaVT", "QLB"));
    options.AddPolicy("ThuThuMuonTra", p => p.RequireClaim("MaVT", "TTM"));
    options.AddPolicy("ThuVien_Staff", p =>
        p.RequireClaim("MaVT", "QTV", "QLB", "TTM"));
});
```

**Kết quả nghiệm thu:**
- `[Authorize(Policy="QuanTriVien")]` → TTM truy cập → 403 Forbidden
- QTV có thể xem trang `/Admin/Settings/Permissions`
- TTM chỉ thấy menu Mượn/Trả, ẩn menu Cài đặt

**Phụ thuộc:** `UTC-05`

---

#### `[UTC-07]` 🟡 Trung bình · Sprint 2 · 8h
**Backend Module Quản lý Tài khoản Cán bộ & Trạng thái tài khoản**

**Mô tả:**
CRUD `tTaiKhoan` + `tNhanVien`: Tạo tài khoản mới cho cán bộ, gán vai trò, kích hoạt/tạm khóa (`TrangThai`). Đặt lại mật khẩu với mã hóa BCrypt/PBKDF2.

**Kết quả nghiệm thu:**
- Tạo tài khoản mới → đăng nhập thành công với mật khẩu đã set
- Khóa tài khoản → đăng nhập → báo lỗi "Tài khoản đã bị tạm khóa"

**Phụ thuộc:** `UTC-06`

---

#### `[UTC-08]` 🔴 Cao · Sprint 2 · 12h
**Service Nhập Dữ Liệu Sinh Viên Từ File Excel QLĐT vào `tBanDoc`**

**Mô tả:**
Sử dụng ClosedXML đọc file `.xlsx` từ phòng Đào tạo:
```csharp
using (var workbook = new XLWorkbook(stream))
{
    var ws = workbook.Worksheet(1);
    foreach (var row in ws.RowsUsed().Skip(1))
    {
        var maSV = row.Cell(1).GetValue<string>(); // 231230001
        var hoTen = row.Cell(2).GetValue<string>();
        // Upsert vào tBanDoc, tạo tTheBanDoc nếu chưa có
    }
}
```
Validate: mã SV đúng format 9 ký tự, họ tên không rỗng. Báo cáo kết quả: bao nhiêu dòng thêm mới / cập nhật / lỗi.

**Kết quả nghiệm thu:**
- Upload file 100 sinh viên → import thành công trong < 3 giây
- File có dòng lỗi → hiển thị danh sách dòng lỗi kèm lý do
- Chạy import 2 lần → không tạo trùng bản ghi

**Phụ thuộc:** `UTC-04`

---

#### `[UTC-10]` 🟡 Trung bình · Sprint 2 · 8h
**Backend Module Quản lý Bạn Đọc, Hồ Sơ & Cập Nhật Trạng Thái Thẻ**

**Mô tả:**
`BanDocController`: Danh sách, tìm kiếm bạn đọc theo `MaBD`/Họ tên/Khoa. Xem hồ sơ chi tiết: thẻ `tTheBanDoc`, lịch sử giao dịch `tGiaoDichMuonTra`. Kích hoạt/Tạm khóa thẻ bạn đọc.

**Kết quả nghiệm thu:**
- Tìm kiếm `231230001` → hiển thị đúng hồ sơ
- Khóa thẻ `TBD231230001` → thẻ `TrangThai = 'Tạm khóa'` → không thể mượn sách

**Phụ thuộc:** `UTC-06`

---

### 👤 Dev 3 — Lê (Catalog & Report)

#### `[UTC-21]` 🔴 Cao · Sprint 2 · 10h
**Backend Module Quản lý Danh mục Tài Liệu & Bộ Lọc**

**Mô tả:**
`CatalogController`: Danh sách tài liệu với bộ lọc đa tiêu chí:
```csharp
query = query
    .Where(t => keyword == null || t.TenTL.Contains(keyword))
    .Where(t => maThL == null || t.MaThL == maThL)
    .Where(t => maNXB == null || t.MaNXB == maNXB)
    .Where(t => namXB == null || t.NamXB == namXB);
```
Hiển thị số bản sao còn lại: `COUNT(tBanSao WHERE TrangThai='Có sẵn')`.

**Kết quả nghiệm thu:**
- Bộ lọc kết hợp 3 tiêu chí → kết quả chính xác
- Số bản sao "Có sẵn" cập nhật realtime sau khi mượn/trả

**Phụ thuộc:** `UTC-02`, `UTC-06`

---

#### `[UTC-22]` 🔴 Cao · Sprint 2 · 8h
**Backend Service Thêm Mới / Chỉnh Sửa Tài Liệu & Gán Tác Giả**

**Mô tả:**
Form tạo mới `tTaiLieu` + gán nhiều tác giả (`tTaiLieu_TacGia`). Auto-generate `MaTL` theo format `TLVN{YY}-{SEQ}`. Upload ảnh bìa lưu vào `wwwroot/uploads/covers/`. Validate bắt buộc: TenTL, MaNXB, NamXB, MaNN, MaThL.

**Kết quả nghiệm thu:**
- Tạo tài liệu với 2 tác giả → `tTaiLieu_TacGia` có 2 bản ghi
- Ảnh bìa upload → hiển thị preview ngay trên form
- Edit tài liệu → thay đổi lưu thành công, không mất tác giả cũ

**Phụ thuộc:** `UTC-21`

---

#### `[UTC-23]` 🔴 Cao · Sprint 2 · 8h
**Nghiệp Vụ Kiểm Tra Trùng Tài Liệu & Điều Hướng Nhập Thêm Bản Sao**

**Mô tả:**
Trước khi INSERT tài liệu mới, kiểm tra:
```csharp
var isDuplicate = await _db.TaiLieu
    .AnyAsync(t =>
        EF.Functions.Like(t.TenTL, $"%{model.TenTL}%") &&
        t.NamXB == model.NamXB &&
        t.MaNXB == model.MaNXB);
```
Nếu trùng → trả về JSON `{ isDuplicate: true, existingId: "TLVN26-001" }` → Frontend hiển thị Modal 19 · Cảnh báo.

**Kết quả nghiệm thu:**
- Nhập tài liệu đã có → xuất hiện modal cảnh báo với tên tài liệu trùng
- Chọn "Nhập thêm bản sao" → redirect đến form thêm bản sao với `MaTL` đã điền sẵn
- Chọn "Tiếp tục" → bỏ qua kiểm tra, lưu tài liệu mới

**Phụ thuộc:** `UTC-22`

---

#### `[UTC-24]` 🔴 Cao · Sprint 2 · 10h
**Backend Module Quản lý Từng Bản Sao Vật Lý & Nhập Bổ Sung**

**Mô tả:**
`CopyController`: CRUD `tBanSao`. Auto-generate `MaBS = MaTL + '-' + SEQ.PadLeft(3,'0')`. Nhập bổ sung hàng loạt: nhập số lượng bản sao → tạo N bản ghi với MaBS tự tăng. Hiển thị trạng thái: Có sẵn (xanh) / Đang mượn (xanh dương) / Mất (đỏ).

**Kết quả nghiệm thu:**
- Nhập 5 bản sao cho `TLVN26-001` → tạo `TLVN26-001-002` đến `TLVN26-001-006`
- Barcode MaBS unique constraint: không cho phép trùng

**Phụ thuộc:** `UTC-22`

---

### 👤 Dev 4 — Phạm (Admin Frontend)

#### `[UTC-33]` 🔴 Cao · Sprint 2 · 8h
**Cắt Giao Diện Razor View Dashboard Tổng Quan Admin**

Razor View `/Admin/Dashboard` mapping Figma `01 · Tổng quan`:
- 4 KPI cards: Tổng tài liệu, Bạn đọc đang mượn, Quá hạn, Đã xử lý hôm nay
- Biểu đồ lưu thông 7 ngày gần nhất (Chart.js)
- Danh sách 10 giao dịch gần nhất
- Cảnh báo nhanh: số sách sắp đến hạn trả

**Phụ thuộc:** `UTC-32`, `UTC-28`

---

#### `[UTC-34]` 🔴 Cao · Sprint 2 · 12h
**Cắt Giao Diện Quản Lý Học Liệu & Modal Cảnh Báo Trùng**

Mapping Figma `03 · Danh mục tài liệu` + `11 · Thêm tài liệu` + `19 · Cảnh báo trùng`:
- DataTable với bộ lọc inline AJAX (không reload trang)
- Form thêm/sửa tài liệu với multi-select tác giả (Select2)
- Modal overlay cảnh báo trùng với 2 CTA buttons

**Phụ thuộc:** `UTC-32`, `UTC-21`, `UTC-22`, `UTC-23`

---

#### `[UTC-35]` 🔴 Cao · Sprint 2 · 8h
**Cắt Giao Diện Quản Lý Bản Sao & Form Nhập Bổ Sung**

Mapping Figma `08 · Quản lý bản sao`:
- Danh sách bản sao theo tài liệu, filter theo trạng thái
- Form nhập số lượng bản sao bổ sung
- Inline edit trạng thái bản sao (AJAX PATCH)

**Phụ thuộc:** `UTC-32`, `UTC-24`

---

#### `[UTC-36]` 🔴 Cao · Sprint 2 · 12h
**Cắt Giao Diện Quản Lý Bạn Đọc & Trang Nhập Excel QLĐT**

Mapping Figma `02 · Quản lý bạn đọc` + `10 · Chi tiết bạn đọc`:
- DataTable bạn đọc + badge trạng thái thẻ
- Trang hồ sơ: info + lịch sử mượn + badge thẻ active/locked
- Form upload Excel drag-and-drop (QLĐT)
- Progress bar khi import + bảng kết quả (thành công / lỗi)

**Phụ thuộc:** `UTC-32`, `UTC-08`, `UTC-10`

---

### 👤 Dev 5 — Hoàng (Reader Portal)

#### `[UTC-42]` 🔴 Cao · Sprint 2 · 12h
**Phát Triển Trang Chủ Thư Viện UTC**

Mapping Figma `01 · Trang chủ Reader`:
- Hero banner UTC + Quick search OPAC
- Section "Tài liệu mới nhập" (lấy từ `tTaiLieu ORDER BY NgayNhap DESC LIMIT 8`)
- Section "Thể loại nổi bật" với icon categories
- News ticker thông báo thư viện

**Phụ thuộc:** `UTC-41`, `UTC-21`

---

#### `[UTC-43]` 🔴 Cao · Sprint 2 · 12h
**Phát Triển Trang Tra Cứu OPAC Đa Tiêu Chí Thời Gian Thực**

Mapping Figma `05 · Tìm kiếm`:
- Search bar debounce 300ms → AJAX call → cập nhật kết quả không reload
- Bộ lọc: Thể loại, Ngôn ngữ, Năm xuất bản, Nhà xuất bản, Trạng thái còn sách
- Card kết quả với ảnh bìa, tên, tác giả, NXB, badge "Còn sách" / "Hết sách"
- Pagination client-side hoặc server-side

**Phụ thuộc:** `UTC-41`, `UTC-21`

---

#### `[UTC-44]` 🔴 Cao · Sprint 2 · 10h
**Phát Triển Trang Chi Tiết Tài Liệu & Bảng Tình Trạng Bản Sao**

Mapping Figma `06 · Chi tiết tài liệu`:
- Thông tin đầy đủ: ảnh bìa, mô tả, tác giả, NXB, thể loại, ngôn ngữ
- Bảng danh sách bản sao với `MaBS` + trạng thái màu
- Số bản sao còn lại: `SELECT COUNT(*) WHERE TrangThai='Có sẵn'`

**Phụ thuộc:** `UTC-41`, `UTC-24`

---

## Sprint 3 — Tuần 3: Lưu thông Mượn/Trả & "Thư viện của tôi"

> **Mục tiêu Sprint 3:** Toàn bộ quy trình mượn/trả tại quầy hoạt động, cảnh báo barcode, xử lý quá hạn, bồi thường, và phân hệ "Thư viện của tôi" cho bạn đọc.

---

### 👤 Dev 2 — Trần (Circulation Backend)

#### `[UTC-11]` 🔴 Cao · Sprint 2→3 · 10h
**Backend Module Quản trị Giao Dịch Mượn Trả & Bộ Lọc Trạng Thái**

`CirculationController`: Danh sách giao dịch với filter `TrangThai` (Đang mượn / Đã trả / Quá hạn). Join `tGiaoDichMuonTra ↔ tTheBanDoc ↔ tBanDoc ↔ tGiaoDich_BanSao`. Tính số ngày quá hạn: `(DateTime.Now - NgayHenTra).Days`.

**Phụ thuộc:** `UTC-04`, `UTC-06`

---

#### `[UTC-12]` 🔴 Cao · Sprint 2 · 10h
**Service Lập Phiếu Mượn & Auto-load Thông Tin Thẻ Bạn Đọc**

```csharp
// Quét mã thẻ → load bạn đọc AJAX
[HttpGet] public async Task<IActionResult> GetReaderInfo(string maTBD)
{
    var the = await _db.TheBanDoc
        .Include(t => t.BanDoc)
        .Include(t => t.GiaoDichMuonTras.Where(g => g.TrangThai == "Đang mượn"))
        .FirstOrDefaultAsync(t => t.MaTBD == maTBD && t.TrangThai == "Hoạt động");
    // return JSON
}
```

Auto-generate `MaGD = "GD" + DateTime.Now.ToString("yyMMddHHmm") + SEQ`.

**Phụ thuộc:** `UTC-11`

---

#### `[UTC-13]` 🔴 Cao · Sprint 2 · 8h
**Validation Engine Kiểm Tra Điều Kiện Mượn Sách**

```csharp
public async Task<ServiceResult<bool>> ValidateBorrowConditionsAsync(string maTBD)
{
    var the = await GetTheAsync(maTBD);
    if (the == null) return ServiceResult<bool>.Fail("Không tìm thấy thẻ");
    if (the.TrangThai == "Tạm khóa") return ServiceResult<bool>.Fail("Thẻ đã bị tạm khóa");
    if (the.NgayHetHan < DateTime.Today) return ServiceResult<bool>.Fail("Thẻ đã hết hạn");
    var hasOverdue = await _db.GiaoDichMuonTra
        .AnyAsync(g => g.MaTBD == maTBD && g.TrangThai == "Đang mượn"
                    && g.NgayHenTra < DateTime.Today);
    if (hasOverdue) return ServiceResult<bool>.Fail("Có tài liệu quá hạn chưa trả");
    return ServiceResult<bool>.Success(true);
}
```

**Phụ thuộc:** `UTC-12`

---

#### `[UTC-14]` 🔴 Cao · Sprint 3 · 8h
**Nghiệp Vụ Chặn Mượn Sách Do Có Tài Liệu Quá Hạn Chưa Trả**

Khi `ValidateBorrowConditionsAsync` trả về lỗi quá hạn:
- Return JSON `{ blocked: true, reason: "overdue", overdueItems: [...] }`
- Frontend hiển thị Figma `17 · Từ chối mượn` với danh sách sách quá hạn
- Nút "Xử lý trả sách" → redirect sang form trả

**Phụ thuộc:** `UTC-13`

---

#### `[UTC-15]` 🔴 Cao · Sprint 3 · 10h
**Backend Service Ghi Nhận Trả Sách Tại Quầy & Tra Cứu Giao Dịch**

```csharp
[Transaction]
public async Task<ServiceResult<ReturnResult>> ProcessReturnAsync(string maTBD)
{
    var activeLoan = await _db.GiaoDichMuonTra
        .Include(g => g.GiaoDich_BanSao)
        .FirstOrDefaultAsync(g => g.MaTBD == maTBD && g.TrangThai == "Đang mượn");
    // Hiển thị danh sách MaBS cần trả để nhân viên quét barcode
}
```

**Phụ thuộc:** `UTC-12`

---

#### `[UTC-16]` 🔴 Cao · Sprint 3 · 10h
**Nghiệp Vụ Đối Chiếu Barcode & Cảnh Báo Trả Sai Bản Sao Vật Lý**

```csharp
public async Task<ServiceResult<ScanResult>> ScanReturnBarcodeAsync(string maGD, string maBsScan)
{
    var expected = await _db.GiaoDich_BanSao
        .Where(g => g.MaGD == maGD && !g.DaTra)
        .Select(g => g.MaBS).ToListAsync();

    if (!expected.Contains(maBsScan))
        return ServiceResult<ScanResult>.Fail($"Mã barcode {maBsScan} không thuộc phiếu mượn này!");

    return ServiceResult<ScanResult>.Success(new ScanResult { IsMatch = true, MaBS = maBsScan });
}
```

→ Frontend hiển thị Figma `18 · Cảnh báo trả sai bản sao`.

**Phụ thuộc:** `UTC-15`

---

#### `[UTC-17]` 🔴 Cao · Sprint 3 · 8h
**Xử Lý Hoàn Tất Trả Sách Thành Công & Cập Nhật Kho Realtime**

```csharp
// Trong transaction:
banSao.TrangThai = "Có sẵn";           // tBanSao
giaoDichBanSao.DaTra = true;            // tGiaoDich_BanSao
giaoDichBanSao.TinhTrang = 1;           // Bình thường
giaoDich.NgayTra = DateTime.Now;        // tGiaoDichMuonTra
giaoDich.TrangThai = "Đã trả";
await _uow.SaveChangesAsync();
```

**Phụ thuộc:** `UTC-16`

---

#### `[UTC-18]` 🔴 Cao · Sprint 3 · 8h
**Kiểm Soát Giao Dịch Đồng Thời (Concurrency Control)**

Cấu hình `[Timestamp]` / `RowVersion` trên `tBanSao` và `tGiaoDichMuonTra`. Bắt `DbUpdateConcurrencyException` → retry 3 lần → thông báo xung đột cho người dùng.

**Phụ thuộc:** `UTC-17`

---

#### `[UTC-19]` 🟡 Trung bình · Sprint 3 · 8h
**Background Worker Quét Sách Quá Hạn Định Kỳ**

```csharp
public class OverdueCheckerWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await _circulationService.MarkOverdueTransactionsAsync();
            await Task.Delay(TimeSpan.FromHours(1), ct);
        }
    }
}
```

Cập nhật `TrangThai = 'Quá hạn'` trong `tGiaoDichMuonTra` khi `NgayHenTra < DateTime.Today`.

**Phụ thuộc:** `UTC-17`

---

#### `[UTC-20]` 🟡 Trung bình · Sprint 3 · 6h
**Backend Service Truy Vấn Lịch Sử Mượn Trả Của Từng Bạn Đọc**

Query lịch sử theo `MaTBD`, join sang `tGiaoDich_BanSao ↔ tBanSao ↔ tTaiLieu`. Phân trang và lọc theo năm/trạng thái.

**Phụ thuộc:** `UTC-11`

---

### 👤 Dev 3 — Lê

#### `[UTC-25]` 🟡 Trung bình · Sprint 3 · 8h
**Backend Module Theo Dõi Sách Quá Hạn & Thao Tác Chuyển Tiếp**

Dashboard cảnh báo: danh sách giao dịch `TrangThai='Quá hạn'` kèm số ngày quá. Nút "Nhắc nhở" ghi log. Nút "Chuyển xử lý mất sách" → trigger flow bồi thường.

**Phụ thuộc:** `UTC-19`

---

#### `[UTC-26]` 🔴 Cao · Sprint 3 · 8h
**Backend Module Quản lý Sự Việc Mất / Hỏng & Ghi Nhận Vi Phạm**

Khi trả sách với `TinhTrang=0`:
- Tạo bản ghi sự việc trong bảng log (hoặc cột mở rộng)
- UPDATE `tBanSao.TrangThai = 'Mất'`
- UPDATE `tGiaoDich_BanSao.TinhTrang = 0`
- Redirect đến flow lập biên bản bồi thường

**Phụ thuộc:** `UTC-17`

---

#### `[UTC-27]` 🔴 Cao · Sprint 3 · 10h
**Backend Service Xử Lý Bồi Thường (Đền Tiền / Đền Sách)**

```csharp
public async Task<ServiceResult<Indemnity>> ProcessIndemnityAsync(IndemnityRequest req)
{
    // req.Type = "TienMat" | "DenSach"
    if (req.Type == "DenSach")
    {
        // Kiểm tra MaBS mới hợp lệ, thêm vào tBanSao với MaTL gốc
        newCopy.TrangThai = "Có sẵn";
    }
    // Đánh dấu giao dịch đã bồi thường xong
    giaoDich.TrangThai = "Đã bồi thường";
    // Cập nhật tBanSao.TrangThai cũ = 'Mất' (giữ nguyên)
}
```

**Phụ thuộc:** `UTC-26`

---

### 👤 Dev 4 — Phạm (Admin Frontend)

#### `[UTC-37]` 🔴 Cao · Sprint 3 · 10h
**Cắt Giao Diện Lập Phiếu Mượn & Màn Hình Từ Chối Quá Hạn**

Mapping Figma `12 · Lập phiếu mượn` + `17 · Từ chối quá hạn`:
- Input quét mã thẻ → AJAX auto-fill thông tin bạn đọc
- Danh sách bản sao đang mượn trong phiếu (thêm/xóa)
- Modal block-mượn với danh sách sách quá hạn

**Phụ thuộc:** `UTC-32`, `UTC-13`, `UTC-14`

---

#### `[UTC-38]` 🔴 Cao · Sprint 3 · 10h
**Cắt Giao Diện Ghi Nhận Trả Sách & Cảnh Báo Sai Mã Bản Sao**

Mapping Figma `13 · Nhận trả` + `18 · Cảnh báo sai bản sao`:
- Input quét barcode bản sao → AJAX validate → tick xanh / cảnh báo đỏ
- Nút hoàn tất từng bản sao + chọn tình trạng (Bình thường / Hỏng·Mất)
- Modal cảnh báo barcode sai với tên sách đúng + sai

**Phụ thuộc:** `UTC-32`, `UTC-16`, `UTC-17`

---

#### `[UTC-39]` 🔴 Cao · Sprint 3 · 10h
**Cắt Giao Diện Theo Dõi Quá Hạn, Mất Hỏng & Hồ Sơ Bồi Thường**

Mapping Figma `05 · Quá hạn`, `15 · Hồ sơ bồi thường`, `20 · Danh sách mất hỏng`, `21 · Hoàn tất bồi thường`:
- Bảng quá hạn với cột số ngày quá + badge màu đỏ tăng dần
- Form lập biên bản bồi thường
- Chọn hình thức: Đền tiền / Đền sách

**Phụ thuộc:** `UTC-32`, `UTC-25`, `UTC-27`

---

### 👤 Dev 5 — Hoàng (Reader Portal)

#### `[UTC-45]` 🔴 Cao · Sprint 3 · 10h
**Tab "Sách Đang Mượn" & Cảnh Báo Quá Hạn**

Mapping Figma `03 · Thư viện của tôi — Current`:
- Danh sách sách đang mượn của bạn đọc đăng nhập
- Badge cảnh báo "Sắp đến hạn" (còn ≤ 3 ngày) màu vàng
- Badge "Quá hạn X ngày" màu đỏ

**Phụ thuộc:** `UTC-41`, `UTC-20`

---

#### `[UTC-46]` 🟡 Trung bình · Sprint 3 · 8h
**Tab "Lịch Sử Mượn Trả Cá Nhân"**

Mapping Figma `08 · Lịch sử`:
- Timeline lịch sử tất cả giao dịch đã hoàn tất
- Filter theo năm, search theo tên tài liệu

**Phụ thuộc:** `UTC-41`, `UTC-20`

---

#### `[UTC-47]` 🟡 Trung bình · Sprint 3 · 8h
**Tab "Thông Báo Nợ Sách & Bồi Thường"**

Mapping Figma `09 · Thông báo`:
- Danh sách thông báo: quá hạn, bồi thường, gia hạn thẻ
- Badge số thông báo chưa đọc trên navigation

**Phụ thuộc:** `UTC-41`, `UTC-19`

---

#### `[UTC-48]` 🟡 Trung bình · Sprint 3 · 8h
**Tab "Thông Tin Bạn Đọc" & Đồng bộ QLĐT**

Mapping Figma `10 · Thông tin bạn đọc`:
- Thông tin cá nhân từ `tBanDoc` đồng bộ với QLĐT
- Thông tin thẻ: ngày cấp, ngày hết hạn, trạng thái
- Không cho phép tự chỉnh sửa (chỉ đọc)

**Phụ thuộc:** `UTC-41`, `UTC-10`

---

## Sprint 4 — Tuần 4: Báo cáo, Dashboard & Bảo mật

> **Mục tiêu Sprint 4:** Dashboard KPI hoạt động, xuất Excel báo cáo, trang nội dung Reader, cấu hình bảo mật hoàn chỉnh.

---

### 👤 Dev 1 — Nguyễn

#### `[UTC-09]` 🔴 Cao · Sprint 4 · 8h
**Cấu hình Bảo mật (CSRF, XSS, CSP) & Middleware Xử lý Lỗi**

```csharp
// CSRF: AntiForgeryToken trên tất cả POST forms
// CSP Header
app.Use(async (context, next) => {
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self' cdn.jsdelivr.net;");
    await next();
});
// Global error handling middleware
app.UseExceptionHandler("/Error/500");
app.UseStatusCodePagesWithRedirects("/Error/{0}");
```

**Phụ thuộc:** `UTC-05`

---

### 👤 Dev 3 — Lê

#### `[UTC-28]` 🔴 Cao · Sprint 4 · 8h
**Backend Service Thống Kê KPI Dashboard Admin**

```csharp
public async Task<DashboardKpiDto> GetDashboardKpiAsync()
{
    return new DashboardKpiDto {
        TongTaiLieu     = await _db.TaiLieu.CountAsync(),
        DangMuon        = await _db.BanSao.CountAsync(b => b.TrangThai == "Đang mượn"),
        QuaHan          = await _db.GiaoDichMuonTra.CountAsync(g =>
                            g.TrangThai == "Đang mượn" && g.NgayHenTra < DateTime.Today),
        XuLyHomNay      = await _db.GiaoDichMuonTra.CountAsync(g =>
                            g.NgayTra.HasValue &&
                            g.NgayTra.Value.Date == DateTime.Today),
    };
}
```

**Phụ thuộc:** `UTC-17`

---

#### `[UTC-29]` 🔴 Cao · Sprint 4 · 8h
**Backend Module Báo Cáo Thống Kê Lưu Thông Theo Kỳ**

Thống kê theo khoảng thời gian: Số lượt mượn/trả, Top 10 tài liệu được mượn nhiều nhất, Top 5 bạn đọc tích cực, Biểu đồ theo tháng.

**Phụ thuộc:** `UTC-28`

---

#### `[UTC-30]` 🟡 Trung bình · Sprint 4 · 8h
**Xuất Báo Cáo Thống Kê Ra File Excel (ClosedXML)**

```csharp
var wb = new XLWorkbook();
var ws = wb.Worksheets.Add("Báo cáo lưu thông");
ws.Cell("A1").Value = "BÁO CÁO LƯUTHÔNG TÀI LIỆU";
ws.Cell("A1").Style.Font.Bold = true;
// ... fill data
using var stream = new MemoryStream();
wb.SaveAs(stream);
return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCao.xlsx");
```

**Phụ thuộc:** `UTC-29`

---

### 👤 Dev 4 — Phạm

#### `[UTC-40]` 🟡 Trung bình · Sprint 4 · 8h
**Cắt Giao Diện Báo Cáo Thống Kê & Cài Đặt Phân Quyền**

Mapping Figma `07 · Báo cáo` + `06 · Cài đặt`:
- Date range picker cho kỳ báo cáo
- Chart.js biểu đồ cột lưu thông theo tháng
- Form cài đặt phân quyền chi tiết cho từng vai trò

**Phụ thuộc:** `UTC-32`, `UTC-29`, `UTC-06`

---

### 👤 Dev 5 — Hoàng

#### `[UTC-49]` 🔵 Thấp · Sprint 4 · 8h
**Trang Dịch vụ, Tin tức & Giới thiệu Sách UTC**

Mapping Figma `07 · Dịch vụ` + `11 · Tin tức`:
- Trang giới thiệu dịch vụ thư viện (nội dung tĩnh)
- Trang tin tức/hoạt động thư viện (có thể dùng CMS đơn giản)

**Phụ thuộc:** `UTC-41`

---

## Sprint 5 — Tuần 5: Kiểm thử & Bàn giao

---

### 👥 Toàn đội — Cả 5 Dev

#### `[UTC-50]` 🔴 Cao · Sprint 5 · 16h (3.2h/người)
**Kiểm Thử Tích Hợp E2E, UAT Đối Chiếu Figma 100% & Bàn Giao**

**Checklist E2E:**
- [ ] Đăng nhập 3 vai trò admin + bạn đọc
- [ ] Nhập Excel QLĐT 50 sinh viên → import thành công
- [ ] Thêm tài liệu mới → kiểm tra trùng → thêm bản sao
- [ ] Lập phiếu mượn → kiểm tra điều kiện → thành công
- [ ] Trả sách → quét barcode đúng → cập nhật kho
- [ ] Trả sách → quét barcode sai → modal cảnh báo hiển thị
- [ ] Sách quá hạn → chặn mượn mới → hiển thị modal
- [ ] Trả sách hỏng → lập biên bản → hoàn tất bồi thường
- [ ] Dashboard KPI cập nhật realtime sau từng giao dịch
- [ ] Xuất báo cáo Excel → file tải về hợp lệ mở được
- [ ] OPAC tìm kiếm đa tiêu chí → kết quả chính xác
- [ ] "Thư viện của tôi" hiển thị đúng lịch sử cá nhân

**UAT Figma:**
- [ ] Pixel-perfect so sánh từng màn hình với Figma (Dev 4 + Dev 5 dẫn)
- [ ] Kiểm tra responsive tablet/mobile

**Bàn giao:**
- [ ] README.md hoàn chỉnh
- [ ] Database script final
- [ ] Tài khoản demo đã seed
- [ ] Hướng dẫn deploy production

**Phụ thuộc:** Tất cả task Sprint 1–4 hoàn thành

---

## 📊 Tổng kết Khối lượng

| Sprint | Thời gian | Số Task | Tổng giờ |
|---|---|:---:|:---:|
| Sprint 1 | Tuần 1 | 10 | ~94h |
| Sprint 2 | Tuần 2 | 20 | ~182h |
| Sprint 3 | Tuần 3 | 14 | ~130h |
| Sprint 4 | Tuần 4 | 5 | ~40h |
| Sprint 5 | Tuần 5 | 1 | ~16h |
| **Tổng** | **5 tuần** | **50** | **~462h** |

---

*File Excel tương tác đầy đủ với dropdown trạng thái Chưa làm / Đang làm / Đã xong:*  
**`c:\Users\lemin\Downloads\Ke_hoach_Phan_chia_Task_UTC_Library.xlsx`**
