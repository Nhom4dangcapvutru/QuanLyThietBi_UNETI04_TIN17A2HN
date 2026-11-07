// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Cài đặt Repository cho phân hệ Quản lý Tài khoản
// Thực thi các truy vấn LINQ (Tìm kiếm, Lọc, Include bảng liên quan) để phục vụ Controller.
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class TaiKhoanRepository : ITaiKhoanRepository
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaiKhoan>> GetAllAsync(string? searchString, string? vaiTro, string? trangThai)
        {
            var query = _context.TaiKhoans.AsQueryable();

            // Tìm kiếm theo tên đăng nhập / họ tên / email
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.TenDangNhap.Contains(searchString)
                                      || t.HoTen.Contains(searchString)
                                      || t.Email.Contains(searchString));
            }

            // Lọc theo vai trò
            if (!string.IsNullOrEmpty(vaiTro))
                query = query.Where(t => t.VaiTro == vaiTro);

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(t => t.TrangThai == trangThai);

            return await query.OrderBy(t => t.MaTaiKhoan).ToListAsync();
        }

        public async Task<TaiKhoan?> GetByIdAsync(int id)
        {
            // Dùng Include để lấy kèm dữ liệu liên quan phục vụ cho hàm Delete
            return await _context.TaiKhoans
                .Include(t => t.KhachHang)
                .Include(t => t.ChiTietSuaChuas)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
        }

        public async Task<bool> CheckTenDangNhapExistsAsync(string tenDangNhap, int? excludeId = null)
        {
            var name = AccountInput.NormalizeName(tenDangNhap).ToUpperInvariant();
            return await _context.TaiKhoans
                .AnyAsync(t => t.TenDangNhap.Trim().ToUpper() == name && t.MaTaiKhoan != excludeId);
        }

        public async Task<bool> CheckEmailExistsAsync(string email, int? excludeId = null)
        {
            var normalizedEmail = AccountInput.NormalizeEmail(email).ToUpperInvariant();
            return await _context.TaiKhoans
                .AnyAsync(t => t.Email.Trim().ToUpper() == normalizedEmail && t.MaTaiKhoan != excludeId);
        }

        public async Task AddAsync(TaiKhoan taiKhoan)
        {
            taiKhoan.TenDangNhap = AccountInput.NormalizeName(taiKhoan.TenDangNhap);
            taiKhoan.HoTen = AccountInput.NormalizeName(taiKhoan.HoTen);
            taiKhoan.Email = AccountInput.NormalizeEmail(taiKhoan.Email);
            taiKhoan.MatKhau = PasswordSecurity.Hash(taiKhoan, taiKhoan.MatKhau);
            await _context.TaiKhoans.AddAsync(taiKhoan);
        }

        public void Update(TaiKhoan taiKhoan)
        {
            _context.TaiKhoans.Update(taiKhoan);
        }

        public void Delete(TaiKhoan taiKhoan)
        {
            _context.TaiKhoans.Remove(taiKhoan);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}