> Cập nhật SQL-only: xem mục 9 [doc.md](../doc.md). Những mục bên dưới mô tả các đợt kiểm tra trước; nguồn dữ liệu hiện tại hoàn toàn là SQL, không còn cache/nguồn JSON.

# Rà soát ứng dụng — 11/11/2026

## Phạm vi và kết quả

- Build .NET thành công: 0 lỗi, 0 cảnh báo.
- 53 lượt tải trang bằng ba phiên riêng: khách, sinh viên, admin. Bao gồm trang chủ, tra cứu, chi tiết sách, dịch vụ, tin tức, đăng nhập, hồ sơ, các tab admin và Swagger.
- 115 tài nguyên nội bộ trả về thành công. Không thấy ID trùng hoặc modal trỏ tới phần tử không tồn tại trong những trang đã kiểm tra.
- 613 đoạn JavaScript và thuộc tính sự kiện trong HTML được kiểm tra cú pháp; đây không phải kiểm tra chạy JavaScript trong trình duyệt.
- 5 kiểm tra tương tác ô tìm bạn đọc đều đạt: debounce, phản hồi đến sai thứ tự, chọn bằng bàn phím, hạn mức, đổi người đang chọn, lỗi mạng và đóng modal.
- Bộ kiểm tra SQL dùng database riêng `QLThuVienUTC_BorrowCheck_*`, đã xóa sau khi chạy. Không thực hiện mượn/trả hoặc tạo/xóa cán bộ trên database thật.

## Lỗi đã sửa

