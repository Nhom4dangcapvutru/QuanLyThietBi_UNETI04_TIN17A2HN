// Họ và tên: Đinh Thị Thùy Linh
// Mã sinh viên: 23103100110
// Nội dung thực hiện: Tiếp nhận - Chẩn đoán - Sửa chữa - Cập nhật trạng thái và chi phí

// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Quản lý khách hàng - CRUD, tìm kiếm, lọc, sắp xếp, phân trang

using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public interface IPhieuSuaChuaRepository
    {
        Task<int?> GetMaKhachHangByTaiKhoanAsync(int maTaiKhoan);
        Task<List<ThietBi>> GetThietBiChoPhepTaoPhieuAsync(int? maKhachHang, bool isAdminOrKtv);
        Task<ThietBi?> GetThietBiKemKhachHangAsync(int maThietBi);
        Task<bool> KiemTraKhachHangHoatDongAsync(int maKhachHang);
        Task<bool> CoPhieuDangXuLyAsync(int maThietBi);
        Task<(List<PhieuSuaChua> Data, int TotalCount)> GetFilteredPhieuAsync(
            int? maKhachHang, bool isAdminOrKtv, string? tuKhoa, string? trangThai, string? sapXep, int page, int pageSize);
        Task<PhieuSuaChua?> GetPhieuDetailsAsync(int? id);
        Task<PhieuSuaChua?> GetPhieuByIdAsync(int? id);
        Task AddAsync(PhieuSuaChua phieu);
        Task SaveAsync();

        Task<(List<PhieuSuaChua> Items, int TotalItems)> GetPagedForProcessingAsync(
            string? searchString, string? trangThai, string? mucDo,
            int? maLoaiThietBi, DateTime? tuNgay, DateTime? denNgay,
            string? sortOrder, int pageIndex, int pageSize);

        Task<PhieuSuaChua?> GetByIdWithDetailsAsync(int id);
        Task<List<LinhKien>> GetAllLinhKiensAsync();

        Task<PhieuSuaChua?> GetByIdForUpdateAsync(int id, bool includeChiTiet = false);
        Task<LinhKien?> GetLinhKienForUpdateAsync(int maLinhKien);
        Task<List<LinhKien>> GetLinhKienConHangAsync();
        Task<ChiTietSuaChua?> GetChiTietForUpdateAsync(int maChiTiet, int maPhieu);
        Task AddChiTietAsync(ChiTietSuaChua chiTiet);
        void DeleteChiTiet(ChiTietSuaChua chiTiet);

        Task<List<LoaiThietBi>> GetLoaiThietBisAsync();
    }
}
