
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories
{
    public class ThongKeRepository : IThongKeRepository
    {
        private readonly ApplicationDbContext _context;

        public ThongKeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ThongKeViewModel> GetThongKeAsync()
        {
            var model = new ThongKeViewModel();

            // 1. Thống kê thiết bị theo loại
            model.SoThietBiTheoLoai = await _context.ThietBis
                .Include(t => t.LoaiThietBi)
                .GroupBy(t => new
                {
                    t.MaLoai,
                    TenLoai = t.LoaiThietBi!.TenLoai,
                    HangSanXuat = t.LoaiThietBi.HangSanXuat
                })
                .Select(g => new ThongKeTheoLoaiItem
                {
                    TenLoai = g.Key.TenLoai,
                    HangSanXuat = g.Key.HangSanXuat,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            // 2. Thống kê phiếu theo trạng thái
            model.SoPhieuTheoTrangThai = await _context.PhieuSuaChuas
                .GroupBy(p => p.TrangThai)
                .Select(g => new ThongKeTheoTrangThaiItem
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

            // 3. Thống kê phiếu theo tháng
            model.SoPhieuTheoThang = await _context.PhieuSuaChuas
                .GroupBy(p => new
                {
                    p.NgayTiepNhan.Year,
                    p.NgayTiepNhan.Month
                })
                .Select(g => new ThongKeTheoThangItem
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.Nam)
                .ThenByDescending(x => x.Thang)
                .Take(12)
                .ToListAsync();

            // 4. Top 5 kỹ thuật viên
            model.TopKyThuatVien = await _context.ChiTietSuaChuas
                .Include(c => c.KyThuatVien)
                .GroupBy(c => new
                {
                    c.MaKyThuatVien,
                    HoTen = c.KyThuatVien!.HoTen
                })
                .Select(g => new ThongKeKyThuatVienItem
                {
                    MaKyThuatVien = g.Key.MaKyThuatVien,
                    HoTen = g.Key.HoTen,
                    SoPhieuXuLy = g.Select(x => x.MaPhieu)
                                    .Distinct().Count(),
                    SoChiTiet = g.Count()
                })
                .OrderByDescending(x => x.SoPhieuXuLy)
                .ThenByDescending(x => x.SoChiTiet)
                .Take(5)
                .ToListAsync();

            // 5. Thời gian sửa chữa trung bình
            var thoiGianList = await _context.PhieuSuaChuas
                .Where(p => p.TrangThai == TrangThaiPhieu.HoanThanh
                    && p.NgayHoanThanh != null)
                .Select(p => EF.Functions.DateDiffDay(
                    p.NgayTiepNhan,
                    p.NgayHoanThanh!.Value))
                .ToListAsync();

            model.ThoiGianSuaChuaTrungBinh = thoiGianList.Any()
                ? thoiGianList.Average()
                : 0;

            // 6. Tổng chi phí linh kiện
            model.TongChiPhiLinhKien = await _context.ChiTietSuaChuas
                .Select(x => (decimal?)x.ThanhTien)
                .SumAsync() ?? 0m;

            // 7. Doanh thu sửa chữa
            model.DoanhThuSuaChua = await _context.PhieuSuaChuas
                .Where(x => x.TrangThai == TrangThaiPhieu.HoanThanh
                    && x.TongChiPhi != null)
                .SumAsync(x => x.TongChiPhi ?? 0m);

            // 8. Thống kê phiếu trong và ngoài bảo hành
            var phieuVoiThietBi = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Where(p => p.ThietBi != null)
                .Select(p => new
                {
                    TrongBaoHanh =
                        p.NgayTiepNhan.Date <= p.ThietBi!.HanBaoHanh.Date
                })
                .ToListAsync();

            model.SoPhieuTrongBaoHanh =
                phieuVoiThietBi.Count(x => x.TrongBaoHanh);

            model.SoPhieuNgoaiBaoHanh =
                phieuVoiThietBi.Count(x => !x.TrongBaoHanh);

            int tong = phieuVoiThietBi.Count;

            model.TyLeSuaChuaTrongBaoHanh = tong > 0
                ? Math.Round(
                    (double)model.SoPhieuTrongBaoHanh / tong * 100, 2)
                : 0;

            return model;
        }
    }
}
