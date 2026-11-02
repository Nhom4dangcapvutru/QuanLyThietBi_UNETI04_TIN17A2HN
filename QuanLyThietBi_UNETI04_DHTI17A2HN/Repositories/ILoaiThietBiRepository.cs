// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Cài đặt Repository cho phân hệ Quản lý Tài khoản
// Thực thi các truy vấn LINQ (Tìm kiếm, Lọc, Include bảng liên quan) để phục vụ Controller.
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface ILoaiThietBiRepository
    {
        Task<IEnumerable<LoaiThietBi>> GetAllAsync(string? searchString);
        Task<LoaiThietBi?> GetByIdAsync(int id);
        Task<LoaiThietBi?> GetByIdWithThietBiAsync(int id);
        Task<bool> CheckTenLoaiExistsAsync(string tenLoai, int? excludeId = null);
        Task AddAsync(LoaiThietBi loai);
        void Update(LoaiThietBi loai);
        void Delete(LoaiThietBi loai);
        Task<int> SaveChangesAsync();
    }
}