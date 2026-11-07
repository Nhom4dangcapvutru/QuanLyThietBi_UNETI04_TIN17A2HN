// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: ViewModel cho chức năng Đăng ký tài khoản khách hàng.
// Chứa các trường thông tin cần thiết và các quy tắc Validation (Data Annotation).

using System.ComponentModel.DataAnnotations;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModels
{
    public class RegisterViewModel
    {
        private string _tenDangNhap = "", _hoTen = "", _email = "", _soDienThoai = "";
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập từ 3-50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get => _tenDangNhap; set => _tenDangNhap = AccountInput.NormalizeName(value); }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu từ 8 đến 100 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*[0-9]).+$", ErrorMessage = "Mật khẩu phải có chữ và số")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare(nameof(MatKhau), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string XacNhanMatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get => _hoTen; set => _hoTen = AccountInput.NormalizeName(value); }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get => _email; set => _email = AccountInput.NormalizeEmail(value); }

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get => _soDienThoai; set => _soDienThoai = AccountInput.NormalizePhone(value); }
    }
}
