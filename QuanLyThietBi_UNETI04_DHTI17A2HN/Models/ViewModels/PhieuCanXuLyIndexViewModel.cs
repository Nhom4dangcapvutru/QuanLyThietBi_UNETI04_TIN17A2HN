// Họ và tên: [DINH THI THUY LINH]
// Mã sinh viên: [23103100110]
// Nội dung thực hiện: Module 4 - ViewModel danh sách phiếu cần xử lý
// (Tìm kiếm, Lọc, Sắp xếp, Phân trang)

using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModels
{
    public class PhieuCanXuLyIndexViewModel
    {
        public List<PhieuSuaChua> DanhSachPhieu { get; set; } = new();

        // Tìm kiếm
        public string? SearchString { get; set; }

        // Lọc
        public string? TrangThai { get; set; }
        public string? MucDo { get; set; }
        public int? MaLoaiThietBi { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }

        // Sắp xếp
        public string? SortOrder { get; set; }

        // Phân trang
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Dropdown
        public SelectList? TrangThaiList { get; set; }
        public SelectList? MucDoList { get; set; }
        public SelectList? LoaiThietBiList { get; set; }
    }
}