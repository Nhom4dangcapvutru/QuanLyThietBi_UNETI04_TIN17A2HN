// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: ViewModel cho trang danh sách khách hàng
// (Hỗ trợ tìm kiếm, lọc, sắp xếp và phân trang)

using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    /// <summary>
    /// ViewModel dùng cho trang Index (danh sách khách hàng).
    /// Chứa:
    ///   - Danh sách khách hàng của trang hiện tại (đã phân trang)
    ///   - Các tiêu chí tìm kiếm/lọc/sắp xếp (giữ lại khi chuyển trang)
    ///   - Thông tin phân trang (tổng số trang, trang hiện tại...)
    /// </summary>
    public class KhachHangFilterViewModel
    {
        // ================================================================
        // 1. DỮ LIỆU HIỂN THỊ (danh sách khách hàng của trang hiện tại)
        // ================================================================
        public List<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();

        // ================================================================
        // 2. TIÊU CHÍ TÌM KIẾM / LỌC / SẮP XẾP
        //    (Gán lại từ query string để giữ điều kiện khi chuyển trang)
        // ================================================================

        /// <summary>
        /// Từ khóa tìm kiếm: áp dụng cho Họ tên, SĐT, Email, CCCD
        /// </summary>
        public string? TuKhoa { get; set; }

        /// <summary>
        /// Lọc theo loại khách hàng: "Cá nhân" hoặc "Doanh nghiệp"
        /// </summary>
        public string? LoaiKhachHang { get; set; }

        /// <summary>
        /// Lọc theo trạng thái: "Hoạt động" hoặc "Ngừng hoạt động"
        /// </summary>
        public string? TrangThai { get; set; }

        /// <summary>
        /// Sắp xếp theo tiêu chí:
        ///   "ten_asc"    : Họ tên A → Z
        ///   "ten_desc"   : Họ tên Z → A
        ///   "ngaytao_asc"  : Ngày tạo tăng dần
        ///   "ngaytao_desc" : Ngày tạo giảm dần
        /// </summary>
        public string? SapXep { get; set; }

        // ================================================================
        // 3. THÔNG TIN PHÂN TRANG
        // ================================================================
        public int TrangHienTai { get; set; } = 1;
        public int TongSoTrang { get; set; }
        public int TongSoBanGhi { get; set; }
        public int KichThuocTrang { get; set; } = 10;

        // ================================================================
        // 4. THUỘC TÍNH HỖ TRỢ HIỂN THỊ
        // ================================================================

        /// <summary>
        /// Kiểm tra còn trang trước không (để hiện/ẩn nút "Trang trước").
        /// </summary>
        public bool CoTrangTruoc => TrangHienTai > 1;

        /// <summary>
        /// Kiểm tra còn trang sau không (để hiện/ẩn nút "Trang sau").
        /// </summary>
        public bool CoTrangSau => TrangHienTai < TongSoTrang;

        // ================================================================
        // 5. DANH SÁCH LỰA CHỌN CHO DROPDOWN (dùng ở View)
        // ================================================================

        /// <summary>
        /// Danh sách loại khách hàng để render dropdown.
        /// </summary>
        public List<string> DanhSachLoaiKhachHang { get; set; } = new List<string>
        {
            "Cá nhân",
            "Doanh nghiệp"
        };

        /// <summary>
        /// Danh sách trạng thái để render dropdown.
        /// </summary>
        public List<string> DanhSachTrangThai { get; set; } = new List<string>
        {
            "Hoạt động",
            "Ngừng hoạt động"
        };
    }
}