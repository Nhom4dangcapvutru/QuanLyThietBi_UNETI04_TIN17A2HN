// Họ và tên: Nguyễn Văn Tú
// Mã sinh viên: 23103100077
// Nội dung thực hiện: Module 2 - Tham số tìm kiếm, lọc, sắp xếp và phân trang thiết bị

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class ThietBiFilter
    {
        public string? SearchString { get; set; }
        public int? MaLoai { get; set; }
        public string? HangSanXuat { get; set; }
        public string? TrangThaiBaoHanh { get; set; }
        public string? TrangThaiThietBi { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }
        public string? SortOrder { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 6;
    }
}
