// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Hồ sơ cá nhân - Khách hàng xem và cập nhật thông tin của mình
// Bảo mật: Chỉ cho phép sửa hồ sơ của chính mình (kiểm tra qua Session)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====================================================================
        // LẤY MÃ KHÁCH HÀNG TỪ SESSION
        // Từ MaTaiKhoan (Session) → truy vấn bảng KhachHang để lấy MaKhachHang
        // ====================================================================
        private async Task<int?> GetCurrentMaKhachHangAsync()
        {
            var maTaiKhoan = SessionHelper.GetMaTaiKhoan(HttpContext.Session);
            if (maTaiKhoan == null) return null;

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            return khachHang?.MaKhachHang;
        }

        // ====================================================================
        // 1. INDEX - Xem hồ sơ cá nhân
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem hồ sơ cá nhân.";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Lấy mã khách hàng từ Session
            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin khách hàng + đếm số thiết bị, phiếu
            var khachHang = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .Include(k => k.ThietBis)
                .Include(k => k.PhieuSuaChuas)
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang.Value);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            // Tạo ViewModel
            var viewModel = new ProfileViewModel
            {
                MaKhachHang = khachHang.MaKhachHang,
                TenDangNhap = khachHang.TaiKhoan?.TenDangNhap,
                NgayTao = khachHang.NgayTao,
                HoTen = khachHang.HoTen,
                SoDienThoai = khachHang.SoDienThoai,
                Email = khachHang.Email,
                DiaChi = khachHang.DiaChi,
                CCCD = khachHang.CCCD,
                LoaiKhachHang = khachHang.LoaiKhachHang,
                MaSoThue = khachHang.MaSoThue,
                TrangThai = khachHang.TrangThai,
                GhiChu = khachHang.GhiChu,
                TongSoThietBi = khachHang.ThietBis?.Count ?? 0,
                TongSoPhieu = khachHang.PhieuSuaChuas?.Count ?? 0,
                SoPhieuDangXuLy = khachHang.PhieuSuaChuas?
                    .Count(p => p.TrangThai == "Chờ tiếp nhận" || p.TrangThai == "Đang xử lý") ?? 0
            };

            return View(viewModel);
        }

        // ====================================================================
        // 2. EDIT - GET: Hiển thị form sửa hồ sơ
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để sửa hồ sơ.";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang.Value);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            // Chỉ cho sửa các trường được phép
            var viewModel = new ProfileViewModel
            {
                MaKhachHang = khachHang.MaKhachHang,
                HoTen = khachHang.HoTen,
                SoDienThoai = khachHang.SoDienThoai,
                Email = khachHang.Email,
                DiaChi = khachHang.DiaChi,
                CCCD = khachHang.CCCD,
                LoaiKhachHang = khachHang.LoaiKhachHang,
                MaSoThue = khachHang.MaSoThue
            };

            return View(viewModel);
        }

        // ====================================================================
        // 3. EDIT - POST: Xử lý sửa hồ sơ
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để sửa hồ sơ.";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Lấy mã khách hàng từ Session (KHÔNG tin tưởng dữ liệu từ form)
            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null || maKhachHang.Value != model.MaKhachHang)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa hồ sơ của người khác.";
                return RedirectToAction("Index", "Home");
            }

            // ----------------------------------------------------------------
            // KIỂM TRA NGHIỆP VỤ
            // ----------------------------------------------------------------
            // Kiểm tra SĐT không trùng với khách hàng khác
            bool trungSdt = await _context.KhachHangs
                .AnyAsync(k => k.SoDienThoai == model.SoDienThoai
                            && k.MaKhachHang != model.MaKhachHang);

            if (trungSdt)
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được sử dụng.");
            }

            // Kiểm tra Email không trùng
            bool trungEmail = await _context.KhachHangs
                .AnyAsync(k => k.Email == model.Email
                            && k.MaKhachHang != model.MaKhachHang);

            if (trungEmail)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                // Lấy entity từ DB
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaKhachHang == model.MaKhachHang);

                if (khachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction(nameof(Index));
                }

                // CHỈ cập nhật các trường được phép
                // (KHÔNG cho sửa: HoTen, MaTaiKhoan, TrangThai, LoaiKhachHang, NgayTao, GhiChu)
                khachHang.SoDienThoai = model.SoDienThoai;
                khachHang.Email = model.Email;
                khachHang.DiaChi = model.DiaChi;
                khachHang.CCCD = model.CCCD;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã cập nhật hồ sơ cá nhân thành công.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}