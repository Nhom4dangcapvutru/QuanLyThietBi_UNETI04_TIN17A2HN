// Họ và tên: Đinh Thị Thùy Linh
// Mã sinh viên: 23103100110
// Nội dung thực hiện: Tiếp nhận - Chẩn đoán - Sửa chữa - Cập nhật trạng thái và chi phí

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class XuLySuaChuaController : Controller
    {
        private const int PageSize = 8;
        private readonly IPhieuSuaChuaRepository _phieuRepo;
        private readonly IQuyTrinhSuaChuaService _quyTrinh;
        private readonly ILogger<XuLySuaChuaController> _logger;

        public XuLySuaChuaController(
            IPhieuSuaChuaRepository phieuRepo,
            IQuyTrinhSuaChuaService quyTrinh,
            ILogger<XuLySuaChuaController> logger)
        {
            _phieuRepo = phieuRepo;
            _quyTrinh = quyTrinh;
            _logger = logger;
        }

        // 1. DANH SÁCH
        public async Task<IActionResult> Index(
            string? searchString, string? trangThai, string? mucDo,
            int? maLoaiThietBi, DateTime? tuNgay, DateTime? denNgay,
            string? sortOrder, int pageIndex = 1)
        {
            var (items, totalItems) = await _phieuRepo.GetPagedForProcessingAsync(
                searchString, trangThai, mucDo, maLoaiThietBi,
                tuNgay, denNgay, sortOrder, pageIndex, PageSize);

            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSort"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["NameSort"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["StatusSort"] = sortOrder == "status_asc" ? "status_desc" : "status_asc";

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
                    await _phieuRepo.GetLoaiThietBisAsync(),
                    "MaLoai", "TenLoai", maLoaiThietBi)
            };

            return View(vm);
        }

        // 2. CHI TIẾT
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var phieu = await _phieuRepo.GetByIdWithDetailsAsync(id.Value);
            if (phieu == null) return NotFound();
            return View(phieu);
        }

        // 3. TIẾP NHẬN (GET)
        [HttpGet]
        public async Task<IActionResult> TiepNhan(int? id)
        {
            if (id == null) return NotFound();
            var phieu = await _phieuRepo.GetByIdForUpdateAsync(id.Value);

            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheTiepNhan(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = $"Chỉ tiếp nhận phiếu ở trạng thái '{TrangThaiPhieu.ChoTiepNhan}'.";
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

        // 3. TIẾP NHẬN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TiepNhan(TiepNhanViewModel vm)
        {
            var phieu = await _phieuRepo.GetByIdForUpdateAsync(vm.MaPhieu);
            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheTiepNhan(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = "Phiếu không còn ở trạng thái chờ tiếp nhận.";
                return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
            }

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp nhận phiếu.";
                return RedirectToAction("Login", "TaiKhoan");
            }

            phieu.TrangThai = TrangThaiPhieu.DangXuLy;
            phieu.MaKyThuatVienTiepNhan = maTaiKhoan.Value;
            phieu.MucDo = vm.MucDo;
            phieu.NgayCapNhat = DateTime.Now;

            if (phieu.ThietBi != null)
                phieu.ThietBi.TrangThai = TrangThaiThietBi.DangSuaChua;

            await _phieuRepo.SaveAsync();

            TempData["SuccessMessage"] = $"Đã tiếp nhận phiếu #{vm.MaPhieu}. Chuyển sang 'Đang xử lý'.";
            return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
        }

        // 4. CHẨN ĐOÁN (GET)
        [HttpGet]
        public async Task<IActionResult> ChanDoan(int? id)
        {
            if (id == null) return NotFound();
            var phieu = await _phieuRepo.GetByIdForUpdateAsync(id.Value);

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
                ConBaoHanh = phieu.ThietBi != null && DateTime.Now.Date <= phieu.ThietBi.HanBaoHanh.Date
            };
            return View(vm);
        }

        // 4. CHẨN ĐOÁN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChanDoan(ChanDoanViewModel vm)
        {
            var phieu = await _phieuRepo.GetByIdForUpdateAsync(vm.MaPhieu);
            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheChanDoan(phieu.TrangThai))
                ModelState.AddModelError("", "Phiếu không ở trạng thái cho phép chẩn đoán.");

            if (vm.HanDuKien.HasValue && vm.HanDuKien.Value.Date < DateTime.Now.Date)
                ModelState.AddModelError(nameof(vm.HanDuKien), "Hạn dự kiến không được trước ngày hiện tại.");

            if (!ModelState.IsValid)
            {
                vm.TenKhachHang = phieu.KhachHang?.HoTen ?? "";
                vm.TenThietBi = phieu.ThietBi?.TenThietBi ?? "";
                vm.SerialNumber = phieu.ThietBi?.SerialNumber ?? "";
                vm.NoiDungLoi = phieu.NoiDungLoi;
                vm.TrangThaiHienTai = phieu.TrangThai;
                vm.ConBaoHanh = phieu.ThietBi != null && DateTime.Now.Date <= phieu.ThietBi.HanBaoHanh.Date;
                return View(vm);
            }

            phieu.KetQuaChanDoan = vm.KetQuaChanDoan;
            phieu.ChiPhiDuKien = vm.ChiPhiDuKien;
            phieu.HanDuKien = vm.HanDuKien;
            phieu.NgayCapNhat = DateTime.Now;

            await _phieuRepo.SaveAsync();

            TempData["SuccessMessage"] = "Đã cập nhật kết quả chẩn đoán và chi phí dự kiến.";
            return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
        }

        // 5. SỬA CHỮA (GET)
        [HttpGet]
        public async Task<IActionResult> SuaChua(int? id)
        {
            if (id == null) return NotFound();
            var phieu = await _phieuRepo.GetByIdForUpdateAsync(id.Value, includeChiTiet: true);

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

                // ĐÃ SỬA: Gọi hàm lấy tất cả linh kiện từ Repo
                LinhKienList = new SelectList(
                    await _phieuRepo.GetAllLinhKiensAsync(),
                    "MaLinhKien", "TenLinhKien")
            };

            return View(vm);
        }

        // 5. THÊM LINH KIỆN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemLinhKien(SuaChuaViewModel vm)
        {
            var phieu = await _phieuRepo.GetByIdForUpdateAsync(vm.MaPhieu);
            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheSuaChua(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = "Phiếu không ở trạng thái cho phép sửa chữa.";
                return RedirectToAction(nameof(Details), new { id = vm.MaPhieu });
            }

            var linhKien = await _phieuRepo.GetLinhKienForUpdateAsync(vm.MaLinhKien);
            if (linhKien == null)
            {
                TempData["ErrorMessage"] = "Linh kiện không tồn tại.";
                return RedirectToAction(nameof(SuaChua), new { id = vm.MaPhieu });
            }

            if (linhKien.SoLuongTon < vm.SoLuong)
            {
                TempData["ErrorMessage"] = $"Linh kiện '{linhKien.TenLinhKien}' chỉ còn {linhKien.SoLuongTon} trong kho.";
                return RedirectToAction(nameof(SuaChua), new { id = vm.MaPhieu });
            }

            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            if (maTaiKhoan == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập.";
                return RedirectToAction("Login", "TaiKhoan");
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
            phieu.ChiTietSuaChuas.Add(chiTiet);
            phieu.NgayCapNhat = DateTime.Now;

            await _phieuRepo.SaveAsync();

            TempData["SuccessMessage"] = $"Đã thêm linh kiện '{linhKien.TenLinhKien}' vào phiếu.";
            return RedirectToAction(nameof(SuaChua), new { id = vm.MaPhieu });
        }

        // 5. XÓA LINH KIỆN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaLinhKien(int maChiTiet, int maPhieu)
        {
            var phieu = await _phieuRepo.GetByIdForUpdateAsync(maPhieu, includeChiTiet: true);
            if (phieu == null) return NotFound();

            if (!_quyTrinh.CoTheSuaChua(phieu.TrangThai))
            {
                TempData["ErrorMessage"] = "Không thể xóa linh kiện khi phiếu không ở trạng thái sửa chữa.";
                return RedirectToAction(nameof(SuaChua), new { id = maPhieu });
            }

            var chiTiet = phieu.ChiTietSuaChuas.FirstOrDefault(c => c.MaChiTiet == maChiTiet);
            if (chiTiet == null) return NotFound();

            if (chiTiet.LinhKien != null)
                chiTiet.LinhKien.SoLuongTon += chiTiet.SoLuong;

            phieu.ChiTietSuaChuas.Remove(chiTiet);
            phieu.NgayCapNhat = DateTime.Now;

            await _phieuRepo.SaveAsync();

            TempData["SuccessMessage"] = "Đã xóa linh kiện khỏi phiếu và hoàn trả tồn kho.";
            return RedirectToAction(nameof(SuaChua), new { id = maPhieu });
        }

        // 6. CẬP NHẬT TRẠNG THÁI (GET)
        [HttpGet]
        public async Task<IActionResult> CapNhatTrangThai(int? id)
        {
            if (id == null) return NotFound();
            var phieu = await _phieuRepo.GetByIdForUpdateAsync(id.Value, includeChiTiet: true);
            if (phieu == null) return NotFound();

            // ĐÃ SỬA: Hoàn thiện phần ViewModel bị cắt cụt
            // Bạn cần điều chỉnh lại các thuộc tính (properties) cho khớp với file CapNhatTrangThaiViewModel.cs thực tế của bạn.
            var vm = new CapNhatTrangThaiViewModel
            {
                MaPhieu = phieu.MaPhieu,
                TenKhachHang = phieu.KhachHang?.HoTen ?? "",
                TenThietBi = phieu.ThietBi?.TenThietBi ?? "",
                TrangThaiHienTai = phieu.TrangThai,
                DanhSachChiTiet = phieu.ChiTietSuaChuas.ToList(),
                ChiPhiDuKien = phieu.ChiPhiDuKien
            }; 

            return View(vm);
        }

    }  
}  