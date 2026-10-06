// Họ và tên: [Tên SV 3 và SV 4]
// Mã sinh viên: [Mã SV 3, Mã SV 4]
// Nội dung thực hiện: Entity PhieuSuaChua - Luồng tiếp nhận, sửa chữa, trạng thái nghiệp vụ

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities
{
    [Table("PhieuSuaChua")]
    public class PhieuSuaChua
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã phiếu")]
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Thiết bị bắt buộc chọn")]
        [Display(Name = "Thiết bị")]
        public int MaThietBi { get; set; }

        [Required(ErrorMessage = "Khách hàng bắt buộc")]
        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        [Required]
        [Display(Name = "Ngày tiếp nhận")]
        public DateTime NgayTiepNhan { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng mô tả nội dung lỗi gặp phải")]
        [StringLength(1000)]
        [Display(Name = "Nội dung lỗi")]
        public string NoiDungLoi { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái phiếu")]
        public string TrangThai { get; set; } = "Chờ tiếp nhận"; // Chờ tiếp nhận -> Đang xử lý -> Hoàn thành (hoặc Đã hủy, Từ chối)

        [Required]
        [StringLength(50)]
        [Display(Name = "Mức độ ưu tiên")]
        public string MucDo { get; set; } = "Bình thường"; // Thấp, Bình thường, Cao, Khẩn cấp

        [DataType(DataType.Date)]
        [Display(Name = "Hạn dự kiến xong")]
        public DateTime? HanDuKien { get; set; }

        [Display(Name = "Ngày hoàn thành")]
        public DateTime? NgayHoanThanh { get; set; }

        // Navigation properties
        [ForeignKey("MaThietBi")]
        public virtual ThietBi? ThietBi { get; set; }

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        public virtual ICollection<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();
    }
}