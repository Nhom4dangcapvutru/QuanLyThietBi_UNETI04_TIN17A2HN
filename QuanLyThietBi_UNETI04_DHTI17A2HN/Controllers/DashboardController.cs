

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModel;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            //var vaiTro = HttpContext.Session.GetString("VaiTro");
            //if (string.IsNullOrEmpty(vaiTro) ||
            //    (vaiTro != VaiTroConstant.Admin && vaiTro != VaiTroConstant.KyThuatVien))
            //{
            //    return RedirectToAction("Login", "TaiKhoan");
            //}

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