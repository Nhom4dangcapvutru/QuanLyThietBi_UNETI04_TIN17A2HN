using Microsoft.AspNetCore.Mvc;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;
using System.Diagnostics;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHomeRepository _repository;

        public HomeController(IHomeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập (được đảm bảo bởi BaseController, nhưng kiểm tra thêm cho an toàn)
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = await _repository.GetDashboardAsync(
                SessionHelper.GetVaiTro(HttpContext.Session),
                SessionHelper.GetMaTaiKhoan(HttpContext.Session));

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
