using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

public interface ILinhKienRepository
{
    Task<LinhKienIndexViewModel> SearchAsync(string? tuKhoa, string? loai,
        string? trangThai, string? sapXep, int trang);
    Task<LinhKien?> GetByIdAsync(int id);
    Task<LinhKien?> GetForUpdateAsync(int id);
    Task<int> CountUsageAsync(int id);
    Task<bool> HasHistoryAsync(int id);
    Task<bool> NameAndTypeExistsAsync(string tenLinhKien, string loaiLinhKien, int excludedId);
    Task AddAsync(LinhKien item);
    Task UpdateAsync(LinhKien item);
    // Trả về false nếu ràng buộc dữ liệu không cho phép xóa.
    Task<bool> DeleteAsync(LinhKien item);
    // Cộng tồn kho trực tiếp tại database, chỉ nhận số lượng dương và không tràn int.
    Task<bool> NhapKhoAsync(int id, int quantity);
}
