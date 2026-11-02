// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Cài đặt Repository cho phân hệ Quản lý Loại thiết bị
// Xử lý truy vấn lấy danh sách, kiểm tra trùng tên và tải kèm danh sách Thiết bị (Include) để kiểm tra khóa ngoại.
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class LoaiThietBiRepository : ILoaiThietBiRepository
    {
        private readonly ApplicationDbContext _context;

        public LoaiThietBiRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LoaiThietBi>> GetAllAsync(string? searchString)
        {
            var query = _context.LoaiThietBis.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(l => l.TenLoai.Contains(searchString)
                                      || l.HangSanXuat.Contains(searchString));
            }

            return await query.OrderBy(l => l.MaLoai).ToListAsync();
        }

        public async Task<LoaiThietBi?> GetByIdAsync(int id)
        {
            return await _context.LoaiThietBis.FindAsync(id);
        }

        public async Task<LoaiThietBi?> GetByIdWithThietBiAsync(int id)
        {
            // Include ThietBis để check ràng buộc trước khi xóa
            return await _context.LoaiThietBis
                .Include(l => l.ThietBis)
                .FirstOrDefaultAsync(l => l.MaLoai == id);
        }

        public async Task<bool> CheckTenLoaiExistsAsync(string tenLoai, int? excludeId = null)
        {
            return await _context.LoaiThietBis
                .AnyAsync(l => l.TenLoai == tenLoai && l.MaLoai != excludeId);
        }

        public async Task AddAsync(LoaiThietBi loai)
        {
            await _context.LoaiThietBis.AddAsync(loai);
        }

        public void Update(LoaiThietBi loai)
        {
            _context.LoaiThietBis.Update(loai);
        }

        public void Delete(LoaiThietBi loai)
        {
            _context.LoaiThietBis.Remove(loai);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}