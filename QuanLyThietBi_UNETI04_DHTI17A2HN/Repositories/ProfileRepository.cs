// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Quản lý khách hàng - CRUD, tìm kiếm, lọc, sắp xếp, phân trang
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public ProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetMaKhachHangByTaiKhoanAsync(int maTaiKhoan)
        {
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);

            return khachHang?.MaKhachHang;
        }

        public async Task<KhachHang?> GetProfileFullDetailsAsync(int maKhachHang)
        {
            return await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .Include(k => k.ThietBis)
                .Include(k => k.PhieuSuaChuas)
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);
        }

        public async Task<KhachHang?> GetByIdAsync(int maKhachHang)
        {
            return await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);
        }

        public async Task<bool> CheckTrungSdtAsync(string sdt, int maKhachHang)
        {
            return await _context.KhachHangs
                .AnyAsync(k => k.SoDienThoai == sdt && k.MaKhachHang != maKhachHang);
        }

        public async Task<bool> CheckTrungEmailAsync(string email, int maKhachHang)
        {
            return await _context.KhachHangs
                .AnyAsync(k => k.Email == email && k.MaKhachHang != maKhachHang);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}