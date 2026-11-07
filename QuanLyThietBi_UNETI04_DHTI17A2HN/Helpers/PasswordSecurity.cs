using Microsoft.AspNetCore.Identity;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;

public static class PasswordSecurity
{
    // Prefix nhận diện định dạng lưu; hash Identity V3 kèm prefix vừa cột nvarchar(100).
    private const string Prefix = "MVC1$";
    private static readonly PasswordHasher<TaiKhoan> Hasher = new();

    public static string Hash(TaiKhoan account, string password)
        => Prefix + Hasher.HashPassword(account, password);

    public static bool Verify(TaiKhoan account, string password, out bool needsRehash)
    {
        needsRehash = false;
        if (account.MatKhau.StartsWith(Prefix, StringComparison.Ordinal))
        {
            try
            {
                var result = Hasher.VerifyHashedPassword(account, account.MatKhau[Prefix.Length..], password);
                needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
                return result != PasswordVerificationResult.Failed;
            }
            catch (FormatException) { return false; }
        }

        // Chuyển tiếp tài khoản cũ: chỉ nâng cấp sau khi xác minh đúng mật khẩu.
        var valid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(account.MatKhau), Encoding.UTF8.GetBytes(password));
        needsRehash = valid;
        return valid;
    }
}
