// Họ và tên: [DINH THI THUY LINH]
// Mã sinh viên: [23103100110]
// Nội dung thực hiện: Module 4 - ViewModel Sửa chữa, thêm linh kiện, tính chi phí

using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModels
{
    public class SuaChuaViewModel
    {
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Chọn linh kiện")]
        [Display(Name = "Linh kiện")]
        public int MaLinhKien { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; } = 1;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Thông tin hiển thị
        public string TenKhachHang { get; set; } = string.Empty;
        public string TenThietBi { get; set; } = string.Empty;
        public string TrangThaiHienTai { get; set; } = string.Empty;
        public List<ChiTietSuaChua> DanhSachChiTiet { get; set; } = new();
        public decimal TongTienLinhKien { get; set; }
        public decimal? ChiPhiDuKien { get; set; }

        public SelectList? LinhKienList { get; set; }
    }
}