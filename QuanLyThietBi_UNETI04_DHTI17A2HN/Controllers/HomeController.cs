using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModel;
using System.Diagnostics;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập (được đảm bảo bởi BaseController, nhưng kiểm tra thêm cho an toàn)
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new DashboardViewModel();
            var isKhachHang = SessionHelper.IsKhachHang(HttpContext.Session);

            if (isKhachHang)
            {
                // ==============================================================
                // PHÂN HỆ KHÁCH HÀNG: CHỈ LẤY DỮ LIỆU CỦA CHÍNH KHÁCH HÀNG NÀY
                // TUYỆT ĐỐI KHÔNG LỘ THÔNG TIN / PHIẾU CỦA KHÁCH HÀNG KHÁC
                // ==============================================================
                var maTaiKhoan = SessionHelper.GetMaTaiKhoan(HttpContext.Session);
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);

                if (khachHang != null)
                {
                    int maKh = khachHang.MaKhachHang;

                    // Thiết bị của riêng khách hàng này
                    model.TongSoThietBi = await _context.ThietBis
                        .CountAsync(x => x.MaKhachHang == maKh);
                    model.SoThietBiKhaDung = await _context.ThietBis
                        .CountAsync(x => x.MaKhachHang == maKh && x.TrangThai == TrangThaiThietBi.HoatDong);
                    model.TongSoLoaiThietBi = await _context.ThietBis
                        .Where(x => x.MaKhachHang == maKh)
                        .Select(x => x.MaLoai)
                        .Distinct()
                        .CountAsync();
                    model.TongSoKhachHang = 1;

                    // Phiếu sửa chữa của riêng khách hàng này
                    var queryPhieu = _context.PhieuSuaChuas.Where(x => x.MaKhachHang == maKh);

                    model.TongSoGiaoDich = await queryPhieu.CountAsync();
                    model.SoGiaoDichChoXuLy = await queryPhieu
                        .CountAsync(x => x.TrangThai == TrangThaiPhieu.ChoTiepNhan);
                    model.SoGiaoDichDangXuLy = await queryPhieu
                        .CountAsync(x => x.TrangThai == TrangThaiPhieu.DangXuLy);
                    model.SoGiaoDichHoanThanh = await queryPhieu
                        .CountAsync(x => x.TrangThai == TrangThaiPhieu.HoanThanh);
                    model.SoGiaoDichDaHuy = await queryPhieu
                        .CountAsync(x => x.TrangThai == TrangThaiPhieu.DaHuy || x.TrangThai == TrangThaiPhieu.TuChoi);
                    
                    // Tổng chi phí sửa chữa khách hàng này đã chi trả
                    model.TongGiaTriPhatSinh = await queryPhieu
                        .Where(x => x.TrangThai == TrangThaiPhieu.HoanThanh && x.TongChiPhi != null)
                        .SumAsync(x => x.TongChiPhi ?? 0m);

                    // CHỈ LẤY PHIẾU CỦA RIÊNG KHÁCH HÀNG NÀY
                    model.PhieuMoiNhat = await queryPhieu
                        .Include(p => p.ThietBi)
                        .Include(p => p.KhachHang)
                        .OrderByDescending(p => p.NgayTiepNhan)
                        .Take(8)
                        .ToListAsync();

                    // Biểu đồ 6 tháng của riêng khách hàng này
                    var sixMonthsAgo = DateTime.Now.AddMonths(-5);
                    var phieu6Thang = await queryPhieu
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
                }
            }
            else
            {
                // ==============================================================
                // PHÂN HỆ QUẢN TRỊ VIÊN / KỸ THUẬT VIÊN: XEM TOÀN BỘ TRUNG TÂM
                // ==============================================================
                model.TongSoLoaiThietBi = await _context.LoaiThietBis.CountAsync();
                model.TongSoThietBi = await _context.ThietBis.CountAsync();
                model.SoThietBiKhaDung = await _context.ThietBis
                    .CountAsync(x => x.TrangThai == TrangThaiThietBi.HoatDong);
                model.TongSoKhachHang = await _context.KhachHangs.CountAsync();
                model.TongSoGiaoDich = await _context.PhieuSuaChuas.CountAsync();
                model.SoGiaoDichChoXuLy = await _context.PhieuSuaChuas
                    .CountAsync(x => x.TrangThai == TrangThaiPhieu.ChoTiepNhan);
                model.SoGiaoDichDangXuLy = await _context.PhieuSuaChuas
                    .CountAsync(x => x.TrangThai == TrangThaiPhieu.DangXuLy);
                model.SoGiaoDichHoanThanh = await _context.PhieuSuaChuas
                    .CountAsync(x => x.TrangThai == TrangThaiPhieu.HoanThanh);
                model.SoGiaoDichDaHuy = await _context.PhieuSuaChuas
                    .CountAsync(x => x.TrangThai == TrangThaiPhieu.DaHuy || x.TrangThai == TrangThaiPhieu.TuChoi);
                
                // Doanh thu toàn bộ trung tâm
                model.TongGiaTriPhatSinh = await _context.PhieuSuaChuas
                    .Where(x => x.TrangThai == TrangThaiPhieu.HoanThanh && x.TongChiPhi != null)
                    .SumAsync(x => x.TongChiPhi ?? 0m);

                // 8 phiếu mới nhất toàn hệ thống
                model.PhieuMoiNhat = await _context.PhieuSuaChuas
                    .Include(p => p.ThietBi)
                    .Include(p => p.KhachHang)
                    .OrderByDescending(p => p.NgayTiepNhan)
                    .Take(8)
                    .ToListAsync();

                // Dữ liệu biểu đồ 6 tháng gần nhất toàn trung tâm
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
