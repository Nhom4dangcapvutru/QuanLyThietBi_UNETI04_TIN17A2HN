

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels
{
    public class ThongKeViewModel
    {
     
        public List<ThongKeTheoLoaiItem> SoThietBiTheoLoai { get; set; } = new();

      
        public List<ThongKeTheoTrangThaiItem> SoPhieuTheoTrangThai { get; set; } = new();

       
        public List<ThongKeTheoThangItem> SoPhieuTheoThang { get; set; } = new();

       
        public List<ThongKeKyThuatVienItem> TopKyThuatVien { get; set; } = new();

        
        public double ThoiGianSuaChuaTrungBinh { get; set; }

 
        public decimal TongChiPhiLinhKien { get; set; }


        public decimal DoanhThuSuaChua { get; set; }

    
        public double TyLeSuaChuaTrongBaoHanh { get; set; }
        public int SoPhieuTrongBaoHanh { get; set; }
        public int SoPhieuNgoaiBaoHanh { get; set; }
    }

    public class ThongKeTheoLoaiItem
    {
        public string TenLoai { get; set; } = string.Empty;
        public string HangSanXuat { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }

    public class ThongKeTheoTrangThaiItem
    {
        public string TrangThai { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }

    public class ThongKeTheoThangItem
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public string Label => $"{Thang:00}/{Nam}";
        public int SoLuong { get; set; }
    }

    public class ThongKeKyThuatVienItem
    {
        public int MaKyThuatVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public int SoPhieuXuLy { get; set; }
        public int SoChiTiet { get; set; }
    }
}