// Họ và tên: [DINH THI THUY LINH]
// Mã sinh viên: [23103100110]
// Nội dung thực hiện: Module 4 - ViewModel Tiếp nhận phiếu sửa chữa

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModels
{
    public class TiepNhanViewModel
    {
        public int MaPhieu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mức độ ưu tiên")]
        [Display(Name = "Mức độ ưu tiên")]
        public string MucDo { get; set; } = "Trung bình";

        [StringLength(500)]
        [Display(Name = "Ghi chú tiếp nhận")]
        public string? GhiChu { get; set; }

        public List<SelectListItem> DanhSachMucDo { get; set; } = new();
    }
}