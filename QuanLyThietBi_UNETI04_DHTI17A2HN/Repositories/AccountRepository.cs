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
    }
}