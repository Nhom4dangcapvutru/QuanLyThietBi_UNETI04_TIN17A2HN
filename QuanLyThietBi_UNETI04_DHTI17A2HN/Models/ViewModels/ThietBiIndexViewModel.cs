// Họ và tên: Nguyễn Văn Tú
// Mã sinh viên: 23103100077
// Nội dung thực hiện: Module 2 - ViewModel truyền dữ liệu Tìm kiếm, Lọc, Sắp xếp và Phân trang

using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Common;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModels
{
    public class ThietBiIndexViewModel
    {
        // 1. Phân trang & Danh sách
        public PaginatedList<ThietBi> DanhSachThietBi { get; set; } = null!;

        // 2. Tìm kiếm (Tên thiết bị, SerialNumber, Tên khách hàng)
        public string? SearchString { get; set; }

        // 3. Lọc theo nhiều tiêu chí kết hợp
        public int? MaLoai { get; set; }
        public string? HangSanXuat { get; set; }
        public string? TrangThaiBaoHanh { get; set; } // "con", "het"
        public string? TrangThaiThietBi { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }

        // 4. Sắp xếp
        public string? SortOrder { get; set; }

        // 5. SelectList để hiển thị Dropdown trên View
        public SelectList? LoaiThietBiList { get; set; }
        public SelectList? HangSanXuatList { get; set; }
        public SelectList? TrangThaiList { get; set; }
    }
}