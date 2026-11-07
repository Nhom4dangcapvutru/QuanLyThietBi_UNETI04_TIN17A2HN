// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Xử lý Đăng nhập, Đăng xuất, Đăng ký, lưu Session và điều hướng phân quyền.
// Áp dụng mô hình Repository (IAccountRepository) để tách biệt logic truy vấn CSDL, giúp code dễ bảo trì.

using Microsoft.AspNetCore.Mvc;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.ViewModels;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels; // Giữ nguyên như file cũ của mày

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        // ==========================================
        // 1. CHỨC NĂNG ĐĂNG NHẬP (GIỮ NGUYÊN CODE CŨ)
        // ==========================================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập thì chuyển về trang chủ
            if (HttpContext.Session.GetInt32("MaTaiKhoan") != null)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

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

        // ==========================================
        // 2. CHỨC NĂNG ĐĂNG KÝ (CHỈ THÊM MỚI PHẦN NÀY)
        // ==========================================
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("MaTaiKhoan") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Đóng gói dữ liệu từ ViewModel sang Entity Tài khoản
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau,
                HoTen = model.HoTen,
                Email = model.Email,
                VaiTro = VaiTroConstant.KhachHang, // Mặc định đăng ký là Khách hàng
                TrangThai = TrangThaiTaiKhoan.HoatDong
            };

            // Đóng gói dữ liệu sang Entity Khách hàng
            var khachHang = new KhachHang
            {
                HoTen = model.HoTen,
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                NgayTao = DateTime.Now,
                LoaiKhachHang = "Cá nhân",
                TrangThai = "Hoạt động"
            };

            // Gọi Repository để thực thi lưu (có sử dụng Transaction)
            var result = await _accountRepository.RegisterAsync(taiKhoan, khachHang);

            if (result)
            {
                TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError("", "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại.");
            return View(model);
        }

        // ==========================================
        // 3. CÁC CHỨC NĂNG KHÁC (GIỮ NGUYÊN CODE CŨ)
        // ==========================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}