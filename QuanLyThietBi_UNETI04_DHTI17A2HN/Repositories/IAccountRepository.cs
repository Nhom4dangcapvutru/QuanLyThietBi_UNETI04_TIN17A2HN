// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Interface Repository cho phân hệ Đăng nhập (Account)
// Chịu trách nhiệm định nghĩa các phương thức truy xuất dữ liệu liên quan đến xác thực tài khoản.
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface IAccountRepository
    {
        Task<TaiKhoan?> LoginAsync(string tenDangNhap, string matKhau);
        Task<bool> RegisterAsync(TaiKhoan taiKhoan, KhachHang khachHang);
    }
}