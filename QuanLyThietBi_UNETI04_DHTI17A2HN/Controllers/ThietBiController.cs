// Họ và tên: Nguyễn Văn Tú
// Mã sinh viên: 23103100077
// Nội dung thực hiện: Module 2 - Quản lý thiết bị: CRUD, Tìm kiếm, Lọc kết hợp, Sắp xếp, Phân trang LINQ
// Phân quyền: Khách hàng chỉ xem thiết bị của mình; Nhân viên/Admin quản lý CRUD

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class ThietBiController : BaseController
    {
        private const int PageSize = 6; // Số lượng bản ghi trên 1 trang  
        private readonly IThietBiRepository _repository;

        public ThietBiController(IThietBiRepository repository)
        {
            _repository = repository;
        }

        private bool IsKhachHang()
        {
            return HttpContext.Session.GetString("VaiTro") == VaiTroConstant.KhachHang;
        }

        // ==========================================
        // 1. DANH SÁCH + TÌM KIẾM + LỌC + SẮP XẾP + PHÂN TRANG
        // ==========================================
        public async Task<IActionResult> Index(
            string? searchString,
            int? maLoai,
            string? hangSanXuat,
            string? trangThaiBaoHanh,
            string? trangThaiThietBi,
            decimal? giaTu,
            decimal? giaDen,
            string? sortOrder,
            int pageIndex = 1)
        {
            var filter = new ThietBiFilter
            {
                SearchString = searchString,
                MaLoai = maLoai,
                HangSanXuat = hangSanXuat,
                TrangThaiBaoHanh = trangThaiBaoHanh,
                TrangThaiThietBi = trangThaiThietBi,
                GiaTu = giaTu,
                GiaDen = giaDen,
                SortOrder = sortOrder,
                PageIndex = pageIndex,
                PageSize = PageSize
            };
            var paginatedItems = await _repository.SearchAsync(filter,
                HttpContext.Session.GetString("VaiTro"), HttpContext.Session.GetInt32("MaTaiKhoan"));

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["BuyDateSortParam"] = sortOrder == "date_buy_asc" ? "date_buy_desc" : "date_buy_asc";
            ViewData["WarrantySortParam"] = sortOrder == "warranty_asc" ? "warranty_desc" : "warranty_asc";
            ViewData["PriceSortParam"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            // E. LẤY DỮ LIỆU CHO CÁC DROPDOWN
            var dsLoai = await _repository.GetLoaiThietBisAsync();
            var dsHang = dsLoai.Select(l => l.HangSanXuat).Distinct().ToList();
            var dsTrangThai = new List<string>
            {
                TrangThaiThietBi.HoatDong,
                TrangThaiThietBi.DangBaoHanh,
                TrangThaiThietBi.DangSuaChua,
                TrangThaiThietBi.Hong
            };

            var viewModel = new ThietBiIndexViewModel
            {
                DanhSachThietBi = paginatedItems,
                SearchString = searchString,
                MaLoai = maLoai,
                HangSanXuat = hangSanXuat,
                TrangThaiBaoHanh = trangThaiBaoHanh,
                TrangThaiThietBi = trangThaiThietBi,
                GiaTu = giaTu,
                GiaDen = giaDen,
                SortOrder = sortOrder,
                LoaiThietBiList = new SelectList(dsLoai, "MaLoai", "TenLoai", maLoai),
                HangSanXuatList = new SelectList(dsHang, hangSanXuat),
                TrangThaiList = new SelectList(dsTrangThai, trangThaiThietBi)
            };

            return View(viewModel);
        }

        // ==========================================
        // 2. CHI TIẾT THIẾT BỊ (Khách hàng chỉ xem thiết bị thuộc sở hữu)
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var thietBi = await _repository.GetDetailsAsync(id.Value,
                HttpContext.Session.GetString("VaiTro"), HttpContext.Session.GetInt32("MaTaiKhoan"));

            if (thietBi == null) return NotFound();

            return View(thietBi);
        }

        // ==========================================
        // 3. THÊM MỚI THIẾT BỊ (CREATE - Chỉ Nhân viên/Admin)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (IsKhachHang())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thêm mới thiết bị. Chức năng này dành cho Nhân viên/Admin.";
                return RedirectToAction("AccessDenied", "Account");
            }

            await LoadDropdownDataAsync();
            var model = new ThietBi
            {
                NgayMua = DateTime.Now.Date,
                HanBaoHanh = DateTime.Now.Date.AddYears(1)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThietBi thietBi)
        {
            if (IsKhachHang())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thêm mới thiết bị. Chức năng này dành cho Nhân viên/Admin.";
                return RedirectToAction("AccessDenied", "Account");
            }

            // Kiểm tra trùng SerialNumber
            if (await _repository.SerialExistsAsync(thietBi.SerialNumber))
            {
                ModelState.AddModelError("SerialNumber", "Số Serial (S/N) này đã tồn tại trong hệ thống.");
            }

            // Kiểm tra nghiệp vụ ngày
            if (thietBi.HanBaoHanh < thietBi.NgayMua)
            {
                ModelState.AddModelError("HanBaoHanh", "Hạn bảo hành không được trước ngày mua thiết bị.");
            }

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(thietBi);
                TempData["SuccessMessage"] = "Thêm mới thiết bị thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownDataAsync(thietBi.MaLoai, thietBi.MaKhachHang);
            return View(thietBi);
        }

        // ==========================================
        // 4. CHỈNH SỬA THIẾT BỊ (EDIT - Chỉ Nhân viên/Admin)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (IsKhachHang())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa thiết bị. Chức năng này dành cho Nhân viên/Admin.";
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null) return NotFound();

            var thietBi = await _repository.GetByIdAsync(id.Value);
            if (thietBi == null) return NotFound();

            await LoadDropdownDataAsync(thietBi.MaLoai, thietBi.MaKhachHang);
            return View(thietBi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThietBi thietBi)
        {
            if (IsKhachHang())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa thiết bị. Chức năng này dành cho Nhân viên/Admin.";
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id != thietBi.MaThietBi) return NotFound();

            if (await _repository.SerialExistsAsync(thietBi.SerialNumber, id))
            {
                ModelState.AddModelError("SerialNumber", "Số Serial này đang trùng với thiết bị khác.");
            }

            if (thietBi.HanBaoHanh < thietBi.NgayMua)
            {
                ModelState.AddModelError("HanBaoHanh", "Hạn bảo hành không được trước ngày mua thiết bị.");
            }

            if (ModelState.IsValid)
            {
                if (!await _repository.UpdateAsync(thietBi)) return NotFound();
                TempData["SuccessMessage"] = "Cập nhật thông tin thiết bị thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownDataAsync(thietBi.MaLoai, thietBi.MaKhachHang);
            return View(thietBi);
        }

        // ==========================================
        // 5. XÓA THIẾT BỊ (DELETE - Chỉ Nhân viên/Admin)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (IsKhachHang())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa thiết bị. Chức năng này dành cho Nhân viên/Admin.";
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null) return NotFound();

            var thietBi = await _repository.GetDetailsAsync(id.Value,
                HttpContext.Session.GetString("VaiTro"), HttpContext.Session.GetInt32("MaTaiKhoan"));

            if (thietBi == null) return NotFound();

            var countPhieu = await _repository.CountHistoryAsync(id.Value);
            ViewBag.HasHistory = countPhieu > 0;
            ViewBag.HistoryCount = countPhieu;

            return View(thietBi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (IsKhachHang())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa thiết bị. Chức năng này dành cho Nhân viên/Admin.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var hasHistory = (await _repository.CountHistoryAsync(id)) > 0;
            if (hasHistory)
            {
                TempData["ErrorMessage"] = "Không thể xóa! Thiết bị này đã có lịch sử sửa chữa/bảo hành trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            if (await _repository.DeleteAsync(id))
            {
                TempData["SuccessMessage"] = "Đã xóa thiết bị thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownDataAsync(int? selectedLoai = null, int? selectedKhachHang = null)
        {
            ViewBag.MaLoai = new SelectList(await _repository.GetLoaiThietBisAsync(), "MaLoai", "TenLoai", selectedLoai);
            ViewBag.MaKhachHang = new SelectList(await _repository.GetKhachHangsAsync(), "MaKhachHang", "HoTen", selectedKhachHang);
        }
    }
}
