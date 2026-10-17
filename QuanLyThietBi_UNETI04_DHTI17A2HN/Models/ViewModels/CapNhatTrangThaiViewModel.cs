// Họ và tên: [Dinh Thi Thuy Linh]
// Mã sinh viên: [23103100110]
// Nội dung thực hiện: Module 4 - ViewModel Cập nhật trạng thái và chi phí cuối cùng

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModels
{
    public class CapNhatTrangThaiViewModel
    {
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
        [Display(Name = "Trạng thái mới")]
        public string TrangThaiMoi { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Tổng chi phí phải >= 0")]
        [Display(Name = "Tổng chi phí thực tế (VNĐ)")]
        public decimal? TongChiPhi { get; set; }

        [Display(Name = "Chi phí dự kiến (tham khảo)")]
        public decimal? ChiPhiDuKienHienThi { get; set; }

        [Display(Name = "Ngày hẹn trả khách")]
        public DateTime? NgayHenTra { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do hủy / từ chối")]
        public string? LyDoHuy { get; set; }

        // Thông tin hiển thị
        public string TenKhachHang { get; set; } = string.Empty;
        public string TenThietBi { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string TrangThaiHienTai { get; set; } = string.Empty;
        public bool ConBaoHanh { get; set; }
        public decimal TongTienLinhKien { get; set; }
        public List<SelectListItem> DanhSachTrangThaiChoPhep { get; set; } = new();
    }
}