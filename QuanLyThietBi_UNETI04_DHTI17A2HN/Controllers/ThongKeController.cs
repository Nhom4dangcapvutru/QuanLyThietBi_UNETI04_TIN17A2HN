
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class ThongKeController : BaseController
    {
        private readonly IThongKeRepository _thongKeRepository;

        public ThongKeController(
            IThongKeRepository thongKeRepository)
        {
            _thongKeRepository = thongKeRepository;
        }

        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session
                .GetString("VaiTro");

            if (string.IsNullOrEmpty(vaiTro))
            {
                var returnUrl = context.HttpContext.Request.Path
                    + context.HttpContext.Request.QueryString;

                TempData["ErrorMessage"] =
                    "Vui lòng đăng nhập để tiếp tục.";

                context.Result = RedirectToAction(
                    "Login", "Account",
                    new { returnUrl = returnUrl.ToString() });

                return;
            }

            if (vaiTro != VaiTroConstant.Admin
                && vaiTro != VaiTroConstant.KyThuatVien)
            {
                TempData["ErrorMessage"] =
                    "Bạn không có quyền truy cập trang Thống kê.";

                context.Result = RedirectToAction(
                    "AccessDenied", "Account");

                return;
            }

            base.OnActionExecuting(context);
        }

        // GET: /ThongKe
        public async Task<IActionResult> Index()
        {
            var model = await _thongKeRepository.GetThongKeAsync();

            return View(model);
        }
    }
}
