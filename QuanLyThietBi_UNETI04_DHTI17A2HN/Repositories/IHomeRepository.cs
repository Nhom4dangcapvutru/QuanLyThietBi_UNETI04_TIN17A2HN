using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

public interface IHomeRepository
{
    // Danh tính và vai trò lấy từ Session, không nhận từ URL hoặc form.
    Task<DashboardViewModel> GetDashboardAsync(string? vaiTro, int? maTaiKhoan);
}
