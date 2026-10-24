using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    public class DashboardViewModel
    {
        // KPI Tổng quan
        public int TongSoLoaiThietBi { get; set; }
        public int TongSoThietBi { get; set; }
        public int SoThietBiKhaDung { get; set; }
        public int TongSoKhachHang { get; set; }
        public int TongSoGiaoDich { get; set; }
        public int SoGiaoDichChoXuLy { get; set; }
        public int SoGiaoDichDangXuLy { get; set; }
        public int SoGiaoDichHoanThanh { get; set; }
        public int SoGiaoDichDaHuy { get; set; }
        public int SoGiaoDichTuChoi { get; set; }
        public decimal TongGiaTriPhatSinh { get; set; }

        // Danh sách phiếu sửa chữa mới nhất (Trung tâm điều hành)
        public List<PhieuSuaChua> PhieuMoiNhat { get; set; } = new();

        // Dữ liệu cho biểu đồ Chart.js
        public List<string> ThangLabels { get; set; } = new();
        public List<int> SoPhieuTheoThangData { get; set; } = new();
        public List<decimal> DoanhThuTheoThangData { get; set; } = new();
    }
}
