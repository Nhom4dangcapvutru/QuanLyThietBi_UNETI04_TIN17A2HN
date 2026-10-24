// Họ và tên:
// Nội dung thực hiện: Entity PhieuSuaChua bổ sung trường
// phục vụ Tiếp nhận, Chẩn đoán, Sửa chữa, Cập nhật trạng thái và chi phí

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
        public string TrangThai { get; set; } = "Chờ tiếp nhận";

        [Required]
        [StringLength(50)]
        [Display(Name = "Mức độ ưu tiên")]
        public string MucDo { get; set; } = "Trung bình";

        [DataType(DataType.Date)]
        [Display(Name = "Hạn dự kiến xong")]
        public DateTime? HanDuKien { get; set; }

        [Display(Name = "Ngày cập nhật cuối")]
        public DateTime? NgayCapNhat { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng chi phí phải >= 0")]
        [Display(Name = "Tổng chi phí thực tế (VNĐ)")]
        public decimal? TongChiPhi { get; set; }

        // Nullable để nhận biết phiếu cũ chưa có phân tích chi phí, không tự suy diễn tiền công.
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TienLinhKienChot { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TienCong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ChiPhiKhac { get; set; }

        [StringLength(500)]
        public string? NoiDungChiPhiKhac { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiamTru { get; set; }

        [StringLength(500)]
        public string? LyDoGiamTru { get; set; }

        [NotMapped]
        public bool CoPhanTichChiPhi => TienLinhKienChot.HasValue && TienCong.HasValue
            && ChiPhiKhac.HasValue && GiamTru.HasValue && TongChiPhi.HasValue;

        [Display(Name = "Ngày hoàn thành")]
        public DateTime? NgayHoanThanh { get; set; }

        // ===== BỔ SUNG CHO MODULE 4 =====
        [Display(Name = "Kỹ thuật viên tiếp nhận")]
        public int? MaKyThuatVienTiepNhan { get; set; }

        [StringLength(1000)]
        [Display(Name = "Kết quả chẩn đoán")]
        public string? KetQuaChanDoan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Chi phí dự kiến phải >= 0")]
        [Display(Name = "Chi phí dự kiến (VNĐ)")]
        public decimal? ChiPhiDuKien { get; set; }

        //[Column(TypeName = "decimal(18,2)")]
        //[Range(0, double.MaxValue, ErrorMessage = "Tổng chi phí phải >= 0")]
        //[Display(Name = "Tổng chi phí thực tế (VNĐ)")]
        //public decimal? TongChiPhi { get; set; }

        [Display(Name = "Ngày hẹn trả khách")]
        public DateTime? NgayHenTra { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do hủy / từ chối")]
        public string? LyDoHuy { get; set; }

        //[Display(Name = "Ngày cập nhật cuối")]
        //public DateTime? NgayCapNhat { get; set; }

        // Navigation properties
        [ForeignKey("MaThietBi")]
        public virtual ThietBi? ThietBi { get; set; }

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MaKyThuatVienTiepNhan")]
        public virtual TaiKhoan? KyThuatVienTiepNhan { get; set; }

        public virtual ICollection<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();
    }
}
