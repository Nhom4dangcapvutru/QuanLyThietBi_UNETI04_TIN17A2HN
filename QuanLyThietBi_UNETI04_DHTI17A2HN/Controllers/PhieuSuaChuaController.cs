// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Phiếu sửa chữa - Tạo phiếu, theo dõi, hủy phiếu
// Phân quyền: Khách hàng chỉ thấy phiếu của chính mình; Nhân viên/Admin quản lý toàn bộ phiếu hệ thống

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class PhieuSuaChuaController : Controller
    {
        private readonly IPhieuSuaChuaRepository _repository;

        public PhieuSuaChuaController(IPhieuSuaChuaRepository repository)
        {
            _repository = repository;
        }

        private async Task<int?> GetCurrentMaKhachHangAsync()
        {
            var maTaiKhoan = SessionHelper.GetMaTaiKhoan(HttpContext.Session);
            if (maTaiKhoan == null) return null;

            return await _repository.GetMaKhachHangByTaiKhoanAsync(maTaiKhoan.Value);
        }

        // ====================================================================
        // 1. INDEX
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            string? sapXep,
            int trang = 1)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem phiếu sửa chữa.";
                return RedirectToAction("Login", "Account");
            }

            var isAdminOrKtv = SessionHelper.IsAdminOrKyThuatVien(HttpContext.Session);
            int? maKhachHang = null;

            if (!isAdminOrKtv)
            {
                maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hồ sơ khách hàng liên kết với tài khoản này.";
                    return RedirectToAction("Index", "Home");
                }
            }

            const int kichThuocTrang = 10;

            // Gọi Repository lấy danh sách
            var (phieus, tongSoBanGhi) = await _repository.GetFilteredPhieuAsync(
                maKhachHang, isAdminOrKtv, tuKhoa, trangThai, sapXep, trang, kichThuocTrang);

            int tongSoTrang = (int)Math.Ceiling(tongSoBanGhi / (double)kichThuocTrang);

            if (trang < 1) trang = 1;
            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            ViewBag.SapXep = sapXep;
            ViewBag.TrangHienTai = trang;
            ViewBag.TongSoTrang = tongSoTrang;
            ViewBag.TongSoBanGhi = tongSoBanGhi;

            ViewBag.DanhSachTrangThai = new List<string>
            {
                TrangThaiPhieu.ChoTiepNhan,
                TrangThaiPhieu.DangXuLy,
                TrangThaiPhieu.HoanThanh,
                TrangThaiPhieu.DaHuy,
                TrangThaiPhieu.TuChoi
            };

            return View(phieus);
        }

        // ====================================================================
        // 2. DETAILS
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            var isAdminOrKtv = SessionHelper.IsAdminOrKyThuatVien(HttpContext.Session);
            int? maKhachHang = null;

            if (!isAdminOrKtv)
            {
                maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var phieu = await _repository.GetPhieuDetailsAsync(id);

            if (phieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            if (!isAdminOrKtv && maKhachHang.HasValue && phieu.MaKhachHang != maKhachHang.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem phiếu này.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PhieuSuaChuaDetailsViewModel
            {
                PhieuSuaChua = phieu,
                KhachHang = phieu.KhachHang,
                ThietBi = phieu.ThietBi,
                ChiTietSuaChuas = phieu.ChiTietSuaChuas?.ToList() ?? new List<ChiTietSuaChua>(),
                NguoiDangNhap_MaTaiKhoan = SessionHelper.GetMaTaiKhoan(HttpContext.Session),
                NguoiDangNhap_VaiTro = SessionHelper.GetVaiTro(HttpContext.Session)
            };

            return View(viewModel);
        }

        // ====================================================================
        // Xuất kết quả sửa chữa đã hoàn thành.
        // ====================================================================
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ExportExcel(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Account");

            var isStaff = SessionHelper.IsAdminOrKyThuatVien(HttpContext.Session);
            var maKhachHang = isStaff ? null : await GetCurrentMaKhachHangAsync();
            if (!isStaff && maKhachHang == null)
                return NotFound();

            var phieu = await _repository.GetPhieuDetailsAsync(id);
            if (phieu == null || (!isStaff && phieu.MaKhachHang != maKhachHang))
                return NotFound();

            if (phieu.TrangThai != TrangThaiPhieu.HoanThanh)
                return Conflict("Chỉ xuất Excel cho phiếu đã hoàn thành.");

            return File(PhieuSuaChuaExcel.Create(phieu),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"PhieuSuaChua-{phieu.MaPhieu}.xlsx");
        }

        // ====================================================================
        // 3. CREATE - GET
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tạo phiếu sửa chữa.";
                return RedirectToAction("Login", "Account");
            }

            var isAdminOrKtv = SessionHelper.IsAdminOrKyThuatVien(HttpContext.Session);
            int? maKhachHang = null;
            List<ThietBi> thietBis;

            if (isAdminOrKtv)
            {
                thietBis = await _repository.GetThietBiChoPhepTaoPhieuAsync(null, true);
                maKhachHang = thietBis.FirstOrDefault()?.MaKhachHang ?? 1;
            }
            else
            {
                maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction(nameof(Index));
                }

                if (!await _repository.KiemTraKhachHangHoatDongAsync(maKhachHang.Value))
                {
                    TempData["ErrorMessage"] = "Hồ sơ khách hàng đã ngừng hoạt động. Vui lòng liên hệ nhân viên để được hỗ trợ.";
                    return RedirectToAction(nameof(Index));
                }

                thietBis = await _repository.GetThietBiChoPhepTaoPhieuAsync(maKhachHang, false);
            }

            var viewModel = new PhieuSuaChuaCreateViewModel
            {
                MaKhachHang = maKhachHang ?? 1,
                TenKhachHang = SessionHelper.GetHoTen(HttpContext.Session) ?? "Khách hàng",
                DanhSachThietBi = thietBis.Select(t => new SelectListItem
                {
                    Value = t.MaThietBi.ToString(),
                    Text = $"{t.TenThietBi} (S/N: {t.SerialNumber}) - KH: {t.KhachHang?.HoTen ?? "N/A"}"
                }).ToList(),
                DanhSachMucDo = new List<SelectListItem>
                {
                    new() { Value = MucDoUuTien.Thap, Text = MucDoUuTien.Thap },
                    new() { Value = MucDoUuTien.TrungBinh, Text = MucDoUuTien.TrungBinh },
                    new() { Value = MucDoUuTien.Cao, Text = MucDoUuTien.Cao },
                    new() { Value = MucDoUuTien.KhanCap, Text = MucDoUuTien.KhanCap }
                }
            };

            return View(viewModel);
        }

        // ====================================================================
        // 4. CREATE - POST
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuSuaChuaCreateViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            var isAdminOrKtv = SessionHelper.IsAdminOrKyThuatVien(HttpContext.Session);
            int maKhachHangVal;

            if (isAdminOrKtv)
            {
                var tb = await _repository.GetThietBiKemKhachHangAsync(model.MaThietBi);
                maKhachHangVal = tb?.MaKhachHang ?? model.MaKhachHang;
            }
            else
            {
                var maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction(nameof(Index));
                }
                maKhachHangVal = maKhachHang.Value;
            }

            var thietBi = await _repository.GetThietBiKemKhachHangAsync(model.MaThietBi);

            if (thietBi == null || (!isAdminOrKtv && thietBi.MaKhachHang != maKhachHangVal))
            {
                ModelState.AddModelError("MaThietBi", "Thiết bị không tồn tại hoặc không thuộc quyền sở hữu.");
            }

            if (thietBi != null)
            {
                if (thietBi.KhachHang == null || thietBi.KhachHang.TrangThai != TrangThaiKhachHang.HoatDong)
                {
                    ModelState.AddModelError("", "Hồ sơ khách hàng không tồn tại hoặc đã ngừng hoạt động. Không thể tạo phiếu sửa chữa.");
                }

                if (thietBi.TrangThai != TrangThaiThietBi.HoatDong && thietBi.TrangThai != TrangThaiThietBi.Hong)
                {
                    ModelState.AddModelError("MaThietBi",
                        "Chỉ được tạo phiếu cho thiết bị ở trạng thái 'Hoạt động bình thường' hoặc 'Hỏng / Không hoạt động'. Thiết bị đang bảo hành hoặc sửa chữa phải hoàn tất phiếu hiện tại trước.");
                }

                if (await _repository.CoPhieuDangXuLyAsync(model.MaThietBi))
                {
                    ModelState.AddModelError("MaThietBi",
                        "Thiết bị này đang có phiếu sửa chữa chưa hoàn tất trong hệ thống.");
                }
            }

            if (ModelState.IsValid)
            {
                var phieu = new PhieuSuaChua
                {
                    MaThietBi = model.MaThietBi,
                    MaKhachHang = maKhachHangVal,
                    NgayTiepNhan = DateTime.Now,
                    NoiDungLoi = model.NoiDungLoi,
                    TrangThai = TrangThaiPhieu.ChoTiepNhan,
                    MucDo = model.MucDo,
                    HanDuKien = null,
                    NgayHoanThanh = null
                };

                await _repository.AddAsync(phieu);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = $"Đã tạo phiếu tiếp nhận sửa chữa thành công! Mã phiếu: #{phieu.MaPhieu}";
                return RedirectToAction(nameof(Details), new { id = phieu.MaPhieu });
            }

            // Nếu có lỗi, load lại dropdown
            var thietBisList = await _repository.GetThietBiChoPhepTaoPhieuAsync(maKhachHangVal, isAdminOrKtv);

            model.DanhSachThietBi = thietBisList.Select(t => new SelectListItem
            {
                Value = t.MaThietBi.ToString(),
                Text = $"{t.TenThietBi} (S/N: {t.SerialNumber})"
            }).ToList();

            model.DanhSachMucDo = new List<SelectListItem>
            {
                new() { Value = MucDoUuTien.Thap, Text = MucDoUuTien.Thap },
                new() { Value = MucDoUuTien.TrungBinh, Text = MucDoUuTien.TrungBinh },
                new() { Value = MucDoUuTien.Cao, Text = MucDoUuTien.Cao },
                new() { Value = MucDoUuTien.KhanCap, Text = MucDoUuTien.KhanCap }
            };
            model.MaKhachHang = maKhachHangVal;
            model.TenKhachHang = SessionHelper.GetHoTen(HttpContext.Session) ?? "Khách hàng";

            return View(model);
        }

        // ====================================================================
        // 5. HUY - GET
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Huy(int? id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            var isAdminOrKtv = SessionHelper.IsAdminOrKyThuatVien(HttpContext.Session);
            int? maKhachHang = null;

            if (!isAdminOrKtv)
            {
                maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var phieu = await _repository.GetPhieuDetailsAsync(id);

            if (phieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            if (!isAdminOrKtv && maKhachHang.HasValue && phieu.MaKhachHang != maKhachHang.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền hủy phiếu này.";
                return RedirectToAction(nameof(Index));
            }

            if (phieu.TrangThai != TrangThaiPhieu.ChoTiepNhan)
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy phiếu đang ở trạng thái 'Chờ tiếp nhận'.";
                return RedirectToAction(nameof(Details), new { id = phieu.MaPhieu });
            }

            return View(phieu);
        }

        // ====================================================================
        // 6. HUY - POST
        // ====================================================================
        [HttpPost, ActionName("Huy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyConfirmed(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            var isAdminOrKtv = SessionHelper.IsAdminOrKyThuatVien(HttpContext.Session);
            int? maKhachHang = null;

            if (!isAdminOrKtv)
            {
                maKhachHang = await GetCurrentMaKhachHangAsync();
                if (maKhachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var phieu = await _repository.GetPhieuByIdAsync(id);

            if (phieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            if (!isAdminOrKtv && maKhachHang.HasValue && phieu.MaKhachHang != maKhachHang.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền hủy phiếu này.";
                return RedirectToAction(nameof(Index));
            }

            if (phieu.TrangThai != TrangThaiPhieu.ChoTiepNhan)
            {
                TempData["ErrorMessage"] = "Không thể hủy phiếu. Chỉ hủy được khi phiếu ở trạng thái 'Chờ tiếp nhận'.";
                return RedirectToAction(nameof(Details), new { id = phieu.MaPhieu });
            }

            phieu.TrangThai = TrangThaiPhieu.DaHuy;
            await _repository.SaveAsync();

            TempData["SuccessMessage"] = $"Đã hủy phiếu #{phieu.MaPhieu} thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
