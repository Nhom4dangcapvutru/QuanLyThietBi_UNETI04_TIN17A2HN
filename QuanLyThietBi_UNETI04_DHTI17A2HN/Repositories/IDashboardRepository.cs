using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> DemTongSoLoaiThietBiAsync();

        Task<int> DemTongSoThietBiAsync();

        Task<int> DemSoThietBiKhaDungAsync();

        Task<int> DemTongSoKhachHangAsync();

        Task<int> DemTongSoGiaoDichAsync();

        Task<int> DemGiaoDichChoXuLyAsync();

        Task<int> DemGiaoDichDangXuLyAsync();

        Task<int> DemGiaoDichHoanThanhAsync();

        Task<int> DemGiaoDichDaHuyAsync();

        Task<int> DemGiaoDichTuChoiAsync();

        Task<decimal> TinhTongGiaTriPhatSinhAsync();

        Task<List<PhieuSuaChua>> LayPhieuMoiNhatAsync(int soLuong);

        Task<List<PhieuSuaChua>> LayPhieuTrongKhoangThangAsync(
            DateTime tuNgay);
    }
}