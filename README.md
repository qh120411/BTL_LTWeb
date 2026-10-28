# 📚 UTC Library — Hệ thống Quản lý Thư viện Đại học Giao thông Vận tải

> **Công nghệ:** ASP.NET Core 8 MVC · Entity Framework Core · SQL Server · Bootstrap 5 · ClosedXML  
> **Mô hình phát triển:** Agile Scrum — 5 Sprint × 1 tuần  
> **Nhân sự:** 5 Lập trình viên chuyên trách  
> **CSDL gốc:** `QLThuVienUTC_TheoBaoCao-1.sql` — 16 bảng  
> **Thiết kế gốc:** Figma UTC Library (Admin 21 màn hình · Reader Portal 11 màn hình)

---

## 📁 Cấu trúc Solution

```
UTCLibrary/
├── UTC.Library.Web/              ← ASP.NET Core MVC (Controllers, Views, wwwroot)
│   ├── Areas/
│   │   ├── Admin/                ← Khu vực quản trị (QTV, QLB, TTM)
│   │   └── Reader/               ← Cổng thông tin bạn đọc (OPAC)
│   ├── Controllers/
│   │   └── AccountController.cs  ← Đăng nhập / Đăng xuất / Phân luồng
│   ├── ViewModels/               ← DTO hiển thị cho Razor Views
│   ├── Views/
│   ├── wwwroot/
│   │   ├── css/utc-theme.css     ← Design System: Navy #0B3C68 + Gold #D99B26
│   │   └── js/
│   └── appsettings.json          ← Connection string → QLThuVienUTC
│
├── UTC.Library.Core/             ← Domain Layer (không phụ thuộc Framework)
│   ├── Entities/                 ← 16 Entity classes ánh xạ từ SQL
│   ├── Interfaces/               ← IGenericRepository<T>, IUnitOfWork, IService...
│   ├── Enums/                    ← TrangThaiTaiLieu, VaiTro, TinhTrangBanSao...
│   └── ServiceResult.cs          ← Chuẩn hóa kết quả IsSuccess / Errors / Data
│
├── UTC.Library.Infrastructure/   ← Data Access Layer
│   ├── Data/
│   │   ├── AppDbContext.cs        ← EF Core DbContext (16 DbSet<>)
│   │   ├── DbInitializer.cs       ← Seed dữ liệu mẫu từ SQL gốc
│   │   └── Migrations/
│   └── Repositories/             ← GenericRepository<T> + UnitOfWork
│
└── UTC.Library.Services/         ← Business Logic Layer
    ├── CatalogService.cs          ← Biên mục tài liệu, kiểm tra trùng
    ├── CirculationService.cs      ← Mượn/Trả, barcode, quá hạn
    ├── ReaderService.cs           ← Bạn đọc, thẻ, nhập Excel QLĐT
    ├── IndemnityService.cs        ← Bồi thường mất/hỏng
    ├── ReportService.cs           ← Thống kê, dashboard KPI
    └── BackgroundJobs/
        └── OverdueCheckerWorker.cs ← Quét sách quá hạn định kỳ (IHostedService)
```

---

## 🗄️ Cơ sở dữ liệu — 16 Bảng (QLThuVienUTC)

### Nhóm Danh mục
| Bảng | Khóa chính | Mô tả |
|---|---|---|
| `tQuocGia` | `MaQG` char(2) | Quốc gia tác giả/tài liệu |
| `tNgonNgu` | `MaNN` char(3) | Ngôn ngữ tài liệu |
| `tTheLoai` | `MaThL` char(6) | Thể loại / Chủ đề tài liệu |
| `tDinhDang` | `MaDD` char(5) | Định dạng (Sách in, PDF, CDROM…) |
| `tVaiTro` | `MaVT` char(3) | Vai trò: `QTV`, `QLB`, `TTM` |
| `tNhaXuatBan` | `MaNXB` char(6) | Nhà xuất bản |
| `tTacGia` | `MaTG` char(8) | Tác giả |

### Nhóm Nhân sự & Tài khoản
| Bảng | Khóa chính | Ghi chú |
|---|---|---|
| `tNhanVien` | `MaNV` char(7) | Cán bộ thư viện (NV26-01…) |
| `tTaiKhoan` | `MaTK` char(7) | FK: `MaNV`, `MaVT`; `MatKhau` hash |

### Nhóm Biên mục Tài liệu
| Bảng | Khóa chính | Ghi chú |
|---|---|---|
| `tTaiLieu` | `MaTL` char(10) | Format: `TLVN26-001` |
| `tTaiLieu_TacGia` | (`MaTL`, `MaTG`) | Composite PK — Quan hệ N-N |
| `tBanSao` | `MaBS` char(14) | Format: `TLVN26-001-001`; `TrangThai`: *Có sẵn / Đang mượn / Mất* |

