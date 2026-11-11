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
6. Endpoint Microsoft chưa cấu hình không còn tự đăng nhập thành một sinh viên mẫu. Bỏ hai nút điền nhanh giảng viên do database không có các tài khoản đó.
7. Cập nhật thông tin liên hệ/ngân hàng giữ nguyên định danh và lịch sử mượn, kể cả khi đọc hồ sơ từ JSON.
8. Form nhập QLĐT trỏ đúng action. Mã sách không tồn tại trả 404 thay vì hiển thị sách khác.
9. Phân quyền kiểm tra vai trò; tạo cán bộ và tài khoản trong một lần lưu, tránh hồ sơ nhân viên mồ côi khi tạo tài khoản thất bại. Giới hạn mã/tên/email theo cột SQL.
10. Tên sách/tên cán bộ được truyền qua `data-*`, tránh làm hỏng JavaScript khi tên chứa dấu nháy. Ô tiền bồi thường chỉ được kiểm tra khi chọn đền tiền.
11. API phân trang chuẩn hóa page/pageSize và tránh tràn số khi offset quá lớn.
12. Nút gửi nhắc trả được vô hiệu hóa khi chưa có dịch vụ email, tránh thông báo gửi thành công giả.

## Phần chưa hoàn chỉnh

- Báo cáo vẫn có số liệu minh họa trong `ReportService` (lượt mượn/trả và mất/hỏng); chưa phải báo cáo nghiệp vụ hoàn toàn từ SQL. Giao diện có ghi “Số liệu minh họa”.
- Nhập QLĐT hiện là màn xem dữ liệu mẫu/hiện có; `ProcessImport` chỉ đếm bạn đọc, chưa đọc tệp hoặc kết nối nguồn đào tạo. Đường dẫn form đã được sửa, nhưng bộ nhập dữ liệu chưa được triển khai.
- Email nhắc trả và Microsoft SSO chưa được kết nối.
- Chưa kiểm tra trực tiếp bố cục hoặc console trong trình duyệt ở 1920×1080, Windows scale 100%, zoom 100%. Các kết quả HTTP/CSS không chứng minh giao diện không tràn hoặc cắt ở kích thước này.

## Chạy lại kiểm tra

Ứng dụng cần chạy tại `http://localhost:5124`. Các kiểm tra HTTP sử dụng tài khoản demo admin và sinh viên hiện có, không gửi thao tác nghiệp vụ hợp lệ làm thay đổi dữ liệu.

```powershell
dotnet build BTL_LTWEB.csproj --no-restore
python -X utf8 tests/Smoke/check_pages.py
python -X utf8 tests/Smoke/check_security.py
node --test tests/Borrowing/reader-search.test.cjs
```

Kiểm tra SQL cần tài khoản Windows có quyền tạo/xóa database thử trên `.\HUYPC`:

```powershell
dotnet run --project tests/Borrowing/BorrowVerification.csproj --no-restore -p:UseAppHost=false -p:OutputPath="$PWD\bin\audit-verification\out"
```
