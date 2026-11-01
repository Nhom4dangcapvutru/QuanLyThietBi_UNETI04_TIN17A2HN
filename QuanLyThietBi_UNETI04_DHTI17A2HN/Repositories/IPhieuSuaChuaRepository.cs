// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Quản lý khách hàng - CRUD, tìm kiếm, lọc, sắp xếp, phân trang
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface IPhieuSuaChuaRepository
    {
        // Lấy mã khách hàng từ tài khoản đang đăng nhập
        Task<int?> GetMaKhachHangByTaiKhoanAsync(int maTaiKhoan);

        // Lấy danh sách thiết bị được phép tạo phiếu (đã bao gồm logic lọc phức tạp)
        Task<List<ThietBi>> GetThietBiChoPhepTaoPhieuAsync(int? maKhachHang, bool isAdminOrKtv);

        // Lấy chi tiết thiết bị kèm khách hàng (để validate)
        Task<ThietBi?> GetThietBiKemKhachHangAsync(int maThietBi);

        // Kiểm tra khách hàng có đang hoạt động không
        Task<bool> KiemTraKhachHangHoatDongAsync(int maKhachHang);

        // Kiểm tra thiết bị có phiếu nào đang xử lý/chờ không
        Task<bool> CoPhieuDangXuLyAsync(int maThietBi);

        // Lấy danh sách phiếu (có lọc, sắp xếp, phân trang)
        Task<(List<PhieuSuaChua> Data, int TotalCount)> GetFilteredPhieuAsync(
            int? maKhachHang,
            bool isAdminOrKtv,
            string? tuKhoa,
            string? trangThai,
            string? sapXep,
            int page,
            int pageSize);

        // Lấy chi tiết phiếu (Include đầy đủ)
        Task<PhieuSuaChua?> GetPhieuDetailsAsync(int? id);

        // Lấy phiếu cơ bản để xử lý Hủy
        Task<PhieuSuaChua?> GetPhieuByIdAsync(int? id);

        // Thêm và Lưu
        Task AddAsync(PhieuSuaChua phieu);
        Task SaveAsync();
    }
}