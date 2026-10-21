// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Phiếu sửa chữa - Tạo phiếu, theo dõi, hủy phiếu
// Phân quyền: Khách hàng chỉ thấy phiếu của chính mình; Nhân viên/Admin quản lý toàn bộ phiếu hệ thống

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class PhieuSuaChuaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhieuSuaChuaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====================================================================
        // HELPER: Lấy mã khách hàng đang đăng nhập
        // ====================================================================
        private async Task<int?> GetCurrentMaKhachHangAsync()
        {
            var maTaiKhoan = SessionHelper.GetMaTaiKhoan(HttpContext.Session);
            if (maTaiKhoan == null) return null;

            var kh = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);

            return kh?.MaKhachHang;
        }

        // ====================================================================
        // 1. INDEX - Danh sách phiếu sửa chữa
        //     (Khách hàng: chỉ xem phiếu của mình; Admin/KTV: xem toàn bộ phiếu)
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            string? sapXep,
            int trang = 1)
        {
            // Kiểm tra đăng nhập
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

            // ----------------------------------------------------------------
            // QUERY
            // ----------------------------------------------------------------
            var query = _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .AsQueryable();

            // Nếu là Khách hàng: Chỉ lấy phiếu của chính mình
            if (!isAdminOrKtv && maKhachHang.HasValue)
            {
                query = query.Where(p => p.MaKhachHang == maKhachHang.Value);
            }

            // Tìm kiếm theo nội dung lỗi, tên thiết bị, serial
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(p =>
                    p.NoiDungLoi.Contains(tuKhoa) ||
                    (p.ThietBi != null && p.ThietBi.TenThietBi.Contains(tuKhoa)) ||
                    (p.ThietBi != null && p.ThietBi.SerialNumber.Contains(tuKhoa)));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            // Sắp xếp
            query = sapXep switch
            {
                "ngay_asc" => query.OrderBy(p => p.NgayTiepNhan),
                "ngay_desc" => query.OrderByDescending(p => p.NgayTiepNhan),
                "trangthai" => query.OrderBy(p => p.TrangThai).ThenByDescending(p => p.NgayTiepNhan),
                _ => query.OrderByDescending(p => p.NgayTiepNhan) // Mặc định: mới nhất trước
            };

            // Phân trang
            const int kichThuocTrang = 10;
            int tongSoBanGhi = await query.CountAsync();
            int tongSoTrang = (int)Math.Ceiling(tongSoBanGhi / (double)kichThuocTrang);

            if (trang < 1) trang = 1;
            if (trang > tongSoTrang && tongSoTrang > 0) trang = tongSoTrang;

            var phieus = await query
                .Skip((trang - 1) * kichThuocTrang)
                .Take(kichThuocTrang)
                .ToListAsync();

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
        // 2. DETAILS - Xem chi tiết phiếu sửa chữa
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

            // Lấy phiếu + kiểm tra quyền sở hữu
            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                    .ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietSuaChuas)
                    .ThenInclude(c => c.LinhKien)
                .Include(p => p.ChiTietSuaChuas)
                    .ThenInclude(c => c.KyThuatVien)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            // BẢO MẬT: Khách hàng chỉ được xem phiếu CỦA CHÍNH MÌNH
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
        // 3. CREATE - GET: Hiển thị form tạo phiếu sửa chữa
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
                thietBis = await _context.ThietBis
                    .Include(t => t.KhachHang)
                    .OrderBy(t => t.TenThietBi)
                    .ToListAsync();
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

                thietBis = await _context.ThietBis
                    .Where(t => t.MaKhachHang == maKhachHang.Value)
                    .OrderBy(t => t.TenThietBi)
                    .ToListAsync();
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
        // 4. CREATE - POST: Xử lý tạo phiếu sửa chữa
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
                var tb = await _context.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == model.MaThietBi);
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

            // 1. Kiểm tra thiết bị
            var thietBi = await _context.ThietBis
                .FirstOrDefaultAsync(t => t.MaThietBi == model.MaThietBi && (isAdminOrKtv || t.MaKhachHang == maKhachHangVal));

            if (thietBi == null)
            {
                ModelState.AddModelError("MaThietBi", "Thiết bị không tồn tại hoặc không thuộc quyền sở hữu.");
            }

            // 2. Không tạo phiếu trùng nếu đang có phiếu chưa hoàn thành
            if (thietBi != null)
            {
                bool coPhieuDangXuLy = await _context.PhieuSuaChuas
                    .AnyAsync(p => p.MaThietBi == model.MaThietBi
                                && (p.TrangThai == TrangThaiPhieu.ChoTiepNhan
                                 || p.TrangThai == TrangThaiPhieu.DangXuLy));

                if (coPhieuDangXuLy)
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

                _context.PhieuSuaChuas.Add(phieu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã tạo phiếu tiếp nhận sửa chữa thành công! Mã phiếu: #{phieu.MaPhieu}";
                return RedirectToAction(nameof(Details), new { id = phieu.MaPhieu });
            }

            // Nếu lỗi → load lại dropdown
            List<ThietBi> thietBisList = isAdminOrKtv
                ? await _context.ThietBis.Include(t => t.KhachHang).OrderBy(t => t.TenThietBi).ToListAsync()
                : await _context.ThietBis.Where(t => t.MaKhachHang == maKhachHangVal).OrderBy(t => t.TenThietBi).ToListAsync();

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
        // 5. HUY - GET: Hiển thị trang xác nhận hủy phiếu
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

            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            // Bảo mật: chỉ xem được phiếu của mình (nếu là khách)
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
        // 6. HUY - POST: Xử lý hủy phiếu
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

            var phieu = await _context.PhieuSuaChuas.FirstOrDefaultAsync(p => p.MaPhieu == id);

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
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã hủy phiếu #{phieu.MaPhieu} thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
