# Kiểm tra tích hợp MVC

Cần .NET 8 SDK, Python 3 và Node.js. Trong thư mục `Tests`, chạy:

```bash
npm install
npx playwright install chromium
python run.py
```

Bộ kiểm tra build ứng dụng, chạy server ở cổng trống và dùng database SQLite tạm với dữ liệu demo riêng. Không dùng database SQL Server/SQLite đang cấu hình cho nhóm. Database và ảnh chụp kiểm tra tạm được dọn khi kết thúc.

Kiểm tra đăng nhập, role, CSRF, hồ sơ, danh mục, mượn trả, bồi thường, thẻ khóa, TSV và CSV; 16 route ở 5 kích thước màn hình. Các trường hợp quá hạn/phiếu thu trùng/khóa tài khoản đã được kiểm tra bổ sung với fixture riêng trong quá trình bàn giao.
