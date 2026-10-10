// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: ViewModel cho form tạo phiếu sửa chữa
// (Chứa dữ liệu form + dropdown thiết bị của khách hàng đang đăng nhập)

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    /// <summary>
    /// ViewModel cho form tạo phiếu sửa chữa.
    /// Khách hàng đăng nhập → chọn 1 thiết bị của mình → mô tả lỗi → gửi.
    /// </summary>
    public class PhieuSuaChuaCreateViewModel
    {
        // ================================================================
        // 1. DỮ LIỆU NGƯỜI DÙNG NHẬP
        // ================================================================

        /// <summary>
        /// Thiết bị được chọn để sửa chữa.
        /// Bắt buộc. Dropdown chỉ hiện thiết bị thuộc khách hàng đang đăng nhập.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thiết bị cần sửa chữa")]
        [Display(Name = "Thiết bị cần sửa")]
        public int MaThietBi { get; set; }

        /// <summary>
        /// Mô tả chi tiết lỗi mà khách hàng gặp phải.
        /// Bắt buộc, tối đa 1000 ký tự (theo entity PhieuSuaChua).
        /// </summary>
        [Required(ErrorMessage = "Vui lòng mô tả nội dung lỗi")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Nội dung lỗi từ 10 đến 1000 ký tự")]
        [Display(Name = "Nội dung lỗi")]
        public string NoiDungLoi { get; set; } = string.Empty;

        /// <summary>
        /// Mức độ ưu tiên: Thấp / Trung bình / Cao / Khẩn cấp.
        /// Mặc định "Trung bình".
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn mức độ ưu tiên")]
        [Display(Name = "Mức độ ưu tiên")]
        public string MucDo { get; set; } = "Trung bình";

        // ================================================================
        // 2. DỮ LIỆU HỖ TRỢ HIỂN THỊ (không lưu vào DB)
        // ================================================================

        /// <summary>
        /// Dropdown thiết bị: chỉ chứa thiết bị của khách hàng đang đăng nhập.
        /// Mỗi item: Value = MaThietBi, Text = "Tên thiết bị - SerialNumber".
        /// </summary>
        public List<SelectListItem> DanhSachThietBi { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Dropdown mức độ ưu tiên (theo MucDoUuTien trong SystemConstants).
        /// </summary>
        public List<SelectListItem> DanhSachMucDo { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Tên khách hàng đang đăng nhập (hiển thị "Xin chào ..." trên form).
        /// </summary>
        public string? TenKhachHang { get; set; }

        /// <summary>
        /// Mã khách hàng đang đăng nhập (để kiểm tra nghiệp vụ phía Controller).
        /// </summary>
        public int MaKhachHang { get; set; }

        // ================================================================
        // 3. THÔNG TIN THIẾT BỊ ĐƯỢC CHỌN (hiển thị preview khi chọn)
        // ================================================================

        /// <summary>
        /// Tên thiết bị đang được chọn (dùng để hiển thị preview).
        /// </summary>
        public string? TenThietBiDuocChon { get; set; }

        /// <summary>
        /// Hạn bảo hành của thiết bị đang được chọn.
        /// Hiển thị để khách hàng biết còn bảo hành hay không.
        /// </summary>
        public DateTime? HanBaoHanhThietBi { get; set; }

        /// <summary>
        /// Thiết bị còn bảo hành hay không (tính từ HanBaoHanh).
        /// </summary>
        public bool ConBaoHanh => HanBaoHanhThietBi.HasValue
                                  && DateTime.Now.Date <= HanBaoHanhThietBi.Value.Date;
    }
}