// Họ và tên: Nguyễn Văn Tú
// Mã sinh viên: 23103100077
// Nội dung thực hiện: Module 2 - Interface Repository quản lý thiết bị

using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Common;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface IThietBiRepository
    {
        // Vai trò và mã tài khoản phải lấy từ Session, không nhận từ form/URL.
        Task<PaginatedList<ThietBi>> SearchAsync(ThietBiFilter filter, string? vaiTro, int? maTaiKhoan);
        Task<ThietBi?> GetDetailsAsync(int id, string? vaiTro, int? maTaiKhoan);
        Task<ThietBi?> GetByIdAsync(int id);
        Task<bool> SerialExistsAsync(string serialNumber, int? excludedId = null);
        Task<bool> ExistsAsync(int id);
        Task<int> CountHistoryAsync(int id);
        Task<List<LoaiThietBi>> GetLoaiThietBisAsync();
        Task<List<KhachHang>> GetKhachHangsAsync();
        Task AddAsync(ThietBi thietBi);
        // Trả về false khi thiết bị đã bị xóa bởi request khác.
        Task<bool> UpdateAsync(ThietBi thietBi);
        Task<bool> DeleteAsync(int id);
    }
}
