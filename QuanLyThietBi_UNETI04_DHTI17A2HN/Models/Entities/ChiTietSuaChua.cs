// Họ và tên: [Tên SV 5 và SV 4]
// Mã sinh viên: [Mã SV 5, Mã SV 4]
// Nội dung thực hiện: Entity ChiTietSuaChua - Ghi nhận linh kiện, kỹ thuật viên, chẩn đoán, tính tiền

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities
{
    [Table("ChiTietSuaChua")]
    public class ChiTietSuaChua
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã chi tiết")]
        public int MaChiTiet { get; set; }

        [Required]
        [Display(Name = "Phiếu sửa chữa")]
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Kỹ thuật viên thực hiện bắt buộc")]
        [Display(Name = "Kỹ thuật viên")]
        public int MaKyThuatVien { get; set; } // Liên kết đến MaTaiKhoan của Kỹ thuật viên

        [Required(ErrorMessage = "Linh kiện bắt buộc")]
        [Display(Name = "Linh kiện thay thế")]
        public int MaLinhKien { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sử dụng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien { get; set; } = 0;

        [Required(ErrorMessage = "Kết quả chẩn đoán bắt buộc")]
        [StringLength(500)]
        [Display(Name = "Kết quả chẩn đoán")]
        public string KetQuaChanDoan { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("MaPhieu")]
        public virtual PhieuSuaChua? PhieuSuaChua { get; set; }

        [ForeignKey("MaKyThuatVien")]
        public virtual TaiKhoan? KyThuatVien { get; set; }

        [ForeignKey("MaLinhKien")]
        public virtual LinhKien? LinhKien { get; set; }
    }
}