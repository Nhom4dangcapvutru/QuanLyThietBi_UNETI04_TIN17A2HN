// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Cài đặt Repository cho phân hệ Đăng nhập (Account)
// Áp dụng mẫu thiết kế Repository Pattern để tách biệt logic truy vấn CSDL khỏi Controller.
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;
        public AccountRepository(ApplicationDbContext context) => _context = context;

        public async Task<TaiKhoan?> LoginAsync(string tenDangNhap, string matKhau)
        {
            return await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap && t.MatKhau == matKhau);
        }
        public async Task<bool> RegisterAsync(TaiKhoan taiKhoan, KhachHang khachHang)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lưu Tài khoản trước để lấy MaTaiKhoan (tự tăng)
                await _context.TaiKhoans.AddAsync(taiKhoan);
                await _context.SaveChangesAsync();

                // 2. Gán MaTaiKhoan vừa tạo cho Khách hàng
                khachHang.MaTaiKhoan = taiKhoan.MaTaiKhoan;

                // 3. Lưu Khách hàng
                await _context.KhachHangs.AddAsync(khachHang);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}