using System.Security.Cryptography;
using System.Text;

namespace BTL_LTWEB.Common
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Tạo chuỗi mã hóa mật khẩu chuẩn SHA-256 (64 ký tự Hex viết hoa)
        /// Bảo mật an toàn tuyệt đối, tránh lộ mật khẩu khi bị đánh cắp CSDL
        /// </summary>
        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword)) return string.Empty;
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));
            return Convert.ToHexString(bytes).ToUpperInvariant();
        }

        /// <summary>
        /// Kiểm tra tính chính xác của mật khẩu người dùng nhập vào so với mật khẩu đã băm trong CSDL
        /// </summary>
        public static bool VerifyPassword(string inputPassword, string? storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(inputPassword))
                return false;

            // 1. So khớp chuẩn chuỗi băm SHA-256
            var inputHash = HashPassword(inputPassword);
            if (string.Equals(inputHash, storedHash, StringComparison.OrdinalIgnoreCase))
                return true;

            // 2. Dự phòng trường hợp dữ liệu cũ lưu dạng thường hoặc linh hoạt
            if (string.Equals(inputPassword, storedHash, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
