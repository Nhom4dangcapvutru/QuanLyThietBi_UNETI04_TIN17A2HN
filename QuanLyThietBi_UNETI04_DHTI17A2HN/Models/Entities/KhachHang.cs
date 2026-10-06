// Họ và tên: [Tên SV 3]
// Mã sinh viên: [Mã SV 3]
// Nội dung thực hiện: Entity KhachHang - Quản lý thông tin khách hàng, hồ sơ cá nhân

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã khách hàng")]
        public int MaKhachHang { get; set; }

        // Nullable nếu khách hàng vãng lai chưa có tài khoản web
        [Display(Name = "Tài khoản liên kết")]
        public int? MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên khách hàng bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(20)]
        [Display(Name = "Số CCCD/CMND")]
        public string? CCCD { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        [Display(Name = "Loại khách hàng")]
        public string LoaiKhachHang { get; set; } = "Cá nhân"; // Cá nhân, Doanh nghiệp

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Hoạt động";

        [StringLength(255)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [StringLength(20)]
        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }

        // Navigation properties
        [ForeignKey("MaTaiKhoan")]
        public virtual TaiKhoan? TaiKhoan { get; set; }

        public virtual ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
        public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
    }
}