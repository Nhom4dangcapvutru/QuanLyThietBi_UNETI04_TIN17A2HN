using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories.Interfaces;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tổng số loại thiết bị
        public async Task<int> DemTongSoLoaiThietBiAsync()
        {
            return await _context.LoaiThietBis.CountAsync();
        }

        // Tổng số thiết bị
        public async Task<int> DemTongSoThietBiAsync()
        {
            return await _context.ThietBis.CountAsync();
        }

        // Số thiết bị khả dụng
        public async Task<int> DemSoThietBiKhaDungAsync()
        {
            return await _context.ThietBis
                .CountAsync(x => x.TrangThai == TrangThaiThietBi.HoatDong);
        }

        // Tổng số khách hàng
        public async Task<int> DemTongSoKhachHangAsync()
        {
            return await _context.KhachHangs.CountAsync();
        }

        // Tổng số giao dịch
        public async Task<int> DemTongSoGiaoDichAsync()
        {
            return await _context.PhieuSuaChuas.CountAsync();
        }

        // Giao dịch chờ xử lý
        public async Task<int> DemGiaoDichChoXuLyAsync()
        {
            return await _context.PhieuSuaChuas
                .CountAsync(x => x.TrangThai == TrangThaiPhieu.ChoTiepNhan);
        }

        // Giao dịch đang xử lý
        public async Task<int> DemGiaoDichDangXuLyAsync()
        {
            return await _context.PhieuSuaChuas
                .CountAsync(x => x.TrangThai == TrangThaiPhieu.DangXuLy);
        }

        // Giao dịch hoàn thành
        public async Task<int> DemGiaoDichHoanThanhAsync()
        {
            return await _context.PhieuSuaChuas
                .CountAsync(x => x.TrangThai == TrangThaiPhieu.HoanThanh);
        }

        // Giao dịch đã hủy
        public async Task<int> DemGiaoDichDaHuyAsync()
        {
            return await _context.PhieuSuaChuas
                .CountAsync(x => x.TrangThai == TrangThaiPhieu.DaHuy);
        }

        // Giao dịch bị từ chối
        public async Task<int> DemGiaoDichTuChoiAsync()
        {
            return await _context.PhieuSuaChuas
                .CountAsync(x => x.TrangThai == TrangThaiPhieu.TuChoi);
        }

        // Tổng giá trị phát sinh từ phiếu đã hoàn thành
        public async Task<decimal> TinhTongGiaTriPhatSinhAsync()
        {
            return await _context.PhieuSuaChuas
                .Where(x => x.TrangThai == TrangThaiPhieu.HoanThanh)
                .SumAsync(x => x.TongChiPhi ?? 0m);
        }

        // Lấy phiếu sửa chữa mới nhất
        public async Task<List<PhieuSuaChua>> LayPhieuMoiNhatAsync(int soLuong)
        {
            return await _context.PhieuSuaChuas
                .AsNoTracking()
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .OrderByDescending(p => p.NgayTiepNhan)
                .Take(soLuong)
                .ToListAsync();
        }

        // Lấy phiếu trong khoảng thời gian
        public async Task<List<PhieuSuaChua>> LayPhieuTrongKhoangThangAsync(
            DateTime tuNgay)
        {
            return await _context.PhieuSuaChuas
                .AsNoTracking()
                .Where(p => p.NgayTiepNhan >= tuNgay)
                .ToListAsync();
        }
    }
}