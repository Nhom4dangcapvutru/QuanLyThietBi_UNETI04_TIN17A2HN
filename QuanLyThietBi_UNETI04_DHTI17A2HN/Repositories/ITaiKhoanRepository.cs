// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Interface Repository cho phân hệ Quản lý Tài khoản
// Định nghĩa các phương thức CRUD, kiểm tra trùng lặp dữ liệu (Tên đăng nhập, Email).
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface ITaiKhoanRepository
    {
        Task<IEnumerable<TaiKhoan>> GetAllAsync(string? searchString, string? vaiTro, string? trangThai);
        Task<TaiKhoan?> GetByIdAsync(int id);
        Task<bool> CheckTenDangNhapExistsAsync(string tenDangNhap, int? excludeId = null);
        Task<bool> CheckEmailExistsAsync(string email, int? excludeId = null);
        Task AddAsync(TaiKhoan taiKhoan);
        void Update(TaiKhoan taiKhoan);
        void Delete(TaiKhoan taiKhoan);
        Task<int> SaveChangesAsync();
    }
}