1. Trả sách phải khớp giao dịch đang mượn và mã bạn đọc. Mã sai không được chuyển sang cuốn đầu tiên; trả lặp không tạo khoản thu lần nữa.
2. Thu phí dùng mã nhân viên thực tế. Trạng thái phiếu, bản sao và thanh toán được lưu cùng transaction; phiếu thu chỉ thêm sau khi SQL thành công.
3. API mượn/trả dùng chung nghiệp vụ SQL với Admin, không ghi dự phòng sang JSON để bỏ qua lỗi hết sách, học liệu điện tử hoặc mã bạn đọc không tồn tại.
4. API chứa dữ liệu bạn đọc/giao dịch yêu cầu tài khoản cán bộ. Các POST mượn/trả, thêm cán bộ và đổi quyền yêu cầu anti-forgery token; Swagger và form AJAX gửi token tương ứng.
5. Đăng nhập không chấp nhận mật khẩu dự phòng chung, không tự ghi đè mật khẩu và không cho tài khoản khóa đăng nhập. Giá trị trạng thái hoạt động bị lỗi mã hóa trong dữ liệu cũ được nhận diện riêng, không sửa dữ liệu gốc.
6. Endpoint Microsoft chưa cấu hình đã được gỡ trong đợt rà soát UI/backend; `/Account/LoginWithMicrosoft` trả `404`. Form login chỉ giữ đăng nhập thư viện. Chi tiết thay đổi tại [doc.md](../doc.md).
7. Cập nhật thông tin liên hệ/ngân hàng giữ nguyên định danh và lịch sử mượn, dữ liệu hồ sơ được lưu SQL.
8. Form nhập QLĐT trỏ đúng action. Mã sách không tồn tại trả 404 thay vì hiển thị sách khác.
9. Phân quyền kiểm tra vai trò; tạo cán bộ và tài khoản trong một lần lưu, tránh hồ sơ nhân viên mồ côi khi tạo tài khoản thất bại. Giới hạn mã/tên/email theo cột SQL.
10. Tên sách/tên cán bộ được truyền qua `data-*`, tránh làm hỏng JavaScript khi tên chứa dấu nháy. Ô tiền bồi thường chỉ được kiểm tra khi chọn đền tiền.
11. API phân trang chuẩn hóa page/pageSize và tránh tràn số khi offset quá lớn.
12. Nút gửi nhắc trả được vô hiệu hóa khi chưa có dịch vụ email, tránh thông báo gửi thành công giả.
13. Báo cáo lấy trực tiếp `TaiLieu`, `BanSao`, `PhieuMuon`, `ChiTietMuon` từ SQL. Lượt mượn tính theo từng bản trong phiếu; lượt trả theo `NgayTra`. Kỳ tháng/quý/năm dùng khoảng từ đầu kỳ đến trước đầu kỳ tiếp theo. Tồn kho và quá hạn là số hiện tại. File Excel dùng cùng bộ truy vấn với trang báo cáo. Bỏ phạm vi cơ sở do SQL chưa có dữ liệu phân chia cơ sở.
14. Trang báo cáo có lọc ngày, tổng hồ sơ bạn đọc từ `BanDoc`, biểu đồ mượn/trả của sáu tháng kết thúc ở tháng được chọn và trạng thái kho hiện tại. Kho sách phân trang SQL 10 đầu sách/trang, tìm tên không dấu hoặc ISBN. Tổng kho và tổng bạn đọc không thay đổi theo phân trang/tìm kiếm. Dòng tổng trong bảng tính toàn bộ kết quả lọc. Excel xuất đầy đủ kết quả, giữ bộ lọc ngày và từ khóa.
15. Đổi trang và tìm sách trong báo cáo cập nhật riêng bảng, giữ bộ lọc và URL. Bấm Back/Forward sẽ tải lại đúng kết quả đã xem. Phản hồi cũ không ghi đè trang mới khi bấm nhanh; lỗi mạng giữ nguyên bảng và cho thử lại. Phân trang có trang đầu/cuối, dấu lược và nút trước/sau cố định; màn nhỏ hiển thị gọn số trang hiện tại. Khi tắt JavaScript, các liên kết và form vẫn hoạt động bình thường.
16. Toàn bộ admin dùng `admin-minimal.css`, chỉ áp dụng trong `.admin-body`. Header và menu chữ gọn; bỏ icon trang trí, breadcrumb lặp và avatar trang trí. Các thao tác bản sách/sửa/xóa/hồ sơ có nhãn chữ. Giữ bảng màu xanh ban đầu, nền xanh nhạt, màu các ô số liệu, cảnh báo, trạng thái và chỉ báo biểu đồ. Các bản view trong `Views` và `Areas/Admin/Views` được cập nhật đồng thời.
17. Trên desktop, menu trái cố định dưới header, dành khoảng trống riêng đúng chiều rộng menu để không đè nội dung. Chiều cao menu theo viewport và chiều cao header đo thực tế; danh sách mục cuộn bên trong khi màn hình thấp. Màn nhỏ tiếp tục dùng menu mở/đóng dạng drawer. Body có chiều cao tự động để header sticky đi theo toàn bộ trang dài.
18. Khung điều hướng giữ bố cục tham chiếu: dải trường nền xanh đậm, hàng logo/tên thư viện Việt–Anh, nút Tổng quan và tài khoản bên phải, menu xanh với tên mục đầy đủ bên trái. Menu và header giữ vị trí khi cuộn. Nội dung biểu mẫu/bảng vẫn theo cách trình bày tối giản; không khôi phục icon trang trí.
19. Quản lý học liệu dùng thẻ ảnh bìa, bộ lọc bên trái và 12 học liệu/trang. Bộ lọc gồm loại tài liệu, chuyên ngành, ngôn ngữ và tình trạng kho; tìm tên/tác giả/ISBN, giữ điều kiện khi đổi trang. Các nút Bản sách/Sửa/Xóa liên kết đúng mã trên từng thẻ; giữ nguyên form thêm/sửa/xóa và CSRF. Trang đọc và quản lý tiếp tục dùng cùng dịch vụ danh mục hiện có. Không chuyển nguồn danh mục từ JSON sang SQL trong thay đổi giao diện này.
20. Mượn trả và quá hạn hiển thị 10 dòng/trang, giữ từ khóa và bộ lọc khi chuyển trang. Tổng số lượt và tiền phạt tính trên toàn bộ kết quả lọc. Form mượn/trả giữ URL trang đang xem, được kiểm tra là đường dẫn nội bộ trước khi chuyển hướng; trang ngoài phạm vi được đưa về trang hợp lệ. Bộ kiểm tra HTTP đối chiếu toàn bộ 49 giao dịch/5 trang, 14 lượt quá hạn/2 trang với API; không thiếu/trùng dòng, bộ lọc và tổng phí đều đúng. Kiểm tra form mượn chỉ gửi dữ liệu không hợp lệ, không tạo hay trả sách trên database hiện có.
21. Quản lý bạn đọc phân trang riêng bảng đang mượn và chưa mượn, 10 người/trang. Hai tham số `borrowingPage`/`nonBorrowingPage` độc lập; liên kết mỗi bảng giữ trang hiện tại của bảng kia, từ khóa và nhóm bạn đọc. Tìm kiếm hoặc đổi nhóm bắt đầu lại từ trang đầu. Kiểm tra HTTP đối chiếu đủ 69 bạn đọc với API (8 đang mượn, 61 chưa mượn/7 trang), các nhóm, tìm có/không dấu, kết quả rỗng và số trang ngoài phạm vi. Các trang mượn trả/quá hạn dùng chung bộ phân trang vẫn đạt kiểm tra.

