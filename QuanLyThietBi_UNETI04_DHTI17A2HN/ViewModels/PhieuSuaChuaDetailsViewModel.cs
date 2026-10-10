// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: ViewModel cho trang chi tiết phiếu sửa chữa
// (Hiển thị thông tin phiếu + khách hàng + thiết bị + chi tiết sửa chữa)

using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    /// <summary>
    /// ViewModel cho trang chi tiết phiếu sửa chữa.
    /// Kết hợp nhiều nguồn dữ liệu: PhieuSuaChua + KhachHang + ThietBi + ChiTietSuaChua.
    /// </summary>
    public class PhieuSuaChuaDetailsViewModel
    {
        // ================================================================
        // 1. THÔNG TIN PHIẾU SỬA CHỮA
        // ================================================================
        public PhieuSuaChua PhieuSuaChua { get; set; } = null!;

        // ================================================================
        // 2. THÔNG TIN KHÁCH HÀNG (lấy từ phiếu)
        // ================================================================
        public KhachHang? KhachHang { get; set; }

        // ================================================================
        // 3. THÔNG TIN THIẾT BỊ (lấy từ phiếu)
        // ================================================================
        public ThietBi? ThietBi { get; set; }

        // ================================================================
        // 4. DANH SÁCH CHI TIẾT SỬA CHỮA
        //    (linh kiện đã thay, kỹ thuật viên thực hiện, chi phí từng dòng)
        // ================================================================
        public List<ChiTietSuaChua> ChiTietSuaChuas { get; set; } = new List<ChiTietSuaChua>();

        // ================================================================
        // 5. CÁC THUỘC TÍNH TÍNH TOÁN
        // ================================================================

        /// <summary>
        /// Tổng tiền sửa chữa = tổng ThanhTien của tất cả ChiTietSuaChua.
        /// Nếu chưa có chi tiết → trả về 0.
        /// </summary>
        public decimal TongTien => ChiTietSuaChuas?.Sum(c => c.ThanhTien) ?? 0;

        /// <summary>
        /// Thiết bị có còn trong thời hạn bảo hành không.
        /// </summary>
        public bool ConBaoHanh => ThietBi != null
                                  && DateTime.Now.Date <= ThietBi.HanBaoHanh.Date;

        /// <summary>
        /// Phiếu có được phép hủy không.
        /// Chỉ hủy khi đang ở trạng thái "Chờ tiếp nhận".
        /// </summary>
        public bool CoTheHuy => PhieuSuaChua != null
                                && PhieuSuaChua.TrangThai == "Chờ tiếp nhận";

        // ================================================================
        // 6. THÔNG TIN NGƯỜI ĐANG ĐĂNG NHẬP (để phân quyền hiển thị)
        // ================================================================

        /// <summary>
        /// Mã tài khoản người đang đăng nhập (đọc từ Session).
        /// </summary>
        public int? NguoiDangNhap_MaTaiKhoan { get; set; }

        /// <summary>
        /// Vai trò người đang đăng nhập (Admin / KyThuatVien / KhachHang).
        /// </summary>
        public string? NguoiDangNhap_VaiTro { get; set; }

        /// <summary>
        /// Người đang đăng nhập có phải là nhân viên/Admin không.
        /// Nhân viên/Admin xem được mọi phiếu; khách hàng chỉ xem phiếu của mình.
        /// </summary>
        public bool LaNhanVien => NguoiDangNhap_VaiTro == "Admin"
                                  || NguoiDangNhap_VaiTro == "KyThuatVien";
    }
}