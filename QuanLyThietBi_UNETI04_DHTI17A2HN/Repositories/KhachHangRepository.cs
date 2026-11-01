// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Quản lý khách hàng - CRUD, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class KhachHangRepository : IKhachHangRepository
    {
        private readonly ApplicationDbContext _context;

        public KhachHangRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<KhachHang> Data, int TotalCount)> GetFilteredAsync(
            string? tuKhoa, string? loaiKhachHang, string? trangThai, string? sapXep, int page, int pageSize)
        {
            var query = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(k =>
                    k.HoTen.Contains(tuKhoa) ||
                    k.SoDienThoai.Contains(tuKhoa) ||
                    k.Email.Contains(tuKhoa) ||
                    (k.CCCD != null && k.CCCD.Contains(tuKhoa)));
            }

            // Lọc
            if (!string.IsNullOrWhiteSpace(loaiKhachHang))
                query = query.Where(k => k.LoaiKhachHang == loaiKhachHang);

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(k => k.TrangThai == trangThai);

            // Sắp xếp
            query = sapXep switch
            {
                "ten_asc" => query.OrderBy(k => k.HoTen),
                "ten_desc" => query.OrderByDescending(k => k.HoTen),
                "ngaytao_asc" => query.OrderBy(k => k.NgayTao),
                "ngaytao_desc" => query.OrderByDescending(k => k.NgayTao),
                _ => query.OrderBy(k => k.MaKhachHang)
            };

            // Đếm tổng số bản ghi trước khi phân trang
            int totalCount = await query.CountAsync();

            // Phân trang
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<KhachHang?> GetDetailsByIdAsync(int? id)
        {
            return await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .Include(k => k.ThietBis)
                .Include(k => k.PhieuSuaChuas)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);
        }

        public async Task<KhachHang?> GetByIdAsync(int? id)
        {
            return await _context.KhachHangs.FindAsync(id);
        }

        public async Task AddAsync(KhachHang khachHang)
        {
            await _context.AddAsync(khachHang);
        }

        public async Task UpdateAsync(KhachHang khachHang)
        {
            _context.Update(khachHang);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(KhachHang khachHang)
        {
            _context.KhachHangs.Remove(khachHang);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.KhachHangs.AnyAsync(e => e.MaKhachHang == id);
        }

        // Các hàm Validate
        public async Task<bool> CheckTrungSdtAsync(string sdt, int? excludeId = null)
        {
            return await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == sdt && (excludeId == null || k.MaKhachHang != excludeId));
        }

        public async Task<bool> CheckTrungEmailAsync(string email, int? excludeId = null)
        {
            return await _context.KhachHangs.AnyAsync(k => k.Email == email && (excludeId == null || k.MaKhachHang != excludeId));
        }

        public async Task<bool> CheckTrungCccdAsync(string cccd, int? excludeId = null)
        {
            return await _context.KhachHangs.AnyAsync(k => k.CCCD == cccd && (excludeId == null || k.MaKhachHang != excludeId));
        }
    }
}