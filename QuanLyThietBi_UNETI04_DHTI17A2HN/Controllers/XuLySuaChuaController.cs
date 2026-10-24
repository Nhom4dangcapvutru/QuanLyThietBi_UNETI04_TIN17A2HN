// Họ và tên: DINH THI THUY LINH
// Nội dung thực hiện: - Tiếp nhận, Chẩn đoán, Sửa chữa,
// Cập nhật trạng thái và chi phí phiếu sửa chữa

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Services;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class XuLySuaChuaController : Controller
    {
        private const int PageSize = 8;
        private readonly ApplicationDbContext _context;
        private readonly IQuyTrinhSuaChuaService _quyTrinh;
        private readonly ILogger<XuLySuaChuaController> _logger;

        public XuLySuaChuaController(
            ApplicationDbContext context,
            IQuyTrinhSuaChuaService quyTrinh,
            ILogger<XuLySuaChuaController> logger)
        {
            _context = context;
            _quyTrinh = quyTrinh;
            _logger = logger;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            // Kiểm tra đăng nhập
            if (!SessionHelper.IsLoggedIn(context.HttpContext.Session))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để truy cập quy trình Xử lý sửa chữa.";
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            // Kiểm tra phân quyền: Chỉ Admin và Kỹ thuật viên mới được vào phân hệ kỹ thuật
            if (!SessionHelper.IsAdminOrKyThuatVien(context.HttpContext.Session))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào phân hệ Xử lý kỹ thuật.";
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }

        // ============================================================
        // 1. DANH SÁCH CẦN XỬ LÝ (Tìm kiếm + Lọc + Sắp xếp + Phân trang)
        // ============================================================
        public async Task<IActionResult> Index(
            string? searchString, string? trangThai, string? mucDo,
            int? maLoaiThietBi, DateTime? tuNgay, DateTime? denNgay,
            string? sortOrder, int pageIndex = 1)
        {


            var query = _context.PhieuSuaChuas
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.KyThuatVienTiepNhan)
                .AsNoTracking()
                .AsQueryable();

            // A. TÌM KIẾM
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var s = searchString.Trim();
                query = query.Where(p =>
                    p.KhachHang!.HoTen.Contains(s) ||
                    p.ThietBi!.TenThietBi.Contains(s) ||
                    p.ThietBi.SerialNumber.Contains(s));
            }

            // B. LỌC KẾT HỢP
            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(p => p.TrangThai == trangThai);

            if (!string.IsNullOrWhiteSpace(mucDo))
                query = query.Where(p => p.MucDo == mucDo);

            if (maLoaiThietBi.HasValue && maLoaiThietBi.Value > 0)
                query = query.Where(p => p.ThietBi!.MaLoai == maLoaiThietBi.Value);

            if (tuNgay.HasValue)
                query = query.Where(p => p.NgayTiepNhan.Date >= tuNgay.Value.Date);

            if (denNgay.HasValue)
                query = query.Where(p => p.NgayTiepNhan.Date <= denNgay.Value.Date);

            // C. SẮP XẾP
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSort"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["NameSort"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["StatusSort"] = sortOrder == "status_asc" ? "status_desc" : "status_asc";

            query = sortOrder switch
            {
                "date_desc" => query.OrderByDescending(p => p.NgayTiepNhan),
                "name_asc" => query.OrderBy(p => p.KhachHang!.HoTen),
                "name_desc" => query.OrderByDescending(p => p.KhachHang!.HoTen),
                "status_asc" => query.OrderBy(p => p.TrangThai),
                "status_desc" => query.OrderByDescending(p => p.TrangThai),
                _ => query.OrderByDescending(p => p.NgayTiepNhan)
            };

            // D. PHÂN TRANG
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var dsTrangThai = new List<string>
            {
                TrangThaiPhieu.ChoTiepNhan, TrangThaiPhieu.DangXuLy,
                TrangThaiPhieu.HoanThanh, TrangThaiPhieu.DaHuy, TrangThaiPhieu.TuChoi
            };

            var dsMucDo = new List<string>
            {
                MucDoUuTien.Thap, MucDoUuTien.TrungBinh,
                MucDoUuTien.Cao, MucDoUuTien.KhanCap
            };

            var vm = new PhieuCanXuLyIndexViewModel
            {
                DanhSachPhieu = items,
                SearchString = searchString,
                TrangThai = trangThai,
                MucDo = mucDo,
                MaLoaiThietBi = maLoaiThietBi,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                SortOrder = sortOrder,
                PageIndex = pageIndex,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
                TrangThaiList = new SelectList(dsTrangThai, trangThai),
                MucDoList = new SelectList(dsMucDo, mucDo),
                LoaiThietBiList = new SelectList(
                    await _context.LoaiThietBis.AsNoTracking().ToListAsync(),
                    "MaLoai", "TenLoai", maLoaiThietBi)
            };

            return View(vm);
        }

        // ============================================================
        // 2. XEM CHI TIẾT PHIẾU
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.KyThuatVienTiepNhan)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.LinhKien)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.KyThuatVien)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();

            return View(phieu);
        }

        // ============================================================
        // 3. TIẾP NHẬN (Chờ tiếp nhận -> Đang xử lý)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> TiepNhan(int? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi).Include(p => p.KhachHang)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheTiepNhan(phieu.TrangThai))
            {
                TempData["ErrorMessage"] =
                    $"Chỉ tiếp nhận phiếu ở trạng thái '{TrangThaiPhieu.ChoTiepNhan}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (phieu.ThietBi == null || phieu.ThietBi.MaKhachHang != phieu.MaKhachHang)
            {
                TempData["ErrorMessage"] = "Thiết bị không thuộc khách hàng đã tạo phiếu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.TenKhachHang = phieu.KhachHang?.HoTen;
            ViewBag.TenThietBi = phieu.ThietBi.TenThietBi;
            ViewBag.SerialNumber = phieu.ThietBi.SerialNumber;
            ViewBag.NoiDungLoi = phieu.NoiDungLoi;

            var vm = new TiepNhanViewModel
            {
                MaPhieu = phieu.MaPhieu,
                MucDo = phieu.MucDo,
                DanhSachMucDo = new List<SelectListItem>
                {
                    new(MucDoUuTien.Thap, MucDoUuTien.Thap),
                    new(MucDoUuTien.TrungBinh, MucDoUuTien.TrungBinh),
                    new(MucDoUuTien.Cao, MucDoUuTien.Cao),
                    new(MucDoUuTien.KhanCap, MucDoUuTien.KhanCap)
                }
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TiepNhan(TiepNhanViewModel vm)
        {
            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .FirstOrDefaultAsync(p => p.MaPhieu == vm.MaPhieu);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheTiepNhan(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = "Phiếu không còn ở trạng thái chờ tiếp nhận.";
                return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
            }

            var maTaiKhoan = SessionHelper.GetMaTaiKhoan(HttpContext.Session);
            if (maTaiKhoan == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            phieu.TrangThai = TrangThaiPhieu.DangXuLy;
            phieu.MaKyThuatVienTiepNhan = maTaiKhoan.Value;
            phieu.MucDo = vm.MucDo;
            phieu.NgayCapNhat = DateTime.Now;

            if (phieu.ThietBi != null)
                phieu.ThietBi.TrangThai = TrangThaiThietBi.DangSuaChua;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Đã tiếp nhận phiếu #{vm.MaPhieu}. Chuyển sang 'Đang xử lý'.";
            return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
        }

        // ============================================================
        // 4. CHẨN ĐOÁN - BÁO CHI PHÍ DỰ KIẾN (Đang xử lý)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ChanDoan(int? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.KhachHang)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheChanDoan(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = "Chỉ chẩn đoán phiếu ở trạng thái 'Đang xử lý'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new ChanDoanViewModel
            {
                MaPhieu = phieu.MaPhieu,
                KetQuaChanDoan = phieu.KetQuaChanDoan ?? "",
                ChiPhiDuKien = phieu.ChiPhiDuKien,
                HanDuKien = phieu.HanDuKien,
                TenKhachHang = phieu.KhachHang?.HoTen ?? "",
                TenThietBi = phieu.ThietBi?.TenThietBi ?? "",
                SerialNumber = phieu.ThietBi?.SerialNumber ?? "",
                NoiDungLoi = phieu.NoiDungLoi,
                TrangThaiHienTai = phieu.TrangThai,
                ConBaoHanh = phieu.ThietBi != null
                    && DateTime.Now.Date <= phieu.ThietBi.HanBaoHanh.Date
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChanDoan(ChanDoanViewModel vm)
        {
            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi).ThenInclude(t => t!.LoaiThietBi)
                .Include(p => p.KhachHang)
                .FirstOrDefaultAsync(p => p.MaPhieu == vm.MaPhieu);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheChanDoan(phieu.TrangThai))
                ModelState.AddModelError("", "Phiếu không ở trạng thái cho phép chẩn đoán.");

            if (vm.HanDuKien.HasValue && vm.HanDuKien.Value.Date < DateTime.Now.Date)
                ModelState.AddModelError(nameof(vm.HanDuKien),
                    "Hạn dự kiến không được trước ngày hiện tại.");

            if (!ModelState.IsValid)
            {
                vm.TenKhachHang = phieu.KhachHang?.HoTen ?? "";
                vm.TenThietBi = phieu.ThietBi?.TenThietBi ?? "";
                vm.SerialNumber = phieu.ThietBi?.SerialNumber ?? "";
                vm.NoiDungLoi = phieu.NoiDungLoi;
                vm.TrangThaiHienTai = phieu.TrangThai;
                vm.ConBaoHanh = phieu.ThietBi != null
                    && DateTime.Now.Date <= phieu.ThietBi.HanBaoHanh.Date;
                return View(vm);
            }

            phieu.KetQuaChanDoan = vm.KetQuaChanDoan;
            phieu.ChiPhiDuKien = vm.ChiPhiDuKien;
            phieu.HanDuKien = vm.HanDuKien;
            phieu.NgayCapNhat = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật kết quả chẩn đoán và chi phí dự kiến.";
            return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
        }

        // ============================================================
        // 5. SỬA CHỮA - THÊM/XÓA LINH KIỆN (Đang xử lý)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SuaChua(int? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietSuaChuas).ThenInclude(c => c.LinhKien)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheSuaChua(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = "Chỉ sửa chữa phiếu ở trạng thái 'Đang xử lý'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new SuaChuaViewModel
            {
                MaPhieu = phieu.MaPhieu,
                TenKhachHang = phieu.KhachHang?.HoTen ?? "",
                TenThietBi = phieu.ThietBi?.TenThietBi ?? "",
                TrangThaiHienTai = phieu.TrangThai,
                DanhSachChiTiet = phieu.ChiTietSuaChuas.ToList(),
                TongTienLinhKien = phieu.ChiTietSuaChuas.Sum(c => c.ThanhTien),
                ChiPhiDuKien = phieu.ChiPhiDuKien,
                LinhKienList = new SelectList(
                    await _context.LinhKiens
                        .Where(l => l.SoLuongTon > 0)
                        .AsNoTracking()
                        .ToListAsync(),
                    "MaLinhKien", "TenLinhKien")
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemLinhKien(SuaChuaViewModel vm)
        {
            var phieu = await _context.PhieuSuaChuas
                .FirstOrDefaultAsync(p => p.MaPhieu == vm.MaPhieu);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheSuaChua(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = "Phiếu không ở trạng thái cho phép sửa chữa.";
                return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
            }

            var linhKien = await _context.LinhKiens
                .FirstOrDefaultAsync(l => l.MaLinhKien == vm.MaLinhKien);

            if (linhKien == null)
            {
                TempData["ErrorMessage"] = "Linh kiện không tồn tại.";
                return RedirectToAction(nameof(SuaChua), new { id = vm.MaPhieu });
            }

            if (linhKien.SoLuongTon < vm.SoLuong)
            {
                TempData["ErrorMessage"] =
                    $"Linh kiện '{linhKien.TenLinhKien}' chỉ còn {linhKien.SoLuongTon} trong kho.";
                return RedirectToAction(nameof(SuaChua), new { id = vm.MaPhieu });
            }

            var maTaiKhoan = SessionHelper.GetMaTaiKhoan(HttpContext.Session);
            if (maTaiKhoan == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            var chiTiet = new ChiTietSuaChua
            {
                MaPhieu = vm.MaPhieu,
                MaKyThuatVien = maTaiKhoan.Value,
                MaLinhKien = vm.MaLinhKien,
                SoLuong = vm.SoLuong,
                DonGia = linhKien.DonGia,
                ThanhTien = linhKien.DonGia * vm.SoLuong,
                KetQuaChanDoan = vm.GhiChu ?? "Thay thế linh kiện",
                GhiChu = vm.GhiChu
            };

            linhKien.SoLuongTon -= vm.SoLuong;
            _context.ChiTietSuaChuas.Add(chiTiet);
            phieu.NgayCapNhat = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thêm linh kiện '{linhKien.TenLinhKien}' vào phiếu.";
            return RedirectToAction(nameof(SuaChua), new { id = vm.MaPhieu });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaLinhKien(int maChiTiet, int maPhieu)
        {
            var chiTiet = await _context.ChiTietSuaChuas
                .Include(c => c.LinhKien)
                .FirstOrDefaultAsync(c => c.MaChiTiet == maChiTiet);

            if (chiTiet == null) return NotFound();

            var phieu = await _context.PhieuSuaChuas
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheSuaChua(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = "Không thể xóa linh kiện khi phiếu không ở trạng thái sửa chữa.";
                return RedirectToAction(nameof(SuaChua), new { id = maPhieu });
            }

            // Hoàn trả tồn kho
            if (chiTiet.LinhKien != null)
                chiTiet.LinhKien.SoLuongTon += chiTiet.SoLuong;

            _context.ChiTietSuaChuas.Remove(chiTiet);
            phieu.NgayCapNhat = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa linh kiện khỏi phiếu và hoàn trả tồn kho.";
            return RedirectToAction(nameof(SuaChua), new { id = maPhieu });
        }

        // ============================================================
        // 6. CẬP NHẬT TRẠNG THÁI VÀ CHI PHÍ CUỐI CÙNG
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> CapNhatTrangThai(int? id)
        {
            if (id == null) return NotFound();

            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietSuaChuas)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();

            var vm = new CapNhatTrangThaiViewModel
            {
                MaPhieu = phieu.MaPhieu,
                TenKhachHang = phieu.KhachHang?.HoTen ?? "",
                TenThietBi = phieu.ThietBi?.TenThietBi ?? "",
                SerialNumber = phieu.ThietBi?.SerialNumber ?? "",
                TrangThaiHienTai = phieu.TrangThai,
                TrangThaiMoi = "",
                ChiPhiDuKienHienThi = phieu.ChiPhiDuKien,
                TongChiPhi = phieu.TongChiPhi,
                NgayHenTra = phieu.NgayHenTra,
                LyDoHuy = phieu.LyDoHuy,
                ConBaoHanh = phieu.ThietBi != null
                    && DateTime.Now.Date <= phieu.ThietBi.HanBaoHanh.Date,
                TongTienLinhKien = phieu.ChiTietSuaChuas.Sum(c => c.ThanhTien),
                DanhSachTrangThaiChoPhep = _quyTrinh.LayTrangThaiChoPhep(phieu.TrangThai)
                    .Select(tt => new SelectListItem { Value = tt, Text = tt })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(CapNhatTrangThaiViewModel vm)
        {
            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietSuaChuas)
                .FirstOrDefaultAsync(p => p.MaPhieu == vm.MaPhieu);

            if (phieu == null) return NotFound();

            // ===== KIỂM TRA LUỒNG TRẠNG THÁI =====
            if (!_quyTrinh.KiemTraChuyenTrangThai(phieu.TrangThai, vm.TrangThaiMoi))
            {
                ModelState.AddModelError(nameof(vm.TrangThaiMoi),
                    $"Không thể chuyển từ '{phieu.TrangThai}' sang '{vm.TrangThaiMoi}'.");
            }

            // Nếu hoàn thành phải có tổng chi phí
            if (vm.TrangThaiMoi == TrangThaiPhieu.HoanThanh && !vm.TongChiPhi.HasValue)
            {
                ModelState.AddModelError(nameof(vm.TongChiPhi),
                    "Vui lòng nhập tổng chi phí thực tế trước khi hoàn thành.");
            }

            // Nếu hủy/từ chối phải có lý do
            if ((vm.TrangThaiMoi == TrangThaiPhieu.DaHuy ||
                 vm.TrangThaiMoi == TrangThaiPhieu.TuChoi) &&
                string.IsNullOrWhiteSpace(vm.LyDoHuy))
            {
                ModelState.AddModelError(nameof(vm.LyDoHuy),
                    "Vui lòng nhập lý do hủy / từ chối.");
            }

            // Nếu có tổng chi phí thì phải >= 0
            if (vm.TongChiPhi.HasValue && vm.TongChiPhi.Value < 0)
            {
                ModelState.AddModelError(nameof(vm.TongChiPhi),
                    "Tổng chi phí phải >= 0.");
            }

            if (!ModelState.IsValid)
            {
                vm.TenKhachHang = phieu.KhachHang?.HoTen ?? "";
                vm.TenThietBi = phieu.ThietBi?.TenThietBi ?? "";
                vm.SerialNumber = phieu.ThietBi?.SerialNumber ?? "";
                vm.TrangThaiHienTai = phieu.TrangThai;
                vm.ConBaoHanh = phieu.ThietBi != null
                    && DateTime.Now.Date <= phieu.ThietBi.HanBaoHanh.Date;
                vm.TongTienLinhKien = phieu.ChiTietSuaChuas.Sum(c => c.ThanhTien);
                vm.DanhSachTrangThaiChoPhep = _quyTrinh
                    .LayTrangThaiChoPhep(phieu.TrangThai)
                    .Select(tt => new SelectListItem { Value = tt, Text = tt })
                    .ToList();
                return View(vm);
            }

            // ===== CẬP NHẬT =====
            phieu.TrangThai = vm.TrangThaiMoi;
            phieu.TongChiPhi = vm.TongChiPhi;
            phieu.NgayHenTra = vm.NgayHenTra;
            phieu.LyDoHuy = vm.LyDoHuy;
            phieu.NgayCapNhat = DateTime.Now;

            // Khi HOÀN THÀNH: set ngày hoàn thành + cập nhật thiết bị
            if (vm.TrangThaiMoi == TrangThaiPhieu.HoanThanh)
            {
                phieu.NgayHoanThanh = DateTime.Now;

                if (phieu.ThietBi != null)
                    phieu.ThietBi.TrangThai = TrangThaiThietBi.HoatDong;
            }

            // Khi HỦY / TỪ CHỐI: cập nhật lại trạng thái thiết bị nếu cần
            if (vm.TrangThaiMoi == TrangThaiPhieu.DaHuy ||
                vm.TrangThaiMoi == TrangThaiPhieu.TuChoi)
            {
                if (phieu.ThietBi != null &&
                    phieu.ThietBi.TrangThai == TrangThaiThietBi.DangSuaChua)
                {
                    phieu.ThietBi.TrangThai = TrangThaiThietBi.HoatDong;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Đã cập nhật trạng thái phiếu thành '{vm.TrangThaiMoi}'.";
            return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
        }

        // ============================================================
        // 7. HỦY PHIẾU NHANH (chỉ khi ở trạng thái cho phép)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhieu(int id, string lyDo)
        {
            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.ThietBi)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheHuy(phieu.TrangThai))
            {
                TempData["ErrorMessage"] =
                    $"Không thể hủy phiếu ở trạng thái '{phieu.TrangThai}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do hủy phiếu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            phieu.TrangThai = TrangThaiPhieu.DaHuy;
            phieu.LyDoHuy = lyDo;
            phieu.NgayCapNhat = DateTime.Now;

            if (phieu.ThietBi != null &&
                phieu.ThietBi.TrangThai == TrangThaiThietBi.DangSuaChua)
            {
                phieu.ThietBi.TrangThai = TrangThaiThietBi.HoatDong;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã hủy phiếu #{id}.";
            return RedirectToAction(nameof(Index));
        }
    }
}