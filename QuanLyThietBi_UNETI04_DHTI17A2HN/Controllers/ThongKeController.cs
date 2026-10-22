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
    public class ThongKeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
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
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang Thống kê. Chỉ Admin và Kỹ thuật viên mới có quyền xem.";
                context.Result = RedirectToAction("AccessDenied", "Account");
                return;
            }

            base.OnActionExecuting(context);
        }

        // GET: /ThongKe
        public async Task<IActionResult> Index()
        {
            var model = new ThongKeViewModel();

       
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

      
            model.SoPhieuTheoTrangThai = await _context.PhieuSuaChuas
                .GroupBy(p => p.TrangThai)
                .Select(g => new ThongKeTheoTrangThaiItem
                {
                    TrangThai = g.Key,
                    SoLuong = g.Count()
                })
                .OrderByDescending(x => x.SoLuong)
                .ToListAsync();

      
            model.SoPhieuTheoThang = await _context.PhieuSuaChuas
                .GroupBy(p => new { p.NgayTiepNhan.Year, p.NgayTiepNhan.Month })
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
                    SoPhieuXuLy = g.Select(x => x.MaPhieu).Distinct().Count(),
                    SoChiTiet = g.Count()
                })
                .OrderByDescending(x => x.SoPhieuXuLy)
                .ThenByDescending(x => x.SoChiTiet)
                .Take(5)
                .ToListAsync();

            
            var thoiGianList = await _context.PhieuSuaChuas
                .Where(p => p.TrangThai == TrangThaiPhieu.HoanThanh && p.NgayHoanThanh != null)
                .Select(p => EF.Functions.DateDiffDay(p.NgayTiepNhan, p.NgayHoanThanh!.Value))
                .ToListAsync();

            model.ThoiGianSuaChuaTrungBinh = thoiGianList.Any()
                ? thoiGianList.Average()
                : 0;

            model.TongChiPhiLinhKien = await _context.ChiTietSuaChuas
                .Select(x => (decimal?)x.ThanhTien)
                .SumAsync() ?? 0m;

            model.DoanhThuSuaChua = await _context.PhieuSuaChuas
                .Where(x => x.TrangThai == TrangThaiPhieu.HoanThanh && x.TongChiPhi != null)
                .SumAsync(x => x.TongChiPhi ?? 0m);

        
            var phieuVoiThietBi = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Where(p => p.ThietBi != null)
                .Select(p => new
                {
                    TrongBaoHanh = p.NgayTiepNhan.Date <= p.ThietBi!.HanBaoHanh.Date
                })
                .ToListAsync();

            model.SoPhieuTrongBaoHanh = phieuVoiThietBi.Count(x => x.TrongBaoHanh);
            model.SoPhieuNgoaiBaoHanh = phieuVoiThietBi.Count(x => !x.TrongBaoHanh);
            int tong = phieuVoiThietBi.Count;
            model.TyLeSuaChuaTrongBaoHanh = tong > 0
                ? Math.Round((double)model.SoPhieuTrongBaoHanh / tong * 100, 2)
                : 0;

            return View(model);
        }
    }
}