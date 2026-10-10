// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Helper đọc/ghi Session - Dùng chung cho Module 3
// (Quản lý khách hàng, hồ sơ cá nhân, tạo phiếu sửa chữa, theo dõi phiếu)

using Microsoft.AspNetCore.Http;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers
{
    /// <summary>
    /// Lớp tiện ích đọc/ghi Session.
    /// Tập trung toàn bộ việc truy cập Session vào 1 chỗ.
    /// Sau này nếu SV1 (đăng nhập) dùng key khác, chỉ cần sửa 3 hằng số bên dưới.
    /// </summary>
    public static class SessionHelper
    {
        // ====================================================================
        // ⚠️ QUAN TRỌNG: 3 hằng số dưới đây PHẢI khớp với key mà SV1 dùng
        // khi lưu Session trong TaiKhoanController (chức năng đăng nhập).
        // Nếu SV1 dùng key khác, chỉ cần sửa 3 dòng này là toàn bộ Module 3
        // tự động chạy đúng, không cần sửa file nào khác.
        // ====================================================================
        public const string Key_MaTaiKhoan = "MaTaiKhoan";
        public const string Key_VaiTro = "VaiTro";
        public const string Key_HoTen = "HoTen";

        // --------------------------------------------------------------------
        // GHI Session (dùng khi test hoặc khi cần set session thủ công)
        // --------------------------------------------------------------------

        /// <summary>
        /// Lưu thông tin đăng nhập vào Session (thường do TaiKhoanController gọi).
        /// </summary>
        public static void SetUser(ISession session, int maTaiKhoan, string vaiTro, string hoTen)
        {
            session.SetInt32(Key_MaTaiKhoan, maTaiKhoan);
            session.SetString(Key_VaiTro, vaiTro ?? string.Empty);
            session.SetString(Key_HoTen, hoTen ?? string.Empty);
        }

        // --------------------------------------------------------------------
        // ĐỌC Session
        // --------------------------------------------------------------------

        /// <summary>
        /// Lấy Mã tài khoản của người đang đăng nhập.
        /// Trả về null nếu chưa đăng nhập.
        /// </summary>
        public static int? GetMaTaiKhoan(ISession session)
        {
            return session.GetInt32(Key_MaTaiKhoan);
        }

        /// <summary>
        /// Lấy Vai trò (Admin / KyThuatVien / KhachHang).
        /// Trả về null nếu chưa đăng nhập.
        /// </summary>
        public static string? GetVaiTro(ISession session)
        {
            return session.GetString(Key_VaiTro);
        }

        /// <summary>
        /// Lấy Họ tên người đang đăng nhập (dùng để hiển thị "Xin chào ...").
        /// </summary>
        public static string? GetHoTen(ISession session)
        {
            return session.GetString(Key_HoTen);
        }

        // --------------------------------------------------------------------
        // KIỂM TRA TRẠNG THÁI ĐĂNG NHẬP
        // --------------------------------------------------------------------

        /// <summary>
        /// Kiểm tra người dùng đã đăng nhập chưa.
        /// </summary>
        public static bool IsLoggedIn(ISession session)
        {
            return session.GetInt32(Key_MaTaiKhoan).HasValue;
        }

        /// <summary>
        /// Kiểm tra người đang đăng nhập có vai trò Admin không.
        /// </summary>
        public static bool IsAdmin(ISession session)
        {
            return session.GetString(Key_VaiTro) == "Admin";
        }

        /// <summary>
        /// Kiểm tra người đang đăng nhập có vai trò Kỹ thuật viên không.
        /// </summary>
        public static bool IsKyThuatVien(ISession session)
        {
            return session.GetString(Key_VaiTro) == "KyThuatVien";
        }

        /// <summary>
        /// Kiểm tra người đang đăng nhập có vai trò Khách hàng không.
        /// </summary>
        public static bool IsKhachHang(ISession session)
        {
            return session.GetString(Key_VaiTro) == "KhachHang";
        }

        /// <summary>
        /// Kiểm tra user có vai trò Admin HOẶC Kỹ thuật viên không.
        /// Dùng để phân quyền cho các chức năng quản lý khách hàng.
        /// </summary>
        public static bool IsAdminOrKyThuatVien(ISession session)
        {
            var vaiTro = session.GetString(Key_VaiTro);
            return vaiTro == "Admin" || vaiTro == "KyThuatVien";
        }

        // --------------------------------------------------------------------
        // XÓA SESSION (đăng xuất)
        // --------------------------------------------------------------------

        /// <summary>
        /// Xóa toàn bộ Session (gọi khi đăng xuất).
        /// </summary>
        public static void Clear(ISession session)
        {
            session.Clear();
        }
    }
}