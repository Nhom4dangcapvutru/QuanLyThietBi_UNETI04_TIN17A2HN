// Họ và tên: Cung Đình Phước
// Mã sinh viên: 23103100311

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories.Interfaces;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(
            IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(vaiTro))
            {
                var returnUrl = context.HttpContext.Request.Path
                    + context.HttpContext.Request.QueryString;

                TempData["ErrorMessage"] =
                    "Vui lòng đăng nhập để tiếp tục.";

                context.Result = RedirectToAction(
                    "Login",
                    "Account",
                    new { returnUrl = returnUrl.ToString() });

                return;
            }

            if (vaiTro != VaiTroConstant.Admin &&
                vaiTro != VaiTroConstant.KyThuatVien)
            {
                TempData["ErrorMessage"] =
                    "Bạn không có quyền truy cập Dashboard. " +
                    "Chỉ Admin và Kỹ thuật viên mới có quyền xem.";

                context.Result = RedirectToAction(
                    "AccessDenied",
                    "Account");

                return;
            }

            base.OnActionExecuting(context);
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TongSoLoaiThietBi =
                    await _dashboardRepository.DemTongSoLoaiThietBiAsync(),

                TongSoThietBi =
                    await _dashboardRepository.DemTongSoThietBiAsync(),

                SoThietBiKhaDung =
                    await _dashboardRepository.DemSoThietBiKhaDungAsync(),

                TongSoKhachHang =
                    await _dashboardRepository.DemTongSoKhachHangAsync(),

                TongSoGiaoDich =
                    await _dashboardRepository.DemTongSoGiaoDichAsync(),

                SoGiaoDichChoXuLy =
                    await _dashboardRepository.DemGiaoDichChoXuLyAsync(),

                SoGiaoDichDangXuLy =
                    await _dashboardRepository.DemGiaoDichDangXuLyAsync(),

                SoGiaoDichHoanThanh =
                    await _dashboardRepository.DemGiaoDichHoanThanhAsync(),

                SoGiaoDichDaHuy =
                    await _dashboardRepository.DemGiaoDichDaHuyAsync(),

                SoGiaoDichTuChoi =
                    await _dashboardRepository.DemGiaoDichTuChoiAsync(),

                TongGiaTriPhatSinh =
                    await _dashboardRepository.TinhTongGiaTriPhatSinhAsync(),

                PhieuMoiNhat =
                    await _dashboardRepository.LayPhieuMoiNhatAsync(8)
            };

            // Dữ liệu biểu đồ 6 tháng gần nhất
            var sixMonthsAgo = DateTime.Now.AddMonths(-5);

            var ngayBatDau = new DateTime(
                sixMonthsAgo.Year,
                sixMonthsAgo.Month,
                1);

            var phieu6Thang =
                await _dashboardRepository.LayPhieuTrongKhoangThangAsync(
                    ngayBatDau);

            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = DateTime.Now.AddMonths(-i);

                var label =
                    $"T{targetMonth.Month}/{targetMonth.Year}";

                model.ThangLabels.Add(label);

                var phieuTrongThang = phieu6Thang
                    .Where(p =>
                        p.NgayTiepNhan.Year == targetMonth.Year &&
                        p.NgayTiepNhan.Month == targetMonth.Month)
                    .ToList();

                model.SoPhieuTheoThangData.Add(
                    phieuTrongThang.Count);

                model.DoanhThuTheoThangData.Add(
                    phieuTrongThang
                        .Where(p =>
                            p.TrangThai == TrangThaiPhieu.HoanThanh)
                        .Sum(p => p.TongChiPhi ?? 0m));
            }

            return View(model);
        }
    }
}