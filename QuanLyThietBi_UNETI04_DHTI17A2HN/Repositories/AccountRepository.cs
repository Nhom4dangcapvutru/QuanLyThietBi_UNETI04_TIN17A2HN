// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Đăng nhập, đăng ký và lưu đồng bộ tài khoản/hồ sơ khách hàng.
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Common;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using System.Data;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

public class AccountRepository(ApplicationDbContext context, ILogger<AccountRepository>? logger = null) : IAccountRepository
{
    public async Task<TaiKhoan?> LoginAsync(string tenDangNhap, string matKhau)
    {
        var name = AccountInput.NormalizeName(tenDangNhap).ToUpperInvariant();
        var account = await context.TaiKhoans.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenDangNhap.Trim().ToUpper() == name);
        if (account == null || !PasswordSecurity.Verify(account, matKhau, out var needsRehash)) return null;

        if (needsRehash && account.TrangThai == TrangThaiTaiKhoan.HoatDong)
        {
            var oldPassword = account.MatKhau;
            var newHash = PasswordSecurity.Hash(account, matKhau);
            // Không ghi đè nếu Admin vừa đổi mật khẩu hoặc khóa tài khoản ở request khác.
            var updated = await context.TaiKhoans.Where(t => t.MaTaiKhoan == account.MaTaiKhoan
                && t.MatKhau == oldPassword && t.TrangThai == TrangThaiTaiKhoan.HoatDong)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.MatKhau, newHash));
            if (updated == 0) return null;
            account.MatKhau = newHash;
        }
        return account;
    }

    public async Task<RegistrationResult> RegisterAsync(TaiKhoan taiKhoan, KhachHang khachHang)
    {
        taiKhoan.TenDangNhap = AccountInput.NormalizeName(taiKhoan.TenDangNhap);
        taiKhoan.HoTen = AccountInput.NormalizeName(taiKhoan.HoTen);
        taiKhoan.Email = AccountInput.NormalizeEmail(taiKhoan.Email);
        khachHang.HoTen = taiKhoan.HoTen;
        khachHang.Email = taiKhoan.Email;
        khachHang.SoDienThoai = AccountInput.NormalizePhone(khachHang.SoDienThoai);
        taiKhoan.VaiTro = VaiTroConstant.KhachHang;
        taiKhoan.TrangThai = TrangThaiTaiKhoan.HoatDong;
        khachHang.TrangThai = TrangThaiKhachHang.HoatDong;
        khachHang.LoaiKhachHang = LoaiKhachHangConstant.CaNhan;

        try
        {
            // Giữ kiểm tra trùng và cả hai lần lưu trong cùng transaction.
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var errors = await GetRegistrationErrorsAsync(taiKhoan, khachHang);
            if (errors.Count > 0) return RegistrationResult.Failure(errors);

            taiKhoan.MatKhau = PasswordSecurity.Hash(taiKhoan, taiKhoan.MatKhau);
            await context.TaiKhoans.AddAsync(taiKhoan);
            await context.SaveChangesAsync();
            khachHang.MaTaiKhoan = taiKhoan.MaTaiKhoan;
            await context.KhachHangs.AddAsync(khachHang);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return RegistrationResult.Success();
        }
        catch (DbUpdateException ex)
        {
            // Transaction đã được dispose/rollback trước khi vào catch.
            context.ChangeTracker.Clear();
            logger?.LogWarning(ex, "Không thể lưu đăng ký tài khoản.");
            var errors = await GetRegistrationErrorsAsync(taiKhoan, khachHang);
            return RegistrationResult.Failure(errors.Count > 0 ? errors : GeneralError());
        }
        catch (Exception ex)
        {
            context.ChangeTracker.Clear();
            logger?.LogError(ex, "Đăng ký tài khoản thất bại.");
            return RegistrationResult.Failure(GeneralError());
        }
    }

    private async Task<Dictionary<string, string>> GetRegistrationErrorsAsync(TaiKhoan account, KhachHang customer)
    {
        var errors = new Dictionary<string, string>();
        var name = account.TenDangNhap.ToUpperInvariant();
        var email = account.Email.ToUpperInvariant();
        var phone = customer.SoDienThoai;
        var internationalPhone = phone.StartsWith("0", StringComparison.Ordinal) ? "+84" + phone[1..] : phone;
        if (await context.TaiKhoans.AnyAsync(t => t.TenDangNhap.Trim().ToUpper() == name))
            errors[nameof(account.TenDangNhap)] = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
        if (await context.TaiKhoans.AnyAsync(t => t.Email.Trim().ToUpper() == email)
            || await context.KhachHangs.AnyAsync(k => k.Email.Trim().ToUpper() == email))
            errors[nameof(account.Email)] = "Email đã được sử dụng. Nếu đã có hồ sơ khách hàng, vui lòng liên hệ nhân viên để liên kết tài khoản.";
        if (await context.KhachHangs.AnyAsync(k =>
            k.SoDienThoai.Replace(" ", "").Replace("-", "").Replace(".", "").Replace("(", "").Replace(")", "") == phone
            || k.SoDienThoai.Replace(" ", "").Replace("-", "").Replace(".", "").Replace("(", "").Replace(")", "") == internationalPhone))
            errors[nameof(customer.SoDienThoai)] = "Số điện thoại đã được sử dụng. Nếu đã có hồ sơ khách hàng, vui lòng liên hệ nhân viên để liên kết tài khoản.";
        return errors;
    }

    private static Dictionary<string, string> GeneralError()
        => new() { [""] = "Chưa thể đăng ký tài khoản. Vui lòng thử lại sau." };
}
