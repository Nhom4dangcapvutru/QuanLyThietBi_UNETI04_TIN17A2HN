// Họ và tên: Cả nhóm thống nhất thực hiện
// Mã sinh viên: Dùng chung 5 thành viên
// Nội dung thực hiện: Định nghĩa hằng số hệ thống (Vai trò, Trạng thái)

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants
{
    public static class VaiTroConstant
    {
        public const string Admin = "Admin";
        public const string KyThuatVien = "KyThuatVien";
        public const string KhachHang = "KhachHang";
    }

    public static class TrangThaiTaiKhoan
    {
        public const string HoatDong = "Hoạt động";
        public const string BiKhoa = "Bị khóa";
    }

    public static class TrangThaiThietBi
    {
        public const string HoatDong = "Hoạt động bình thường";
        public const string DangBaoHanh = "Đang bảo hành";
        public const string DangSuaChua = "Đang sửa chữa";
        public const string Hong = "Hỏng / Không hoạt động";
    }

    public static class TrangThaiPhieu
    {
        public const string ChoTiepNhan = "Chờ tiếp nhận";
        public const string DangXuLy = "Đang xử lý";
        public const string HoanThanh = "Hoàn thành";
        public const string DaHuy = "Đã hủy";
        public const string TuChoi = "Từ chối tiếp nhận";
    }

    public static class MucDoUuTien
    {
        public const string Thap = "Thấp";
        public const string TrungBinh = "Trung bình";
        public const string Cao = "Cao";
        public const string KhanCap = "Khẩn cấp";
    }

    public static class LoaiKhachHangConstant
    {
        public const string CaNhan = "Cá nhân";
        public const string DoanhNghiep = "Doanh nghiệp";
    }
}