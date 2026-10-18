

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

            
                TongGiaTriPhatSinh = await _context.PhieuSuaChuas
                    .Where(x => x.TrangThai == TrangThaiPhieu.HoanThanh && x.TongChiPhi != null)
                    .SumAsync(x => x.TongChiPhi ?? 0m)
            };

            return View(model);
        }
    }
}