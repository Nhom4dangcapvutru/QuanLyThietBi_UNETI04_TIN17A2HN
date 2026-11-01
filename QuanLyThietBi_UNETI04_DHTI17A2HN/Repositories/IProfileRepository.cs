// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Quản lý khách hàng - CRUD, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface IProfileRepository
    {
        // Lấy mã khách hàng từ tài khoản đăng nhập
        Task<int?> GetMaKhachHangByTaiKhoanAsync(int maTaiKhoan);

        // Lấy hồ sơ đầy đủ (kèm theo đếm số thiết bị, số phiếu) cho trang Index
        Task<KhachHang?> GetProfileFullDetailsAsync(int maKhachHang);

        // Lấy hồ sơ cơ bản để Sửa
        Task<KhachHang?> GetByIdAsync(int maKhachHang);

        // Các hàm kiểm tra trùng lặp
        Task<bool> CheckTrungSdtAsync(string sdt, int maKhachHang);
        Task<bool> CheckTrungEmailAsync(string email, int maKhachHang);

        // Lưu thay đổi
        Task SaveAsync();
    }
}