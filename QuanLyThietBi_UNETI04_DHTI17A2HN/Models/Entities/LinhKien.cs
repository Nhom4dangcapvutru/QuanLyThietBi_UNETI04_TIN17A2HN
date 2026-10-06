// Họ và tên: [Tên SV 5]
// Mã sinh viên: [Mã SV 5]
// Nội dung thực hiện: Entity LinhKien - Quản lý kho linh kiện thay thế

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities
{
    [Table("LinhKien")]
    public class LinhKien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã linh kiện")]
        public int MaLinhKien { get; set; }

        [Required(ErrorMessage = "Tên linh kiện không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên linh kiện")]
        public string TenLinhKien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại linh kiện bắt buộc")]
        [StringLength(100)]
        [Display(Name = "Loại linh kiện")]
        public string LoaiLinhKien { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn giá bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải >= 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá (VNĐ)")]
        public decimal DonGia { get; set; } = 0;

        [Required(ErrorMessage = "Số lượng tồn bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn phải >= 0")]
        [Display(Name = "Số lượng tồn")]
        public int SoLuongTon { get; set; } = 0;

        [Required]
        [StringLength(30)]
        [Display(Name = "Đơn vị tính")]
        public string DonViTinh { get; set; } = "Cái"; // Cái, Bộ, Chiếc...

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Còn hàng";

        // Navigation property
        public virtual ICollection<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();
    }
}