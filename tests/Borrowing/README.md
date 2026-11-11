# Kiểm tra mượn và trả sách

Chạy tại thư mục gốc của repo, bằng tài khoản Windows có quyền tạo database trên SQL Server `.\HUYPC`:

```powershell
dotnet run --project tests/Borrowing/BorrowVerification.csproj
```

Chương trình tạo database riêng tên `QLThuVienUTC_BorrowCheck_*` cùng dữ liệu giả, rồi xóa database đó sau khi chạy. Không sử dụng database `QLThuVienUTC` hoặc thư mục `App_Data` của ứng dụng.

Các trường hợp được kiểm tra:

- Mượn lần đầu khi SQL chưa có học liệu, bản sao hoặc hồ sơ nhân viên.
- Mã học liệu trong catalog khác khóa identity của SQL.
- Đúng người lập phiếu, cập nhật hồ sơ bạn đọc, mượn và trả cập nhật lại tồn kho.
- Chặn trùng đầu sách, hết bản sao, học liệu điện tử, bạn đọc/tài khoản không hợp lệ.
- Giới hạn 5 cuốn cho sinh viên và 10 cuốn cho giảng viên.
- Tìm bạn đọc theo mã hoặc tên có/không dấu ngay trong SQL, tối đa 10 kết quả; số sách đang mượn không tính sách đã trả.
- Kỳ 1 mở mượn từ 11/08 đến 31/12, kỳ 2 từ 02/01 đến 31/05; hạn trả cuối kỳ trong cùng năm, bao gồm ngày cuối kỳ và năm nhuận.
- Ngày ngoài học kỳ bị từ chối ở cả SQL và API, không ghi phiếu mượn hoặc hồ sơ fallback.
- Rollback toàn bộ dữ liệu phiếu mượn khi gặp lỗi SQL.
- Trả đúng mã giao dịch và bạn đọc, chặn trả lặp; khoản thu dùng cán bộ thật và không tạo phiếu thu khi SQL lưu thất bại.
- API từ chối dữ liệu mượn/trả không hợp lệ thay vì ghi dự phòng vào JSON.
- Cập nhật liên hệ giữ nguyên định danh và lịch sử mượn.
- Tài khoản khóa, mật khẩu dự phòng chung và vai trò không hợp lệ bị từ chối; cán bộ và tài khoản được tạo cùng nhau.

Kiểm tra tương tác ô tìm bạn đọc (cần Node.js):

```powershell
node --test tests/Borrowing/reader-search.test.cjs
```
