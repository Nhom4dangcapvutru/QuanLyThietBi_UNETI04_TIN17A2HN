// Họ và tên: [DINH THI THUY LINH]
// Mã sinh viên: [23103100110]
// Nội dung thực hiện: Module 4 - ViewModel Sửa chữa, thêm linh kiện, tính chi phí

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    public class SuaChuaViewModel
    {
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Chọn linh kiện")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn linh kiện hợp lệ")]
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
        [BindNever, ValidateNever]
        public string TenKhachHang { get; set; } = string.Empty;
        [BindNever, ValidateNever]
        public string TenThietBi { get; set; } = string.Empty;
        [BindNever, ValidateNever]
        public string TrangThaiHienTai { get; set; } = string.Empty;
        [BindNever, ValidateNever]
        public List<ChiTietSuaChua> DanhSachChiTiet { get; set; } = new();
        [BindNever, ValidateNever]
        public decimal TongTienLinhKien { get; set; }
        [BindNever, ValidateNever]
        public decimal? ChiPhiDuKien { get; set; }

        [BindNever, ValidateNever]
        public SelectList? LinhKienList { get; set; }
    }
}
