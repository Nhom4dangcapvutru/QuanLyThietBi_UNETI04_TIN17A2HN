// Họ và tên: [DINH THI THUY LINH]
// Mã sinh viên: [23103100110]
// Nội dung thực hiện: Module 4 - Service kiểm tra luồng trạng thái nghiệp vụ
// Tiếp nhận -> Chẩn đoán -> Sửa chữa -> Hoàn thành / Hủy / Từ chối

using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Services
{
    public interface IQuyTrinhSuaChuaService
    {
        bool KiemTraChuyenTrangThai(string trangThaiHienTai, string trangThaiMoi);
        List<string> LayTrangThaiChoPhep(string trangThaiHienTai);
        bool CoTheTiepNhan(string trangThai);
        bool CoTheChanDoan(string trangThai);
        bool CoTheSuaChua(string trangThai);
        bool CoTheHoanThanh(string trangThai);
        bool CoTheHuy(string trangThai);
        bool LaTrangThaiKetThuc(string trangThai);
    }

    public class QuyTrinhSuaChuaService : IQuyTrinhSuaChuaService
    {
        /// <summary>
        /// Luồng nghiệp vụ:
        /// Chờ tiếp nhận --(Tiếp nhận)--> Đang xử lý --(Hoàn thành)--> Hoàn thành
        ///                                |
        ///                                \--(Hủy)--> Đã hủy
        /// Chờ tiếp nhận --(Từ chối)--> Từ chối tiếp nhận
        /// </summary>
        private static readonly Dictionary<string, List<string>> Luong = new()
        {
            [TrangThaiPhieu.ChoTiepNhan] = new()
            {
                TrangThaiPhieu.DangXuLy,
                TrangThaiPhieu.TuChoi
            },
            [TrangThaiPhieu.DangXuLy] = new()
            {
                TrangThaiPhieu.HoanThanh,
                TrangThaiPhieu.DaHuy
            },
            [TrangThaiPhieu.HoanThanh] = new(),   // Kết thúc
            [TrangThaiPhieu.DaHuy] = new(),        // Kết thúc
            [TrangThaiPhieu.TuChoi] = new()        // Kết thúc
        };

        public bool KiemTraChuyenTrangThai(string hienTai, string moi)
        {
            if (string.IsNullOrWhiteSpace(hienTai) || string.IsNullOrWhiteSpace(moi))
                return false;
            if (hienTai == moi) return false;
            return Luong.TryGetValue(hienTai, out var ds) && ds.Contains(moi);
        }

        public List<string> LayTrangThaiChoPhep(string hienTai)
        {
            return Luong.TryGetValue(hienTai, out var ds) ? ds : new List<string>();
        }

        public bool CoTheTiepNhan(string tt) => tt == TrangThaiPhieu.ChoTiepNhan;
        public bool CoTheChanDoan(string tt) => tt == TrangThaiPhieu.DangXuLy;
        public bool CoTheSuaChua(string tt) => tt == TrangThaiPhieu.DangXuLy;
        public bool CoTheHoanThanh(string tt) => tt == TrangThaiPhieu.DangXuLy;

        public bool CoTheHuy(string tt)
            => tt == TrangThaiPhieu.ChoTiepNhan || tt == TrangThaiPhieu.DangXuLy;

        public bool LaTrangThaiKetThuc(string tt)
            => tt == TrangThaiPhieu.HoanThanh
            || tt == TrangThaiPhieu.DaHuy
            || tt == TrangThaiPhieu.TuChoi;
    }
}
