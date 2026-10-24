// Họ và tên: [Dinh Thi Thuy Linh]
// Mã sinh viên: [23103100110]
// Nội dung thực hiện: Module 4 - ViewModel Cập nhật trạng thái và chi phí cuối cùng

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    public class CapNhatTrangThaiViewModel
    {
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
        [Display(Name = "Trạng thái mới")]
        public string TrangThaiMoi { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "1000000000000", ErrorMessage = "Tiền công phải từ 0 đến 1.000.000.000.000 VNĐ")]
        [Display(Name = "Tiền công sửa chữa (VNĐ)")]
        public decimal? TienCong { get; set; }

        [Range(typeof(decimal), "0", "1000000000000", ErrorMessage = "Chi phí khác phải từ 0 đến 1.000.000.000.000 VNĐ")]
        [Display(Name = "Chi phí khác (VNĐ)")]
        public decimal? ChiPhiKhac { get; set; }

        [StringLength(500)]
        [Display(Name = "Nội dung chi phí khác")]
        public string? NoiDungChiPhiKhac { get; set; }

        [Range(typeof(decimal), "0", "1000000000000", ErrorMessage = "Giảm trừ phải từ 0 đến 1.000.000.000.000 VNĐ")]
        [Display(Name = "Giảm trừ bảo hành / ưu đãi (VNĐ)")]
        public decimal? GiamTru { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do giảm trừ")]
        public string? LyDoGiamTru { get; set; }

        [Display(Name = "Chi phí dự kiến (tham khảo)")]
        [BindNever, ValidateNever]
        public decimal? ChiPhiDuKienHienThi { get; set; }

        [Display(Name = "Ngày hẹn trả khách")]
        public DateTime? NgayHenTra { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do hủy / từ chối")]
        public string? LyDoHuy { get; set; }

        // Thông tin hiển thị
        [BindNever, ValidateNever]
        public string TenKhachHang { get; set; } = string.Empty;
        [BindNever, ValidateNever]
        public string TenThietBi { get; set; } = string.Empty;
        [BindNever, ValidateNever]
        public string SerialNumber { get; set; } = string.Empty;
        [BindNever, ValidateNever]
        public string TrangThaiHienTai { get; set; } = string.Empty;
        [BindNever, ValidateNever]
        public bool ConBaoHanh { get; set; }
        [BindNever, ValidateNever]
        public decimal TongTienLinhKien { get; set; }
        [BindNever, ValidateNever]
        public List<SelectListItem> DanhSachTrangThaiChoPhep { get; set; } = new();
    }
}
