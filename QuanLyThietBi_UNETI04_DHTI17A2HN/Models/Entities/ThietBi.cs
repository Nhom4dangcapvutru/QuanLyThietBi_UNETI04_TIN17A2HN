// Họ và tên: [Tên SV 2]
// Mã sinh viên: [Mã SV 2]
// Nội dung thực hiện: Entity ThietBi - Quản lý thiết bị, tìm kiếm, lọc, phân trang

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities
{
    [Table("ThietBi")]
    public class ThietBi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã thiết bị")]
        public int MaThietBi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại thiết bị")]
        [Display(Name = "Loại thiết bị")]
        public int MaLoai { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chủ sở hữu thiết bị")]
        [Display(Name = "Khách hàng sở hữu")]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Serial Number không được để trống")]
        [StringLength(50, ErrorMessage = "Serial Number tối đa 50 ký tự")]
        [Display(Name = "Số Serial (S/N)")]
        public string SerialNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên thiết bị không được để trống")]
        [StringLength(150, ErrorMessage = "Tên thiết bị tối đa 150 ký tự")]
        [Display(Name = "Tên thiết bị")]
        public string TenThietBi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày mua không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày mua")]
        public DateTime NgayMua { get; set; }

        [Required(ErrorMessage = "Hạn bảo hành không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn bảo hành")]
        public DateTime HanBaoHanh { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái thiết bị")]
        public string TrangThai { get; set; } = "Hoạt động bình thường";

        [StringLength(255)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá trị (VNĐ)")]
        public decimal GiaTri { get; set; } = 0;

        // Thuộc tính tính toán (không lưu DB): Còn bảo hành hay không
        [NotMapped]
        public bool ConBaoHanh => DateTime.Now.Date <= HanBaoHanh.Date;

        // Navigation properties
        [ForeignKey("MaLoai")]
        public virtual LoaiThietBi? LoaiThietBi { get; set; }

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
    }
}