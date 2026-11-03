using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Interfaces;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class ChiTietSuaChuaRepository : IChiTietSuaChuaRepository
    {
        private readonly ApplicationDbContext _context;

        public ChiTietSuaChuaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách chi tiết sửa chữa kèm thông tin liên quan.
        public async Task<List<ChiTietSuaChua>> GetAllAsync()
        {
            return await _context.ChiTietSuaChuas
                .AsNoTracking()
                .Include(c => c.PhieuSuaChua)
                .Include(c => c.LinhKien)
                .Include(c => c.KyThuatVien)
                .OrderByDescending(c => c.MaChiTiet)
                .ToListAsync();
        }

        // Lấy bản ghi có tracking để cập nhật hoặc xóa.
        public async Task<ChiTietSuaChua?> GetByIdAsync(int id)
        {
            return await _context.ChiTietSuaChuas
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);
        }

        // Lấy bản ghi kèm các navigation property để hiển thị Details/Delete.
        public async Task<ChiTietSuaChua?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.ChiTietSuaChuas
                .AsNoTracking()
                .Include(c => c.PhieuSuaChua)
                .Include(c => c.LinhKien)
                .Include(c => c.KyThuatVien)
                .FirstOrDefaultAsync(c => c.MaChiTiet == id);
        }

        public async Task<PhieuSuaChua?> GetPhieuByIdAsync(int maPhieu)
        {
            return await _context.PhieuSuaChuas
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);
        }

        public async Task<LinhKien?> GetLinhKienByIdAsync(int maLinhKien)
        {
            return await _context.LinhKiens
                .FirstOrDefaultAsync(l => l.MaLinhKien == maLinhKien);
        }

        public async Task<TaiKhoan?> GetTaiKhoanByIdAsync(int maTaiKhoan)
        {
            return await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);
        }

        public async Task<List<PhieuSuaChua>> GetAllPhieuAsync()
        {
            return await _context.PhieuSuaChuas
                .AsNoTracking()
                .OrderByDescending(p => p.MaPhieu)
                .ToListAsync();
        }

        // Giữ đúng giá trị VaiTro đang được sử dụng trong project.
        public async Task<List<TaiKhoan>> GetKyThuatViensAsync()
        {
            return await _context.TaiKhoans
                .AsNoTracking()
                .Where(t => t.VaiTro == "NhanVien" ||
                            t.VaiTro == "KyThuatVien")
                .OrderBy(t => t.HoTen)
                .ToListAsync();
        }

        public async Task<List<LinhKien>> GetLinhKienConHangAsync()
        {
            return await _context.LinhKiens
                .AsNoTracking()
                .Where(l => l.SoLuongTon > 0)
                .OrderBy(l => l.TenLinhKien)
                .ToListAsync();
        }

        // Có thể loại trừ một chi tiết khi tính lại tổng tiền lúc Edit/Delete.
        public async Task<decimal> GetTongChiPhiAsync(
            int maPhieu,
            int? excludeMaChiTiet = null)
        {
            var query = _context.ChiTietSuaChuas
                .Where(c => c.MaPhieu == maPhieu);

            if (excludeMaChiTiet.HasValue)
            {
                query = query.Where(
                    c => c.MaChiTiet != excludeMaChiTiet.Value);
            }

            return await query.SumAsync(c => (decimal?)c.ThanhTien) ?? 0m;
        }

        public async Task AddAsync(ChiTietSuaChua chiTiet)
        {
            await _context.ChiTietSuaChuas.AddAsync(chiTiet);
        }

        public void Delete(ChiTietSuaChua chiTiet)
        {
            _context.ChiTietSuaChuas.Remove(chiTiet);
        }

        public async Task SaveChangesAsync()
        {
            // EF Core tự dùng transaction cho một lần SaveChanges.
            await _context.SaveChangesAsync();
        }
    }
}
