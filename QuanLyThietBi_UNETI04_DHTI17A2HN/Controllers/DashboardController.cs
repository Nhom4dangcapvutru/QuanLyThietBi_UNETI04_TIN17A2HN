// Họ và tên: Cung Đình Phước
// Mã sinh viên: 23103100311

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModel;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục.";
                context.Result = RedirectToAction("Login", "Account", new { returnUrl = returnUrl.ToString() });
                return;
            }

            if (vaiTro != VaiTroConstant.Admin && vaiTro != VaiTroConstant.KyThuatVien)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập Dashboard. Chỉ Admin và Kỹ thuật viên mới có quyền xem.";
                context.Result = RedirectToAction("AccessDenied", "Account");
                return;
            }

            base.OnActionExecuting(context);
        }

        // GET: /Dashboard
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
                // Lấy 8 phiếu sửa chữa mới nhất để hiển thị tại bảng điều hành trung tâm
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
    }
}
