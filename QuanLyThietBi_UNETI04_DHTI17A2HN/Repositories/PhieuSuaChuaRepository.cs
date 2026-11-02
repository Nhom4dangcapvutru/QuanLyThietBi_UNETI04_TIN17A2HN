// Họ và tên: Đinh Thị Thùy Linh
// Mã sinh viên: 23103100110
// Nội dung thực hiện: Tiếp nhận - Chẩn đoán - Sửa chữa - Cập nhật trạng thái và chi phí

// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Quản lý khách hàng - CRUD, tìm kiếm, lọc, sắp xếp, phân trang

using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class PhieuSuaChuaRepository : IPhieuSuaChuaRepository
    {
        private readonly ApplicationDbContext _context;

        public PhieuSuaChuaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetMaKhachHangByTaiKhoanAsync(int maTaiKhoan)
        {
            var kh = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);
            return kh?.MaKhachHang;
        }

        public async Task<List<ThietBi>> GetThietBiChoPhepTaoPhieuAsync(int? maKhachHang, bool isAdminOrKtv)
        {
            var query = _context.ThietBis.AsNoTracking()
                .Include(t => t.KhachHang)
                .Where(t => t.KhachHang != null
                    && t.KhachHang.TrangThai == TrangThaiKhachHang.HoatDong
                    && (t.TrangThai == TrangThaiThietBi.HoatDong || t.TrangThai == TrangThaiThietBi.Hong)
                    && !t.PhieuSuaChuas.Any(p => p.TrangThai == TrangThaiPhieu.ChoTiepNhan
                        || p.TrangThai == TrangThaiPhieu.DangXuLy));

            if (!isAdminOrKtv && maKhachHang.HasValue)
            {
                query = query.Where(t => t.MaKhachHang == maKhachHang.Value);
            }

            return await query.OrderBy(t => t.TenThietBi).ToListAsync();
        }

        public async Task<ThietBi?> GetThietBiKemKhachHangAsync(int maThietBi)
        {
            return await _context.ThietBis
                .Include(t => t.KhachHang)
                .FirstOrDefaultAsync(t => t.MaThietBi == maThietBi);
        }

        public async Task<bool> KiemTraKhachHangHoatDongAsync(int maKhachHang)
        {
            return await _context.KhachHangs.AnyAsync(k => k.MaKhachHang == maKhachHang
                && k.TrangThai == TrangThaiKhachHang.HoatDong);
        }

        public async Task<bool> CoPhieuDangXuLyAsync(int maThietBi)
        {
            return await _context.PhieuSuaChuas
                .AnyAsync(p => p.MaThietBi == maThietBi
                            && (p.TrangThai == TrangThaiPhieu.ChoTiepNhan
                             || p.TrangThai == TrangThaiPhieu.DangXuLy));
        }

        public async Task<(List<PhieuSuaChua> Data, int TotalCount)> GetFilteredPhieuAsync(
            int? maKhachHang, bool isAdminOrKtv, string? tuKhoa, string? trangThai, string? sapXep, int page, int pageSize)
        {
            var query = _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .AsQueryable();

            if (!isAdminOrKtv && maKhachHang.HasValue)
            {
                query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(p =>
                    p.NoiDungLoi.Contains(tuKhoa) ||
                    (p.ThietBi != null && p.ThietBi.TenThietBi.Contains(tuKhoa)) ||
                    (p.ThietBi != null && p.ThietBi.SerialNumber.Contains(tuKhoa)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            query = sapXep switch
            {
                "ngay_asc" => query.OrderBy(p => p.NgayTiepNhan),
                "ngay_desc" => query.OrderByDescending(p => p.NgayTiepNhan),
                "trangthai" => query.OrderBy(p => p.TrangThai).ThenByDescending(p => p.NgayTiepNhan),
                _ => query.OrderByDescending(p => p.NgayTiepNhan)
            };

            int totalCount = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<PhieuSuaChua?> GetPhieuDetailsAsync(int? id)
        {
            return await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                    .ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietSuaChuas)
                    .ThenInclude(c => c.LinhKien)
                .Include(p => p.ChiTietSuaChuas)
                    .ThenInclude(c => c.KyThuatVien)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);
        }

        public async Task<PhieuSuaChua?> GetPhieuByIdAsync(int? id)
        {
            return await _context.PhieuSuaChuas.FirstOrDefaultAsync(p => p.MaPhieu == id);
        }

        public async Task AddAsync(PhieuSuaChua phieu)
        {
            await _context.PhieuSuaChuas.AddAsync(phieu);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<(List<PhieuSuaChua> Items, int TotalItems)> GetPagedForProcessingAsync(
            string? searchString, string? trangThai, string? mucDo,
            int? maLoaiThietBi, DateTime? tuNgay, DateTime? denNgay,
            string? sortOrder, int pageIndex, int pageSize)
        {
            var query = _context.PhieuSuaChuas
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.KyThuatVienTiepNhan)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                query = query.Where(p =>
                    p.KhachHang!.HoTen.Contains(s) ||
                    p.ThietBi!.TenThietBi.Contains(s) ||
                    p.ThietBi.SerialNumber.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(p => p.TrangThai == trangThai);

            if (!string.IsNullOrWhiteSpace(mucDo))
                query = query.Where(p => p.MucDo == mucDo);

            if (maLoaiThietBi.HasValue && maLoaiThietBi.Value > 0)
                query = query.Where(p => p.ThietBi!.MaLoai == maLoaiThietBi.Value);

            if (tuNgay.HasValue)
                query = query.Where(p => p.NgayTiepNhan.Date >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(p => p.NgayTiepNhan.Date <= denNgay.Value.Date);

            query = sortOrder switch
            {
                "date_desc" => query.OrderByDescending(p => p.NgayTiepNhan),
                "name_asc" => query.OrderBy(p => p.KhachHang!.HoTen),
                "name_desc" => query.OrderByDescending(p => p.KhachHang!.HoTen),
                "status_asc" => query.OrderBy(p => p.TrangThai),
                "status_desc" => query.OrderByDescending(p => p.TrangThai),
                _ => query.OrderByDescending(p => p.NgayTiepNhan)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<PhieuSuaChua?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.PhieuSuaChuas
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.KyThuatVienTiepNhan)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.LinhKien)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.KyThuatVien)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieu == id);
        }

        public async Task<PhieuSuaChua?> GetByIdForUpdateAsync(int id, bool includeChiTiet = false)
        {
            var query = _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .AsQueryable();  

            if (includeChiTiet)
            {
                query = query.Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.LinhKien);
            }

            return await query.FirstOrDefaultAsync(p => p.MaPhieu == id);
        }

        public async Task<LinhKien?> GetLinhKienForUpdateAsync(int maLinhKien)
        { 
            return await _context.LinhKiens.FirstOrDefaultAsync(l => l.MaLinhKien == maLinhKien);
        }

        public async Task<List<LoaiThietBi>> GetLoaiThietBisAsync()
        {
            return await _context.LoaiThietBis.AsNoTracking().ToListAsync();
        } 
        public async Task<List<LinhKien>> GetAllLinhKiensAsync()
        {
            return await _context.LinhKiens.AsNoTracking().ToListAsync();
        }
    }
}