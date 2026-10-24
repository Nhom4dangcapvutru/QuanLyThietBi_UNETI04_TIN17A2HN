// Họ và tên: [DINH THI THUY LINH]
// Mã sinh viên: [23103100110]
// Nội dung thực hiện: Module 4 - ViewModel Chẩn đoán và báo chi phí dự kiến

using System.ComponentModel.DataAnnotations;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    public class ChanDoanViewModel
    {
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập kết quả chẩn đoán")]
        [StringLength(1000)]
        [Display(Name = "Kết quả chẩn đoán")]
        public string KetQuaChanDoan { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Chi phí dự kiến phải >= 0")]
        [Display(Name = "Chi phí dự kiến (VNĐ)")]
        public decimal? ChiPhiDuKien { get; set; }

        [Display(Name = "Hạn dự kiến hoàn thành")]
        public DateTime? HanDuKien { get; set; }

        public string TenKhachHang { get; set; } = string.Empty;
        public string TenThietBi { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string NoiDungLoi { get; set; } = string.Empty;
        public string TrangThaiHienTai { get; set; } = string.Empty;
        public bool ConBaoHanh { get; set; }
    }
}