Kiểm tra danh mục dạng thẻ: 90 học liệu qua 8 trang; 14 tổ hợp bộ lọc/từ khóa, trường hợp rỗng và trang ngoài phạm vi đều đạt. Dữ liệu cho hộp thoại sửa và bản sách khớp mã học liệu. Bộ kiểm tra tải trang đạt 53 lượt/118 tài nguyên/548 đoạn JavaScript và sự kiện. Không gửi thao tác thêm/sửa/xóa hợp lệ lên dữ liệu đang dùng để kiểm tra giao diện.

Lần kiểm tra sau thay đổi giao diện admin: build không lỗi/cảnh báo; 53 lượt tải trang, 117 tài nguyên, 540 đoạn JavaScript/sự kiện trong HTML và 11 kiểm tra tương tác tìm bạn đọc/phân trang đều đạt. Đối chiếu riêng 10 trang admin xác nhận cùng stylesheet, không còn icon font trang trí và các nút/liên kết có nhãn. Đây là kiểm tra HTML/JavaScript, chưa phải xác nhận trực quan tại các kích thước màn hình.

## Phần chưa hoàn chỉnh

- Nhập QLĐT hiện là màn xem dữ liệu mẫu/hiện có; `ProcessImport` chỉ đếm bạn đọc, chưa đọc tệp hoặc kết nối nguồn đào tạo. Đường dẫn form đã được sửa, nhưng bộ nhập dữ liệu chưa được triển khai.
- Email nhắc trả chưa được kết nối. Microsoft SSO không có trong UI đăng nhập; action mẫu đã được gỡ.
- Chưa kiểm tra trực tiếp bố cục hoặc console trong trình duyệt ở 1920×1080, Windows scale 100%, zoom 100%. Các kết quả HTTP/CSS không chứng minh giao diện không tràn hoặc cắt ở kích thước này.

## Chạy lại kiểm tra

Ứng dụng cần chạy tại `http://localhost:5124`. Các kiểm tra HTTP sử dụng tài khoản demo admin và sinh viên hiện có, không gửi thao tác nghiệp vụ hợp lệ làm thay đổi dữ liệu.

```powershell
dotnet build BTL_LTWEB.csproj --no-restore
python -X utf8 tests/Smoke/check_pages.py
python -X utf8 tests/Smoke/check_security.py
python -X utf8 tests/Smoke/check_catalog.py
python -X utf8 tests/Smoke/check_circulation_pagination.py
python -X utf8 tests/Smoke/check_reader_pagination.py
node --test tests/Borrowing/reader-search.test.cjs
```

Kiểm tra SQL cần tài khoản Windows có quyền tạo/xóa database thử trên `.\HUYPC`:

```powershell
dotnet run --project tests/Borrowing/BorrowVerification.csproj --no-restore -p:UseAppHost=false -p:OutputPath="$PWD\bin\audit-verification\out"
```

Kiểm tra báo cáo trên database thử riêng (tự xóa sau khi chạy):

```powershell
dotnet run --project tests/Reports/ReportVerification.csproj --no-restore -p:UseAppHost=false -p:OutputPath=bin/report-verification/out
```

Đối chiếu báo cáo với truy vấn SQL trên `QLThuVienUTC` hiện có, chỉ đọc dữ liệu:

```powershell
dotnet tests/Reports/bin/report-verification/out/ReportVerification.dll --live
python -X utf8 tests/Reports/check_report.py
node --test tests/Reports/pagination.test.cjs
```

Bộ kiểm tra báo cáo bao gồm đầu/cuối kỳ, khoảng ngày tùy chọn, sách trả ở kỳ khác với ngày mượn, sách đã bồi thường, hạn trả hôm nay, tên sách trùng, đầu sách không có bản giấy, trạng thái mất/hỏng/khác, tìm không dấu/ISBN, phân trang, tổng từng dòng, số liệu biểu đồ, Excel và thông báo khi thiếu database. Không tạo lại database project để kiểm tra trạng thái thiếu database.
