// Họ và tên: [Tên SV 1]
// Mã sinh viên: [Mã SV 1]
// Nội dung thực hiện: Entity LoaiThietBi - Quản lý danh mục loại thiết bị, dữ liệu nền

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities
{
    [Table("LoaiThietBi")]
    public class LoaiThietBi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã loại")]
        public int MaLoai { get; set; }

        [Required(ErrorMessage = "Tên loại thiết bị không được để trống")]
        [StringLength(100, ErrorMessage = "Tên loại tối đa 100 ký tự")]
        [Display(Name = "Tên loại thiết bị")]
        public string TenLoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hãng sản xuất không được để trống")]
        [StringLength(100)]
        [Display(Name = "Hãng sản xuất")]
        public string HangSanXuat { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Thời hạn bảo hành mặc định bắt buộc")]
        [Range(1, 120, ErrorMessage = "Thời hạn bảo hành từ 1 đến 120 tháng")]
        [Display(Name = "Bảo hành mặc định (tháng)")]
        public int ThoiHanBaoHanhMacDinh { get; set; } = 12;

        // Navigation property (1 Loại có nhiều Thiết bị)
        public virtual ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
    }
}