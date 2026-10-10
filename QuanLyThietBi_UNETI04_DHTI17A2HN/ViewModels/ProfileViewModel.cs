// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: ViewModel cho trang hồ sơ cá nhân
// (Khách hàng xem và cập nhật thông tin của chính mình)

using System.ComponentModel.DataAnnotations;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    /// <summary>
    /// ViewModel cho trang hồ sơ cá nhân của khách hàng.
    /// Dùng cho cả action Index (xem) và Edit (sửa).
    /// </summary>
    public class ProfileViewModel
    {
        // ================================================================
        // 1. THÔNG TIN ĐỊNH DANH (không cho sửa)
        // ================================================================

        /// <summary>
        /// Mã khách hàng - dùng để định danh, KHÔNG cho sửa.
        /// </summary>
        [Display(Name = "Mã khách hàng")]
        public int MaKhachHang { get; set; }

        /// <summary>
        /// Tên đăng nhập (lấy từ bảng TaiKhoan) - KHÔNG cho sửa.
        /// </summary>
        [Display(Name = "Tên đăng nhập")]
        public string? TenDangNhap { get; set; }

        /// <summary>
        /// Ngày tạo tài khoản - KHÔNG cho sửa.
        /// </summary>
        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; }

        // ================================================================
        // 2. THÔNG TIN CÁ NHÂN (cho phép sửa)
        // ================================================================

        /// <summary>
        /// Họ tên khách hàng - KHÔNG cho sửa (chỉ Admin/NV mới sửa được).
        /// </summary>
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại - CHO PHÉP sửa.
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        /// <summary>
        /// Email - CHO PHÉP sửa.
        /// </summary>
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ - CHO PHÉP sửa.
        /// </summary>
        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        /// <summary>
        /// Số CCCD/CMND - CHO PHÉP sửa.
        /// </summary>
        [StringLength(20)]
        [RegularExpression(@"^[0-9]{9,12}$", ErrorMessage = "CCCD phải là 9-12 chữ số")]
        [Display(Name = "Số CCCD/CMND")]
        public string? CCCD { get; set; }

        /// <summary>
        /// Loại khách hàng: Cá nhân / Doanh nghiệp - CHO PHÉP sửa.
        /// </summary>
        [Required]
        [Display(Name = "Loại khách hàng")]
        public string LoaiKhachHang { get; set; } = "Cá nhân";

        /// <summary>
        /// Mã số thuế (chỉ áp dụng cho doanh nghiệp) - CHO PHÉP sửa.
        /// </summary>
        [StringLength(20)]
        [Display(Name = "Mã số thuế")]
        public string? MaSoThue { get; set; }

        // ================================================================
        // 3. TRẠNG THÁI (chỉ xem, không sửa)
        // ================================================================

        /// <summary>
        /// Trạng thái khách hàng (Hoạt động / Ngừng hoạt động).
        /// Chỉ Admin/NV mới đổi được. Khách hàng chỉ xem.
        /// </summary>
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Hoạt động";

        /// <summary>
        /// Ghi chú nội bộ - chỉ Admin/NV thấy, KHÔNG hiển thị cho khách hàng.
        /// </summary>
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // ================================================================
        // 4. THÔNG TIN BỔ SUNG (hiển thị trên trang Index)
        // ================================================================

        /// <summary>
        /// Tổng số thiết bị của khách hàng này.
        /// </summary>
        [Display(Name = "Tổng số thiết bị")]
        public int TongSoThietBi { get; set; }

        /// <summary>
        /// Tổng số phiếu sửa chữa đã tạo.
        /// </summary>
        [Display(Name = "Tổng số phiếu sửa chữa")]
        public int TongSoPhieu { get; set; }

        /// <summary>
        /// Số phiếu đang xử lý (Chờ tiếp nhận + Đang xử lý).
        /// </summary>
        [Display(Name = "Phiếu đang xử lý")]
        public int SoPhieuDangXuLy { get; set; }
    }
}