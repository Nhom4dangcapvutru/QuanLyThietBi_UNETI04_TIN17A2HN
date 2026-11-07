using System.ComponentModel.DataAnnotations;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

public class TaiKhoanEditViewModel
{
    public int MaTaiKhoan { get; set; }
    private string _tenDangNhap = "", _hoTen = "", _email = "";

    [Required, StringLength(50, MinimumLength = 3)]
    public string TenDangNhap { get => _tenDangNhap; set => _tenDangNhap = AccountInput.NormalizeName(value); }

    [DataType(DataType.Password), StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới từ 6 đến 100 ký tự")]
    public string? MatKhau { get; set; }

    [Required, StringLength(100)]
    public string HoTen { get => _hoTen; set => _hoTen = AccountInput.NormalizeName(value); }

    [Required, EmailAddress, StringLength(100)]
    public string Email { get => _email; set => _email = AccountInput.NormalizeEmail(value); }

    [Required, RegularExpression("^(Admin|KyThuatVien|KhachHang)$")]
    public string VaiTro { get; set; } = "";

    [Required, RegularExpression("^(Hoạt động|Bị khóa)$")]
    public string TrangThai { get; set; } = "";
}
