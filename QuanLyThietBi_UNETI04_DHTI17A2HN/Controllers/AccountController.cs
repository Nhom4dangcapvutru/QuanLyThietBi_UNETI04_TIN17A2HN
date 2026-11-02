// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Xử lý Đăng nhập, Đăng xuất, lưu Session và điều hướng phân quyền.
// Áp dụng mô hình Repository (IAccountRepository) để tách biệt logic truy vấn CSDL, giúp code dễ bảo trì.

using Microsoft.AspNetCore.Mvc;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels; // Đã sửa lại cho đúng namespace
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories; // Thêm namespace Repository

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class AccountController : Controller
    {
        // Khai báo Repository thay vì dùng trực tiếp ApplicationDbContext
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập thì chuyển về trang chủ
            if (HttpContext.Session.GetInt32("MaTaiKhoan") != null)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid) return View(model);

            // Gọi Repository để kiểm tra thông tin đăng nhập
            var taiKhoan = await _accountRepository.LoginAsync(model.TenDangNhap, model.MatKhau);

            // 1. Sai thông tin
            if (taiKhoan == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // 2. Tài khoản bị khóa
            if (taiKhoan.TrangThai == TrangThaiTaiKhoan.BiKhoa)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin.");
                return View(model);
            }

            // 3. Lưu Session
            HttpContext.Session.SetInt32("MaTaiKhoan", taiKhoan.MaTaiKhoan);
            HttpContext.Session.SetString("TenDangNhap", taiKhoan.TenDangNhap);
            HttpContext.Session.SetString("HoTen", taiKhoan.HoTen);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);

            // 4. CÓ returnUrl → quay lại đúng trang đang muốn vào
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // 5. Không có returnUrl → theo vai trò
            return taiKhoan.VaiTro switch
            {
                VaiTroConstant.Admin => RedirectToAction("Index", "Home"),
                VaiTroConstant.KyThuatVien => RedirectToAction("Index", "Home"),
                VaiTroConstant.KhachHang => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}