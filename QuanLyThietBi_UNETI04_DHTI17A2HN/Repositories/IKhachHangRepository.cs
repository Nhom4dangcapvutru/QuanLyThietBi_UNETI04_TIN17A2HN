// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Quản lý khách hàng - CRUD, tìm kiếm, lọc, sắp xếp, phân trang

using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface IKhachHangRepository
    {
        // Hàm lấy dữ liệu có Phân trang, Tìm kiếm, Lọc, Sắp xếp
        Task<(List<KhachHang> Data, int TotalCount)> GetFilteredAsync(
            string? tuKhoa,
            string? loaiKhachHang,
            string? trangThai,
            string? sapXep,
            int page,
            int pageSize);

        // Lấy chi tiết (kèm theo các bảng liên quan)
        Task<KhachHang?> GetDetailsByIdAsync(int? id);

        // Lấy cơ bản để Sửa
        Task<KhachHang?> GetByIdAsync(int? id);

        // Thêm, Sửa, Xóa, Lưu
        Task AddAsync(KhachHang khachHang);
        Task UpdateAsync(KhachHang khachHang);
        Task DeleteAsync(KhachHang khachHang);
        Task SaveAsync();

        // Các hàm kiểm tra nghiệp vụ (Validate)
        Task<bool> ExistsAsync(int id);
        Task<bool> CheckTrungSdtAsync(string sdt, int? excludeId = null);
        Task<bool> CheckTrungEmailAsync(string email, int? excludeId = null);
        Task<bool> CheckTrungCccdAsync(string cccd, int? excludeId = null);
    }
}