### Nhóm Bạn đọc
| Bảng | Khóa chính | Ghi chú |
|---|---|---|
| `tBanDoc` | `MaBD` char(9) | Mã sinh viên UTC (231230001) |
| `tTheBanDoc` | `MaTBD` char(12) | FK: `MaBD`; `TrangThai`: *Hoạt động / Tạm khóa* |

### Nhóm Giao dịch Mượn/Trả
| Bảng | Khóa chính | Ghi chú |
|---|---|---|
| `tGiaoDichMuonTra` | `MaGD` char(12) | FK: `MaTBD`, `MaTK`; `NgayMuon`, `NgayHenTra`, `NgayTra`, `TrangThai` |
| `tGiaoDich_BanSao` | (`MaGD`, `MaBS`) | `TinhTrang` bit: 1=Bình thường / 0=Mất·Hỏng |

---

## 👥 Phân công 5 Thành viên

| Dev | Họ tên | Vai trò | Số Task | Tổng giờ |
|:---:|---|---|:---:|:---:|
| **Dev 1** | Nguyễn (Team Lead) | Kiến trúc hệ thống & Backend Core | 10 | ~98h |
| **Dev 2** | Trần | Senior Backend — Lưu thông Mượn/Trả | 10 | ~86h |
| **Dev 3** | Lê | Backend — Biên mục & Báo cáo | 10 | ~86h |
| **Dev 4** | Phạm | Lead Frontend — Admin Portal (21 màn hình) | 10 | ~98h |
| **Dev 5** | Hoàng | Frontend/Fullstack — Reader Portal OPAC | 10 | ~88h |

---

## 🚀 Hướng dẫn Cài đặt & Chạy dự án

### Yêu cầu môi trường
- .NET SDK 8.0+
- SQL Server 2019+ hoặc SQL Server LocalDB
- Visual Studio 2022 / VS Code + C# Extension / Rider

### Bước 1 — Khởi tạo CSDL
```sql
-- Chạy file gốc để tạo schema và dữ liệu mẫu
sqlcmd -S localhost -E -i "QLThuVienUTC_TheoBaoCao-1.sql"
```

### Bước 2 — Cấu hình Connection String
Sửa `UTC.Library.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QLThuVienUTC;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Bước 3 — Apply EF Core Migrations
```bash
cd UTC.Library.Web
dotnet ef database update --project ../UTC.Library.Infrastructure
```

### Bước 4 — Chạy ứng dụng
```bash
dotnet run --project UTC.Library.Web
```
Truy cập: `https://localhost:7xxx`

### Tài khoản demo mặc định (từ DbInitializer)
| Tài khoản | Mật khẩu | Vai trò |
|---|---|---|
| `admin.demo@utc.edu.vn` | `Admin@123` | Quản trị viên (QTV) |
| `quanly.ban@utc.edu.vn` | `QLB@2026` | Quản lý bạn đọc (QLB) |
| `thuthu@utc.edu.vn` | `TTM@2026` | Thủ thư mượn trả (TTM) |
| `231230001` (MaSV) | `SV@123` | Bạn đọc — Sinh viên UTC |

---

## 🗂️ Tài liệu tham khảo dự án

| Tài liệu | Mô tả | Đường dẫn |
|---|---|---|
| **Figma Admin (02)** | 21 màn hình giao diện Admin | `c:\Users\lemin\Downloads\UTC Library (1)` |
| **Figma Reader (01)** | 11 màn hình Portal Bạn đọc | `c:\Users\lemin\Downloads\UTC Library (4)` |
| **SQL Schema** | CSDL gốc 16 bảng | `c:\Users\lemin\Downloads\QLThuVienUTC_TheoBaoCao-1.sql` |
| **Excel Task Plan** | Kế hoạch 50 task, dropdown trạng thái | `c:\Users\lemin\Downloads\Ke_hoach_Phan_chia_Task_UTC_Library.xlsx` |
| **PLAN.md** | Chi tiết từng task theo Dev & Sprint | `PLAN.md` (cùng thư mục) |

---

## 🌐 Cấu trúc URL chính

