using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Interfaces
{
    public interface IChiTietSuaChuaRepository
    {
        Task<List<ChiTietSuaChua>> GetAllAsync();
        Task<ChiTietSuaChua?> GetByIdAsync(int id);
        Task<ChiTietSuaChua?> GetByIdWithDetailsAsync(int id);

        Task<PhieuSuaChua?> GetPhieuByIdAsync(int maPhieu);
        Task<LinhKien?> GetLinhKienByIdAsync(int maLinhKien);
        Task<TaiKhoan?> GetTaiKhoanByIdAsync(int maTaiKhoan);

        Task<List<PhieuSuaChua>> GetAllPhieuAsync();
        Task<List<TaiKhoan>> GetKyThuatViensAsync();
        Task<List<LinhKien>> GetLinhKienConHangAsync();

        Task<decimal> GetTongChiPhiAsync(
            int maPhieu,
            int? excludeMaChiTiet = null);

        Task AddAsync(ChiTietSuaChua chiTiet);
        void Delete(ChiTietSuaChua chiTiet);
        Task SaveChangesAsync();
    }
}
