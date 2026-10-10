// Họ và tên: Nguyễn Thị Thảo
// Mã sinh viên: 23103100092
// Nội dung thực hiện: Phiếu sửa chữa - Tạo phiếu, theo dõi, hủy phiếu
// (Khách hàng chỉ thấy phiếu của chính mình; kiểm tra quyền sở hữu trên Controller)

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
        // 1. INDEX - Danh sách phiếu sửa chữa của tôi
        //     (Tìm kiếm + Lọc + Sắp xếp + Phân trang)
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
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Lấy mã khách hàng đang đăng nhập
            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            // ----------------------------------------------------------------
            // QUERY: CHỈ LẤY PHIẾU CỦA CHÍNH MÌNH
            // ----------------------------------------------------------------
            var query = _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .Where(p => p.MaKhachHang == maKhachHang.Value)
                .AsQueryable();

            // Tìm kiếm theo nội dung lỗi, tên thiết bị
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

            // Truyền dữ liệu qua ViewBag (đơn giản, không cần ViewModel phức tạp)
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            ViewBag.SapXep = sapXep;
            ViewBag.TrangHienTai = trang;
            ViewBag.TongSoTrang = tongSoTrang;
            ViewBag.TongSoBanGhi = tongSoBanGhi;

            // Danh sách trạng thái để render dropdown
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
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction(nameof(Index));
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

            // ================================================================
            // BẢO MẬT: Khách hàng chỉ được xem phiếu CỦA CHÍNH MÌNH
            // (Thay đổi mã trên URL cũng không xem được phiếu người khác)
            // ================================================================
            if (phieu.MaKhachHang != maKhachHang.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem phiếu này.";
                return RedirectToAction(nameof(Index));
            }

            // Tạo ViewModel
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
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy danh sách thiết bị của khách hàng này
            var thietBis = await _context.ThietBis
                .Where(t => t.MaKhachHang == maKhachHang.Value)
                .OrderBy(t => t.TenThietBi)
                .ToListAsync();

            // Tạo ViewModel
            var viewModel = new PhieuSuaChuaCreateViewModel
            {
                MaKhachHang = maKhachHang.Value,
                TenKhachHang = SessionHelper.GetHoTen(HttpContext.Session),
                DanhSachThietBi = thietBis.Select(t => new SelectListItem
                {
                    Value = t.MaThietBi.ToString(),
                    Text = $"{t.TenThietBi} (S/N: {t.SerialNumber})"
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
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            // ================================================================
            // KIỂM TRA NGHIỆP VỤ (theo yêu cầu đề bài mục 7.3)
            // ================================================================

            // 1. Khách hàng phải tồn tại & đang Hoạt động
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang.Value);

            if (khachHang == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
            }
            else if (khachHang.TrangThai != "Hoạt động")
            {
                ModelState.AddModelError("", "Tài khoản khách hàng đang bị khóa, không thể tạo phiếu.");
            }

            // 2. Thiết bị phải tồn tại & thuộc khách hàng đang đăng nhập
            var thietBi = await _context.ThietBis
                .FirstOrDefaultAsync(t => t.MaThietBi == model.MaThietBi
                                       && t.MaKhachHang == maKhachHang.Value);

            if (thietBi == null)
            {
                ModelState.AddModelError("MaThietBi",
                    "Thiết bị không tồn tại hoặc không thuộc quyền sở hữu của bạn.");
            }

            // 3. Không tạo phiếu trùng: cùng thiết bị đang có phiếu chưa hoàn thành
            if (thietBi != null)
            {
                bool coPhieuDangXuLy = await _context.PhieuSuaChuas
                    .AnyAsync(p => p.MaThietBi == model.MaThietBi
                                && (p.TrangThai == TrangThaiPhieu.ChoTiepNhan
                                 || p.TrangThai == TrangThaiPhieu.DangXuLy));

                if (coPhieuDangXuLy)
                {
                    ModelState.AddModelError("MaThietBi",
                        "Thiết bị này đang có phiếu sửa chữa chưa hoàn thành. Vui lòng chờ xử lý xong.");
                }
            }

            // ================================================================
            // NẾU HỢP LỆ → LƯU DATABASE
            // ================================================================
            if (ModelState.IsValid)
            {
                var phieu = new PhieuSuaChua
                {
                    MaThietBi = model.MaThietBi,
                    MaKhachHang = maKhachHang.Value,
                    NgayTiepNhan = DateTime.Now, // Hệ thống tự gán
                    NoiDungLoi = model.NoiDungLoi,
                    TrangThai = TrangThaiPhieu.ChoTiepNhan, // Mặc định
                    MucDo = model.MucDo,
                    HanDuKien = null,
                    NgayHoanThanh = null
                };

                _context.PhieuSuaChuas.Add(phieu);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã gửi phiếu sửa chữa thành công! Mã phiếu: #{phieu.MaPhieu}";
                return RedirectToAction(nameof(Details), new { id = phieu.MaPhieu });
            }

            // Nếu lỗi → load lại dropdown
            var thietBis = await _context.ThietBis
                .Where(t => t.MaKhachHang == maKhachHang.Value)
                .OrderBy(t => t.TenThietBi)
                .ToListAsync();

            model.DanhSachThietBi = thietBis.Select(t => new SelectListItem
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
            model.MaKhachHang = maKhachHang.Value;
            model.TenKhachHang = SessionHelper.GetHoTen(HttpContext.Session);

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
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            if (id == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            // Bảo mật: chỉ xem được phiếu của mình
            if (phieu.MaKhachHang != maKhachHang.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền hủy phiếu này.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trạng thái có cho phép hủy không
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
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var maKhachHang = await GetCurrentMaKhachHangAsync();
            if (maKhachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            var phieu = await _context.PhieuSuaChuas
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra quyền sở hữu
            if (phieu.MaKhachHang != maKhachHang.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền hủy phiếu này.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trạng thái cho phép hủy
            if (phieu.TrangThai != TrangThaiPhieu.ChoTiepNhan)
            {
                TempData["ErrorMessage"] = "Không thể hủy phiếu. Chỉ hủy được khi phiếu ở trạng thái 'Chờ tiếp nhận'.";
                return RedirectToAction(nameof(Details), new { id = phieu.MaPhieu });
            }

            // Cập nhật trạng thái → Đã hủy
            phieu.TrangThai = TrangThaiPhieu.DaHuy;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã hủy phiếu #{phieu.MaPhieu} thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}