// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Hồ sơ cá nhân - Khách hàng xem và cập nhật thông tin của mình
// Bảo mật: Chỉ cho phép sửa hồ sơ của chính mình (kiểm tra qua Session)

using Microsoft.AspNetCore.Mvc;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileRepository _repository;

        public ProfileController(IProfileRepository repository)
        {
            _repository = repository;
        }

        // ====================================================================
        // LẤY MÃ KHÁCH HÀNG TỪ SESSION
        // ====================================================================
        private async Task<int?> GetCurrentMaKhachHangAsync()
        {
            var maTaiKhoan = SessionHelper.GetMaTaiKhoan(HttpContext.Session);
            if (maTaiKhoan == null) return null;

            return await _repository.GetMaKhachHangByTaiKhoanAsync(maTaiKhoan.Value);
        }

        // ====================================================================
        // 1. INDEX - Xem hồ sơ cá nhân
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem hồ sơ cá nhân.";
                return RedirectToAction("Login", "Account");
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            // Gọi Repository lấy dữ liệu
            var khachHang = await _repository.GetProfileFullDetailsAsync(maKhachHang.Value);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

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
                return RedirectToAction("Login", "Account");
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            var khachHang = await _repository.GetByIdAsync(maKhachHang.Value);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

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
                return RedirectToAction("Login", "Account");
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null || maKhachHang.Value != model.MaKhachHang)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa hồ sơ của người khác.";
                return RedirectToAction("Index", "Home");
            }

            // Gọi Repository kiểm tra nghiệp vụ
            if (await _repository.CheckTrungSdtAsync(model.SoDienThoai, model.MaKhachHang))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được sử dụng.");
            }

            if (await _repository.CheckTrungEmailAsync(model.Email, model.MaKhachHang))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                // Lấy entity từ Repo
                var khachHang = await _repository.GetByIdAsync(model.MaKhachHang);

                if (khachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction(nameof(Index));
                }

                // CHỈ cập nhật các trường được phép
                khachHang.SoDienThoai = model.SoDienThoai;
                khachHang.Email = model.Email;
                khachHang.DiaChi = model.DiaChi;
                khachHang.CCCD = model.CCCD;

                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Đã cập nhật hồ sơ cá nhân thành công.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}