// Họ và tên: Nguyễn Văn Tú
// Mã sinh viên: 23103100077
// Nội dung thực hiện: Module 2 - Repository EF Core cho CRUD, phân quyền dữ liệu, tìm kiếm, lọc, sắp xếp và phân trang

using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Common;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class ThietBiRepository : IThietBiRepository
    {
        private readonly ApplicationDbContext _context;

        public ThietBiRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<ThietBi> QueryThietBiDuocPhepXem(string? vaiTro, int? maTaiKhoan)
        {
            var query = _context.ThietBis.AsNoTracking();

            if (vaiTro == VaiTroConstant.Admin || vaiTro == VaiTroConstant.KyThuatVien)
                return query;

            if (vaiTro != VaiTroConstant.KhachHang || !maTaiKhoan.HasValue)
                return query.Where(t => false);

            // Lọc tại Database bằng tài khoản trong Session, không nhận mã khách từ URL.
            return query.Where(t => t.KhachHang != null
                && t.KhachHang.MaTaiKhoan == maTaiKhoan.Value);
        }

        public async Task<PaginatedList<ThietBi>> SearchAsync(
            ThietBiFilter filter, string? vaiTro, int? maTaiKhoan)
        {
            // Truy vấn gốc
            var query = QueryThietBiDuocPhepXem(vaiTro, maTaiKhoan)
                .Include(t => t.LoaiThietBi)
                .Include(t => t.KhachHang)
                .AsNoTracking()
                .AsQueryable();

            // A. TÌM KIẾM (Tên thiết bị, Serial, Tên khách hàng)
            if (!string.IsNullOrWhiteSpace(filter.SearchString))
            {
                var search = filter.SearchString.Trim();
                query = query.Where(t => t.TenThietBi.Contains(search)
                                      || t.SerialNumber.Contains(search)
                                      || t.KhachHang!.HoTen.Contains(search));
            }

            // B. LỌC KẾT HỢP
            if (filter.MaLoai.HasValue && filter.MaLoai.Value > 0)
            {
                query = query.Where(t => t.MaLoai == filter.MaLoai.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.HangSanXuat))
            {
                query = query.Where(t => t.LoaiThietBi!.HangSanXuat == filter.HangSanXuat);
            }

            if (filter.TrangThaiBaoHanh == "con")
            {
                query = query.Where(t => t.HanBaoHanh.Date >= DateTime.Now.Date);
            }
            else if (filter.TrangThaiBaoHanh == "het")
            {
                query = query.Where(t => t.HanBaoHanh.Date < DateTime.Now.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.TrangThaiThietBi))
            {
                query = query.Where(t => t.TrangThai == filter.TrangThaiThietBi);
            }

            if (filter.GiaTu.HasValue)
            {
                query = query.Where(t => t.GiaTri >= filter.GiaTu.Value);
            }

            if (filter.GiaDen.HasValue)
            {
                query = query.Where(t => t.GiaTri <= filter.GiaDen.Value);
            }

            // C. SẮP XẾP BẰNG LINQ
            query = filter.SortOrder switch
            {
                "name_desc" => query.OrderByDescending(t => t.TenThietBi),
                "date_buy_asc" => query.OrderBy(t => t.NgayMua),
                "date_buy_desc" => query.OrderByDescending(t => t.NgayMua),
                "warranty_asc" => query.OrderBy(t => t.HanBaoHanh),
                "warranty_desc" => query.OrderByDescending(t => t.HanBaoHanh),
                "price_asc" => query.OrderBy(t => t.GiaTri),
                "price_desc" => query.OrderByDescending(t => t.GiaTri),
                _ => query.OrderBy(t => t.TenThietBi)
            };

            // D. PHÂN TRANG
            return await PaginatedList<ThietBi>.CreateAsync(query, filter.PageIndex, filter.PageSize);
        }

        public async Task<ThietBi?> GetDetailsAsync(int id, string? vaiTro, int? maTaiKhoan)
        {
            var isStaff = vaiTro == VaiTroConstant.Admin || vaiTro == VaiTroConstant.KyThuatVien;
            return await QueryThietBiDuocPhepXem(vaiTro, maTaiKhoan)
                .Include(t => t.LoaiThietBi)
                .Include(t => t.KhachHang)
                // Thiết bị từng đổi chủ không được làm lộ lịch sử của khách hàng cũ.
                .Include(t => t.PhieuSuaChuas.Where(p => isStaff
                    || (p.KhachHang != null && p.KhachHang.MaTaiKhoan == maTaiKhoan)))
                .FirstOrDefaultAsync(t => t.MaThietBi == id);
        }

        public Task<ThietBi?> GetByIdAsync(int id)
            => _context.ThietBis.AsNoTracking().FirstOrDefaultAsync(t => t.MaThietBi == id);

        public Task<bool> SerialExistsAsync(string serialNumber, int? excludedId = null)
            => _context.ThietBis.AnyAsync(t => t.SerialNumber == serialNumber
                && (!excludedId.HasValue || t.MaThietBi != excludedId.Value));

        public Task<bool> ExistsAsync(int id)
            => _context.ThietBis.AnyAsync(t => t.MaThietBi == id);

        public Task<int> CountHistoryAsync(int id)
            => _context.PhieuSuaChuas.CountAsync(p => p.MaThietBi == id);

        // Chỉ đọc dữ liệu cho dropdown của Module 2; không sửa nghiệp vụ module khác.
        public Task<List<LoaiThietBi>> GetLoaiThietBisAsync()
            => _context.LoaiThietBis.AsNoTracking().ToListAsync();

        public Task<List<KhachHang>> GetKhachHangsAsync()
            => _context.KhachHangs.AsNoTracking().ToListAsync();

        public async Task AddAsync(ThietBi thietBi)
        {
            _context.ThietBis.Add(thietBi);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(ThietBi thietBi)
        {
            try
            {
                _context.ThietBis.Update(thietBi);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(thietBi.MaThietBi)) return false;
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var thietBi = await _context.ThietBis.FindAsync(id);
            if (thietBi == null) return false;

            _context.ThietBis.Remove(thietBi);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
