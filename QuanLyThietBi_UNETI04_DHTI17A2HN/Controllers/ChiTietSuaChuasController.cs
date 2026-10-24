// Họ và tên: Cung Đình Phước
// Mã sinh viên: 23103100311

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class ChiTietSuaChuasController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ChiTietSuaChuasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục.";
                context.Result = RedirectToAction("Login", "Account", new { returnUrl = returnUrl.ToString() });
                return;
            }

            if (vaiTro != VaiTroConstant.Admin && vaiTro != VaiTroConstant.KyThuatVien)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập chức năng này. Chỉ Admin và Kỹ thuật viên mới được quản lý chi tiết sửa chữa.";
                context.Result = RedirectToAction("AccessDenied", "Account");
                return;
            }

            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Index()
        {
        var data = await _context.ChiTietSuaChuas
            .Include(c => c.PhieuSuaChua)
            .Include(c => c.LinhKien)
            .Include(c => c.KyThuatVien)
            .OrderByDescending(c => c.MaChiTiet)
            .ToListAsync();

        return View(data);
    }

    // =====================================================
    // DETAILS
    // =====================================================

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var chiTiet = await _context.ChiTietSuaChuas
            .Include(c => c.PhieuSuaChua)
            .Include(c => c.LinhKien)
            .Include(c => c.KyThuatVien)
            .FirstOrDefaultAsync(c => c.MaChiTiet == id);

        if (chiTiet == null)
            return NotFound();

        return View(chiTiet);
    }

    // =====================================================
    // CREATE - GET
    // =====================================================

    public async Task<IActionResult> Create()
    {
        await LoadDropdown();

        return View();
    }

    // =====================================================
    // CREATE - POST
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MaPhieu,MaKyThuatVien,MaLinhKien,SoLuong,KetQuaChanDoan,GhiChu")]
            ChiTietSuaChua chiTiet)
    {
        var phieu = await _context.PhieuSuaChuas
            .FirstOrDefaultAsync(p => p.MaPhieu == chiTiet.MaPhieu);

        if (phieu != null && phieu.TrangThai != TrangThaiPhieu.DangXuLy)
            ModelState.AddModelError("MaPhieu", "Chỉ thay đổi linh kiện khi phiếu đang xử lý.");
        if (phieu == null)
        {
            ModelState.AddModelError(
                "MaPhieu",
                "Phiếu sửa chữa không tồn tại.");
        }

    
        var linhKien = await _context.LinhKiens
            .FirstOrDefaultAsync(l => l.MaLinhKien == chiTiet.MaLinhKien);

        if (linhKien == null)
        {
            ModelState.AddModelError(
                "MaLinhKien",
                "Linh kiện không tồn tại.");
        }

        if (linhKien?.TrangThai == TrangThaiLinhKien.NgungSuDung)
            ModelState.AddModelError("MaLinhKien", "Linh kiện này đã ngừng sử dụng.");

        var kyThuatVien = await _context.TaiKhoans
            .FirstOrDefaultAsync(t =>
                t.MaTaiKhoan == chiTiet.MaKyThuatVien);

        if (kyThuatVien == null)
        {
            ModelState.AddModelError(
                "MaKyThuatVien",
                "Kỹ thuật viên không tồn tại.");
        }

       
        if (chiTiet.SoLuong <= 0)
        {
            ModelState.AddModelError(
                "SoLuong",
                "Số lượng phải lớn hơn 0.");
        }

        if (linhKien != null)
        {
           
            if (chiTiet.SoLuong > linhKien.SoLuongTon)
            {
                ModelState.AddModelError(
                    "SoLuong",
                    $"Kho chỉ còn {linhKien.SoLuongTon} {linhKien.DonViTinh}.");
            }

           
            chiTiet.DonGia = linhKien.DonGia;

            chiTiet.ThanhTien =
                chiTiet.SoLuong * chiTiet.DonGia;
        }

        if (ModelState.IsValid)
        {
         
            linhKien!.SoLuongTon -= chiTiet.SoLuong;
            linhKien.TrangThai = TrangThaiLinhKien.TheoTonKho(linhKien.SoLuongTon, false);

           
            if (phieu != null)
            {
                phieu.NgayCapNhat = DateTime.Now;
            }

            _context.Add(chiTiet);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Thêm chi tiết sửa chữa thành công.";

            return RedirectToAction(nameof(Index));
        }

        await LoadDropdown(
            chiTiet.MaPhieu,
            chiTiet.MaKyThuatVien,
            chiTiet.MaLinhKien);

        return View(chiTiet);
    }

    // =====================================================
    // EDIT - GET
    // =====================================================

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var chiTiet = await _context.ChiTietSuaChuas
            .FindAsync(id);

        if (chiTiet == null)
            return NotFound();

        if (!await _context.PhieuSuaChuas.AnyAsync(p => p.MaPhieu == chiTiet.MaPhieu
            && p.TrangThai == TrangThaiPhieu.DangXuLy))
        {
            TempData["Error"] = "Phiếu không ở trạng thái cho phép thay đổi linh kiện.";
            return RedirectToAction(nameof(Index));
        }

        await LoadDropdown(
            chiTiet.MaPhieu,
            chiTiet.MaKyThuatVien,
            chiTiet.MaLinhKien);

        return View(chiTiet);
    }

    // =====================================================
    // EDIT - POST
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
           int id,
           [Bind("MaChiTiet,MaPhieu,MaKyThuatVien,MaLinhKien,SoLuong,KetQuaChanDoan,GhiChu")]
            ChiTietSuaChua model)
    {
        if (id != model.MaChiTiet)
            return NotFound();

        var oldData = await _context.ChiTietSuaChuas
            .FirstOrDefaultAsync(c =>
                c.MaChiTiet == id);

        if (oldData == null)
            return NotFound();

        var phieuGoc = await _context.PhieuSuaChuas.FindAsync(oldData.MaPhieu);
        var phieuMoi = await _context.PhieuSuaChuas.FindAsync(model.MaPhieu);
        if (phieuGoc?.TrangThai != TrangThaiPhieu.DangXuLy || phieuMoi?.TrangThai != TrangThaiPhieu.DangXuLy)
        {
            TempData["Error"] = "Chỉ thay đổi linh kiện giữa các phiếu đang xử lý. Phiếu đã nghiệm thu được giữ nguyên chi phí.";
            return RedirectToAction(nameof(Index));
        }

        var linhKien = await _context.LinhKiens
            .FirstOrDefaultAsync(l =>
                l.MaLinhKien == model.MaLinhKien);

        if (linhKien == null)
        {
            ModelState.AddModelError(
                "MaLinhKien",
                "Linh kiện không tồn tại.");

            await LoadDropdown(
                model.MaPhieu,
                model.MaKyThuatVien,
                model.MaLinhKien);

            return View(model);
        }

       
        if (linhKien.TrangThai == TrangThaiLinhKien.NgungSuDung)
            ModelState.AddModelError("MaLinhKien", "Linh kiện này đã ngừng sử dụng.");

        var linhKienCu = await _context.LinhKiens.FindAsync(oldData.MaLinhKien);
        var soLuongKhaDung = (long)linhKien.SoLuongTon
            + (oldData.MaLinhKien == model.MaLinhKien ? oldData.SoLuong : 0);
        if (soLuongKhaDung - model.SoLuong > int.MaxValue)
            ModelState.AddModelError("SoLuong", "Điều chỉnh số lượng vượt giới hạn tồn kho.");
        if (linhKienCu != null && oldData.MaLinhKien != model.MaLinhKien
            && oldData.SoLuong > int.MaxValue - linhKienCu.SoLuongTon)
            ModelState.AddModelError("MaLinhKien", "Hoàn trả linh kiện cũ vượt giới hạn tồn kho.");

      
        if (model.SoLuong <= 0)
        {
            ModelState.AddModelError(
                "SoLuong",
                "Số lượng phải lớn hơn 0.");
        }

        if (model.SoLuong > soLuongKhaDung)
        {
            ModelState.AddModelError(
                "SoLuong",
                $"Kho chỉ còn {linhKien.SoLuongTon} {linhKien.DonViTinh}.");
        }

        model.DonGia = linhKien.DonGia;

        model.ThanhTien =
            model.SoLuong * model.DonGia;

        if (ModelState.IsValid)
        {
           
            if (oldData.MaLinhKien == model.MaLinhKien)
                linhKien.SoLuongTon = (int)(soLuongKhaDung - model.SoLuong);
            else
            {
                if (linhKienCu != null)
                {
                    linhKienCu.SoLuongTon += oldData.SoLuong;
                    linhKienCu.TrangThai = TrangThaiLinhKien.TheoTonKho(linhKienCu.SoLuongTon,
                        linhKienCu.TrangThai == TrangThaiLinhKien.NgungSuDung);
                }
                linhKien.SoLuongTon -= model.SoLuong;
            }
            linhKien.TrangThai = TrangThaiLinhKien.TheoTonKho(linhKien.SoLuongTon, false);

            oldData.MaPhieu = model.MaPhieu;
            oldData.MaKyThuatVien = model.MaKyThuatVien;
            oldData.MaLinhKien = model.MaLinhKien;
            oldData.SoLuong = model.SoLuong;
            oldData.DonGia = model.DonGia;
            oldData.ThanhTien = model.ThanhTien;
            oldData.KetQuaChanDoan = model.KetQuaChanDoan;
            oldData.GhiChu = model.GhiChu;

            phieuGoc.NgayCapNhat = DateTime.Now;
            phieuMoi.NgayCapNhat = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật chi tiết sửa chữa thành công.";

            return RedirectToAction(nameof(Index));
        }


        await LoadDropdown(
            model.MaPhieu,
            model.MaKyThuatVien,
            model.MaLinhKien);

        return View(model);
    }

    // =====================================================
    // DELETE - GET
    // =====================================================

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var chiTiet = await _context.ChiTietSuaChuas
            .Include(c => c.PhieuSuaChua)
            .Include(c => c.LinhKien)
            .Include(c => c.KyThuatVien)
            .FirstOrDefaultAsync(c =>
                c.MaChiTiet == id);

        if (chiTiet == null)
            return NotFound();

        if (!await _context.PhieuSuaChuas.AnyAsync(p => p.MaPhieu == chiTiet.MaPhieu
            && p.TrangThai == TrangThaiPhieu.DangXuLy))
        {
            TempData["Error"] = "Phiếu không ở trạng thái cho phép thay đổi linh kiện.";
            return RedirectToAction(nameof(Index));
        }

        return View(chiTiet);
    }

    // =====================================================
    // DELETE - POST
    // =====================================================

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var chiTiet = await _context.ChiTietSuaChuas
            .FirstOrDefaultAsync(c =>
                c.MaChiTiet == id);

        if (chiTiet == null)
            return NotFound();

        var phieu = await _context.PhieuSuaChuas.FindAsync(chiTiet.MaPhieu);
        if (phieu?.TrangThai != TrangThaiPhieu.DangXuLy)
        {
            TempData["Error"] = "Không thể xóa linh kiện của phiếu ngoài trạng thái đang xử lý.";
            return RedirectToAction(nameof(Index));
        }

      
        var linhKien = await _context.LinhKiens
            .FirstOrDefaultAsync(l =>
                l.MaLinhKien == chiTiet.MaLinhKien);

        if (linhKien != null)
        {
            if (chiTiet.SoLuong > int.MaxValue - linhKien.SoLuongTon)
            {
                TempData["Error"] = "Hoàn trả linh kiện vượt giới hạn tồn kho.";
                return RedirectToAction(nameof(Index));
            }
            linhKien.SoLuongTon += chiTiet.SoLuong;
            linhKien.TrangThai = TrangThaiLinhKien.TheoTonKho(linhKien.SoLuongTon,
                linhKien.TrangThai == TrangThaiLinhKien.NgungSuDung);
        }


        phieu.NgayCapNhat = DateTime.Now;

        _context.ChiTietSuaChuas.Remove(chiTiet);

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Xóa chi tiết sửa chữa thành công.";

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // LOAD DROPDOWN
    // =====================================================

    private async Task LoadDropdown(
        int? maPhieu = null,
        int? maKyThuatVien = null,
        int? maLinhKien = null)
    {
        var phieuList = await _context.PhieuSuaChuas
            .Where(p => p.TrangThai == TrangThaiPhieu.DangXuLy)
            .OrderByDescending(p => p.MaPhieu)
            .ToListAsync();

        ViewBag.MaPhieu = new SelectList(
            phieuList,
            "MaPhieu",
            "MaPhieu",
            maPhieu);

        var kyThuatVienList = await _context.TaiKhoans
            .Where(t => t.VaiTro == VaiTroConstant.KyThuatVien
                     || t.VaiTro == VaiTroConstant.Admin)
            .OrderBy(t => t.HoTen)
            .ToListAsync();

        ViewBag.MaKyThuatVien = new SelectList(
            kyThuatVienList,
            "MaTaiKhoan",
            "HoTen",
            maKyThuatVien);

        var linhKienList = await _context.LinhKiens
            .Where(l => l.SoLuongTon > 0 && l.TrangThai != TrangThaiLinhKien.NgungSuDung)
            .OrderBy(l => l.TenLinhKien)
            .ToListAsync();

        ViewBag.MaLinhKien = new SelectList(
            linhKienList,
            "MaLinhKien",
            "TenLinhKien",
            maLinhKien);
    }
}
}
