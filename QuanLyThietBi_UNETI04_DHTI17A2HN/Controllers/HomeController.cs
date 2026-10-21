using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModel;
using System.Diagnostics;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TongSoLoaiThietBi = await _context.LoaiThietBis.CountAsync(),
                TongSoThietBi = await _context.ThietBis.CountAsync(),
                SoThietBiKhaDung = await _context.ThietBis
                    .CountAsync(x => x.TrangThai == TrangThaiThietBi.HoatDong),
                TongSoKhachHang = await _context.KhachHangs.CountAsync(),
                TongSoGiaoDich = await _context.PhieuSuaChuas.CountAsync(),
                SoGiaoDichChoXuLy = await _context.PhieuSuaChuas
                    .CountAsync(x => x.TrangThai == TrangThaiPhieu.ChoTiepNhan),
                SoGiaoDichDangXuLy = await _context.PhieuSuaChuas
                    .CountAsync(x => x.TrangThai == TrangThaiPhieu.DangXuLy),
                SoGiaoDichHoanThanh = await _context.PhieuSuaChuas
                    .CountAsync(x => x.TrangThai == TrangThaiPhieu.HoanThanh),
                SoGiaoDichDaHuy = await _context.PhieuSuaChuas
                    .CountAsync(x => x.TrangThai == TrangThaiPhieu.DaHuy || x.TrangThai == TrangThaiPhieu.TuChoi),
                TongGiaTriPhatSinh = await _context.PhieuSuaChuas
                    .Where(x => x.TrangThai == TrangThaiPhieu.HoanThanh && x.TongChiPhi != null)
                    .SumAsync(x => x.TongChiPhi ?? 0m),
                PhieuMoiNhat = await _context.PhieuSuaChuas
                    .Include(p => p.ThietBi)
                    .Include(p => p.KhachHang)
                    .OrderByDescending(p => p.NgayTiepNhan)
                    .Take(8)
                    .ToListAsync()
            };

            // Dữ liệu biểu đồ 6 tháng gần nhất
            var sixMonthsAgo = DateTime.Now.AddMonths(-5);
            var phieu6Thang = await _context.PhieuSuaChuas
                .Where(p => p.NgayTiepNhan >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
                .ToListAsync();

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = DateTime.Now.AddMonths(-i);
                var label = $"T{targetMonth.Month}/{targetMonth.Year}";
                model.ThangLabels.Add(label);

                var phieuTrongThang = phieu6Thang
                    .Where(p => p.NgayTiepNhan.Year == targetMonth.Year && p.NgayTiepNhan.Month == targetMonth.Month)
                    .ToList();

                model.SoPhieuTheoThangData.Add(phieuTrongThang.Count);
                model.DoanhThuTheoThangData.Add(phieuTrongThang
                    .Where(p => p.TrangThai == TrangThaiPhieu.HoanThanh)
                    .Sum(p => p.TongChiPhi ?? 0m));
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