```
/                              → Trang chủ Reader Portal
/Account/Login                 → Đăng nhập (phân luồng Admin / Reader)
/Account/Logout

/Admin/Dashboard               → Tổng quan KPI (01 · Figma Admin)
/Admin/Catalog                 → Quản lý Tài liệu (03 · Figma)
/Admin/Catalog/Create          → Thêm tài liệu + kiểm tra trùng (11, 19 · Figma)
/Admin/Copy                    → Quản lý Bản sao (08 · Figma)
/Admin/Reader                  → Quản lý Bạn đọc (02 · Figma)
/Admin/Reader/Import           → Nhập Excel QLĐT
/Admin/Circulation             → Mượn/Trả tài liệu (04, 12 · Figma)
/Admin/Circulation/Borrow      → Lập phiếu mượn (12 · Figma)
/Admin/Circulation/Return      → Ghi nhận trả (13, 18 · Figma)
/Admin/Overdue                 → Theo dõi quá hạn (05, 17 · Figma)
/Admin/Incident                → Sự việc mất/hỏng (15, 20 · Figma)
/Admin/Indemnity               → Hồ sơ bồi thường (21 · Figma)
/Admin/Report                  → Báo cáo thống kê + xuất Excel (07 · Figma)
/Admin/Settings                → Cài đặt quyền nhân viên (06 · Figma)

/Reader/Home                   → Trang chủ UTC (01 · Figma Reader)
/Reader/Search                 → Tra cứu OPAC (05 · Figma Reader)
/Reader/Document/{id}          → Chi tiết tài liệu (06 · Figma Reader)
/Reader/MyLibrary/Current      → Sách đang mượn (03 · Figma Reader)
/Reader/MyLibrary/History      → Lịch sử mượn (08 · Figma Reader)
/Reader/MyLibrary/Notification → Thông báo (09 · Figma Reader)
/Reader/MyLibrary/Profile      → Thông tin bạn đọc (10 · Figma Reader)
/Reader/Services               → Dịch vụ thư viện (07 · Figma Reader)
/Reader/News                   → Tin tức & hoạt động (11 · Figma Reader)
```

---

## ⚙️ Kiến trúc luồng dữ liệu chính

### Luồng Mượn sách
```
[Quầy mượn] → Quét MaBD/MaTBD
  → Kiểm tra tTheBanDoc.TrangThai = 'Hoạt động'
  → Kiểm tra giao dịch quá hạn trong tGiaoDichMuonTra
  ├─ Có quá hạn → HTTP 409 → Redirect 17 · Từ chối + thông báo
  └─ Đủ điều kiện → INSERT tGiaoDichMuonTra + tGiaoDich_BanSao
                   → UPDATE tBanSao.TrangThai = 'Đang mượn'
```

### Luồng Trả sách
```
[Quầy trả] → Quét MaBS (barcode vật lý)
  → Tra cứu tGiaoDich_BanSao theo MaGD active
  ├─ MaBS không khớp → HTTP 422 → Modal cảnh báo 18 · Sai bản sao
  └─ Khớp → Chọn TinhTrang (1=Tốt / 0=Mất·Hỏng)
      ├─ TinhTrang=1 → UPDATE NgayTra + TrangThai='Đã trả'
      │               → UPDATE tBanSao.TrangThai='Có sẵn'
      └─ TinhTrang=0 → UPDATE tBanSao.TrangThai='Mất'
                     → INSERT biên bản → Redirect 15/21 · Bồi thường
```

### Luồng Kiểm tra Trùng Tài liệu
```
[Admin thêm tài liệu] → Nhập TenTL + NamXB + MaNXB
  → Query: SELECT COUNT(*) FROM tTaiLieu
           WHERE TenTL LIKE '%@input%' AND NamXB=@y AND MaNXB=@nxb
  ├─ COUNT > 0 → Modal 19 · Cảnh báo trùng → "Nhập thêm bản sao" / "Tiếp tục"
  └─ COUNT = 0 → Proceed INSERT tTaiLieu + tTaiLieu_TacGia
```

---

## 📋 Quy ước Code

```csharp
// ServiceResult pattern - chuẩn hóa kết quả từ Service Layer
public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public T Data { get; set; }

    public static ServiceResult<T> Success(T data, string msg = "")
        => new() { IsSuccess = true, Data = data, Message = msg };
    public static ServiceResult<T> Fail(string error)
        => new() { IsSuccess = false, Errors = new List<string> { error } };
}

// Controller sử dụng
var result = await _circulationService.BorrowAsync(request);
if (!result.IsSuccess)
    return BadRequest(result.Errors);
return Ok(result.Data);
```

```csharp
// Chuẩn đặt tên Mã định danh
// Tài liệu:    TLVN26-001     (tTaiLieu.MaTL)
// Bản sao:     TLVN26-001-001 (tBanSao.MaBS)
// Giao dịch:   GD2026-00001   (tGiaoDichMuonTra.MaGD)
// Thẻ bạn đọc: TBD2026-0001   (tTheBanDoc.MaTBD)
// Nhân viên:   NV26-01        (tNhanVien.MaNV)
// Tài khoản:   TK26-01        (tTaiKhoan.MaTK)
```

---

> Xem chi tiết từng task, mô tả kỹ thuật và kết quả nghiệm thu trong **[PLAN.md](PLAN.md)**
