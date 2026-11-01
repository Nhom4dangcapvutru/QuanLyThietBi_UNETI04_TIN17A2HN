// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Quản lý khách hàng - CRUD, tìm kiếm, lọc, sắp xếp, phân trang
// Phân quyền: Admin/Kỹ thuật viên được truy cập; Khách hàng bị chặn.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly IKhachHangRepository _repository;

        public KhachHangController(IKhachHangRepository repository)
        {
            _repository = repository;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (!SessionHelper.IsLoggedIn(context.HttpContext.Session))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục.";
                context.Result = RedirectToAction("Login", "Account", new { returnUrl = returnUrl.ToString() });
                return;
            }

            if (!SessionHelper.IsAdminOrKyThuatVien(context.HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này. Chỉ Admin và Kỹ thuật viên mới được quản lý khách hàng.";
                context.Result = RedirectToAction("AccessDenied", "Account");
                return;
            }

            base.OnActionExecuting(context);
        }

        private bool KiemTraQuyenNhanVien()
        {
            return SessionHelper.IsAdminOrKyThuatVien(HttpContext.Session);
        }

        // 1. INDEX
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? loaiKhachHang,
            string? trangThai,
            string? sapXep,
            int trang = 1)
        {
            if (!KiemTraQuyenNhanVien())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            const int kichThuocTrang = 10;

            // Gọi Repository lấy dữ liệu
            var (khachHangs, tongSoBanGhi) = await _repository.GetFilteredAsync(
                tuKhoa, loaiKhachHang, trangThai, sapXep, trang, kichThuocTrang);

            int tongSoTrang = (int)Math.Ceiling(tongSoBanGhi / (double)kichThuocTrang);

            if (trang < 1) trang = 1;
            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;

            var viewModel = new KhachHangFilterViewModel
            {
                KhachHangs = khachHangs,
                TuKhoa = tuKhoa,
                LoaiKhachHang = loaiKhachHang,
                TrangThai = trangThai,
                SapXep = sapXep,
                TrangHienTai = trang,
                TongSoTrang = tongSoTrang,
                TongSoBanGhi = tongSoBanGhi,
                KichThuocTrang = kichThuocTrang
            };

            return View(viewModel);
        }

        // 2. DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (!KiemTraQuyenNhanVien())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            var khachHang = await _repository.GetDetailsByIdAsync(id);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(khachHang);
        }

        // 3. CREATE - GET
        [HttpGet]
        public IActionResult Create()
        {
            if (!KiemTraQuyenNhanVien())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // 4. CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("HoTen,SoDienThoai,Email,DiaChi,CCCD,LoaiKhachHang,TrangThai,GhiChu,MaSoThue")]
            KhachHang khachHang)
        {
            if (!KiemTraQuyenNhanVien())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra nghiệp vụ (gọi qua Repository)
            if (await _repository.CheckTrungSdtAsync(khachHang.SoDienThoai))
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được đăng ký cho khách hàng khác.");

            if (await _repository.CheckTrungEmailAsync(khachHang.Email))
                ModelState.AddModelError("Email", "Email này đã được đăng ký cho khách hàng khác.");

            if (!string.IsNullOrWhiteSpace(khachHang.CCCD) && await _repository.CheckTrungCccdAsync(khachHang.CCCD))
                ModelState.AddModelError("CCCD", "Số CCCD này đã được đăng ký.");

            if (khachHang.LoaiKhachHang == LoaiKhachHangConstant.DoanhNghiep
                && string.IsNullOrWhiteSpace(khachHang.MaSoThue))
            {
                ModelState.AddModelError("MaSoThue", "Khách hàng doanh nghiệp bắt buộc phải có mã số thuế.");
            }

            if (ModelState.IsValid)
            {
                khachHang.NgayTao = DateTime.Now;

                await _repository.AddAsync(khachHang);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = $"Đã thêm khách hàng '{khachHang.HoTen}' thành công.";
                return RedirectToAction(nameof(Index));
            }

            return View(khachHang);
        }

        // 5. EDIT - GET
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!KiemTraQuyenNhanVien())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            var khachHang = await _repository.GetByIdAsync(id);
            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(khachHang);
        }

        // 6. EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaKhachHang,MaTaiKhoan,HoTen,SoDienThoai,Email,DiaChi,CCCD,NgayTao,LoaiKhachHang,TrangThai,GhiChu,MaSoThue")]
            KhachHang khachHang)
        {
            if (!KiemTraQuyenNhanVien())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id != khachHang.MaKhachHang)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (await _repository.CheckTrungSdtAsync(khachHang.SoDienThoai, khachHang.MaKhachHang))
                ModelState.AddModelError("SoDienThoai", "Số điện thoại này đã được đăng ký cho khách hàng khác.");

            if (await _repository.CheckTrungEmailAsync(khachHang.Email, khachHang.MaKhachHang))
                ModelState.AddModelError("Email", "Email này đã được đăng ký cho khách hàng khác.");

            if (!string.IsNullOrWhiteSpace(khachHang.CCCD) && await _repository.CheckTrungCccdAsync(khachHang.CCCD, khachHang.MaKhachHang))
                ModelState.AddModelError("CCCD", "Số CCCD này đã được đăng ký.");

            if (khachHang.LoaiKhachHang == LoaiKhachHangConstant.DoanhNghiep
                && string.IsNullOrWhiteSpace(khachHang.MaSoThue))
            {
                ModelState.AddModelError("MaSoThue", "Khách hàng doanh nghiệp bắt buộc phải có mã số thuế.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateAsync(khachHang);
                    await _repository.SaveAsync();

                    TempData["SuccessMessage"] = $"Đã cập nhật khách hàng '{khachHang.HoTen}' thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _repository.ExistsAsync(khachHang.MaKhachHang))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                        return RedirectToAction(nameof(Index));
                    }
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(khachHang);
        }

        // 7. DELETE - GET
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!KiemTraQuyenNhanVien())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            var khachHang = await _repository.GetDetailsByIdAsync(id);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            return View(khachHang);
        }

        // 8. DELETE - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!KiemTraQuyenNhanVien())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy chi tiết để check các bảng liên quan (ThietBis, PhieuSuaChuas)
            var khachHang = await _repository.GetDetailsByIdAsync(id);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            if (khachHang.ThietBis.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng vì đang có thiết bị trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            if (khachHang.PhieuSuaChuas.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng vì đang có phiếu sửa chữa trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            string tenKhachHang = khachHang.HoTen;

            await _repository.DeleteAsync(khachHang);
            await _repository.SaveAsync();

            TempData["SuccessMessage"] = $"Đã xóa khách hàng '{tenKhachHang}' thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}