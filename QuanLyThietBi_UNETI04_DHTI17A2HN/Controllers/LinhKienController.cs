using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers;

// Kho linh kiện: Admin quản lý; kỹ thuật viên chỉ xem danh mục và tồn kho.
public class LinhKienController(ApplicationDbContext context) : BaseController
{
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
        base.OnActionExecuting(actionContext);
        if (actionContext.Result != null) return;
        var role = actionContext.HttpContext.Session.GetString("VaiTro");
        var action = actionContext.RouteData.Values["action"]?.ToString();
        if ((role != VaiTroConstant.Admin && role != VaiTroConstant.KyThuatVien)
            || (role != VaiTroConstant.Admin
                && !string.Equals(action, nameof(Index), StringComparison.OrdinalIgnoreCase)
                && !string.Equals(action, nameof(Details), StringComparison.OrdinalIgnoreCase)))
            actionContext.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa, string? loai, string? trangThai,
        string? sapXep, int trang = 1)
    {
        var query = context.LinhKiens.AsNoTracking();
        tuKhoa = tuKhoa?.Trim();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
            query = query.Where(l => l.TenLinhKien.Contains(tuKhoa) || l.LoaiLinhKien.Contains(tuKhoa));
        if (!string.IsNullOrWhiteSpace(loai)) query = query.Where(l => l.LoaiLinhKien == loai);
        query = trangThai switch
        {
            TrangThaiLinhKien.NgungSuDung => query.Where(l => l.TrangThai == TrangThaiLinhKien.NgungSuDung),
            TrangThaiLinhKien.ConHang => query.Where(l => l.TrangThai != TrangThaiLinhKien.NgungSuDung && l.SoLuongTon > 0),
            TrangThaiLinhKien.HetHang => query.Where(l => l.TrangThai != TrangThaiLinhKien.NgungSuDung && l.SoLuongTon == 0),
            _ => query
        };
        const int pageSize = 10;
        var count = await query.CountAsync();
        var pages = (int)Math.Ceiling(count / (double)pageSize);
        trang = Math.Clamp(trang, 1, Math.Max(1, pages));
        var sorted = sapXep switch
        {
            "ten_desc" => query.OrderByDescending(l => l.TenLinhKien),
            "ton_asc" => query.OrderBy(l => l.SoLuongTon),
            "ton_desc" => query.OrderByDescending(l => l.SoLuongTon),
            "gia_asc" => query.OrderBy(l => l.DonGia),
            "gia_desc" => query.OrderByDescending(l => l.DonGia),
            _ => query.OrderBy(l => l.TenLinhKien)
        };
        return View(new LinhKienIndexViewModel
        {
            DanhSach = await sorted.ThenBy(l => l.MaLinhKien).Skip((trang - 1) * pageSize).Take(pageSize).ToListAsync(),
            DanhSachLoai = await context.LinhKiens.AsNoTracking().Select(l => l.LoaiLinhKien).Distinct().OrderBy(l => l).ToListAsync(),
            TuKhoa = tuKhoa, Loai = loai, TrangThai = trangThai, SapXep = sapXep,
            Trang = trang, TongSoTrang = pages, TongSoBanGhi = count
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await context.LinhKiens.AsNoTracking().FirstOrDefaultAsync(l => l.MaLinhKien == id);
        if (item == null) return NotFound();
        ViewBag.SoLuotSuDung = await context.ChiTietSuaChuas.CountAsync(c => c.MaLinhKien == id);
        return View(item);
    }

    [HttpGet]
    public IActionResult Create() => View(new LinhKienCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LinhKienCreateViewModel model)
    {
        model.MaLinhKien = 0;
        await ValidateItem(model);
        if (!ModelState.IsValid) return View(model);
        var item = new LinhKien { SoLuongTon = model.SoLuongBanDau!.Value };
        ApplyItem(item, model);
        context.LinhKiens.Add(item);
        await context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm linh kiện mới vào kho.";
        return RedirectToAction(nameof(Details), new { id = item.MaLinhKien });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await context.LinhKiens.AsNoTracking().FirstOrDefaultAsync(l => l.MaLinhKien == id);
        if (item == null) return NotFound();
        return View(new LinhKienEditViewModel { MaLinhKien = id, TenLinhKien = item.TenLinhKien,
            LoaiLinhKien = item.LoaiLinhKien, DonGia = item.DonGia, DonViTinh = item.DonViTinh,
            NgungSuDung = item.TrangThai == TrangThaiLinhKien.NgungSuDung, SoLuongTonHienTai = item.SoLuongTon });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LinhKienEditViewModel model)
    {
        if (id != model.MaLinhKien) return NotFound();
        var item = await context.LinhKiens.FindAsync(id);
        if (item == null) return NotFound();
        model.SoLuongTonHienTai = item.SoLuongTon;
        await ValidateItem(model);
        if (!ModelState.IsValid) return View(model);
        ApplyItem(item, model);
        await context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật thông tin linh kiện. Đơn giá trên các phiếu cũ được giữ nguyên.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> NhapKho(int id)
    {
        var item = await context.LinhKiens.AsNoTracking().FirstOrDefaultAsync(l => l.MaLinhKien == id);
        if (item == null) return NotFound();
        if (item.TrangThai == TrangThaiLinhKien.NgungSuDung) return StockDisabled(id);
        return View(new NhapKhoLinhKienViewModel { MaLinhKien = id, TenLinhKien = item.TenLinhKien,
            DonViTinh = item.DonViTinh, SoLuongTonHienTai = item.SoLuongTon });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapKho(int id, NhapKhoLinhKienViewModel model)
    {
        if (id != model.MaLinhKien) return NotFound();
        var item = await context.LinhKiens.AsNoTracking().FirstOrDefaultAsync(l => l.MaLinhKien == id);
        if (item == null) return NotFound();
        if (item.TrangThai == TrangThaiLinhKien.NgungSuDung) return StockDisabled(id);
        model.TenLinhKien = item.TenLinhKien; model.DonViTinh = item.DonViTinh;
        model.SoLuongTonHienTai = item.SoLuongTon;
        if (model.SoLuongNhap > int.MaxValue - item.SoLuongTon)
            ModelState.AddModelError(nameof(model.SoLuongNhap), "Số lượng nhập vượt giới hạn tồn kho.");
        if (!ModelState.IsValid) return View(model);
        var quantity = model.SoLuongNhap!.Value;
        // Cộng ngay tại Database để hai lần nhập đồng thời không ghi đè nhau.
        var updated = await context.LinhKiens.Where(l => l.MaLinhKien == id
            && l.TrangThai != TrangThaiLinhKien.NgungSuDung && l.SoLuongTon <= int.MaxValue - quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.SoLuongTon, l => l.SoLuongTon + quantity)
                .SetProperty(l => l.TrangThai, TrangThaiLinhKien.ConHang));
        if (updated == 0)
        {
            TempData["Error"] = "Tồn kho hoặc trạng thái vừa thay đổi. Vui lòng kiểm tra lại trước khi nhập.";
            return RedirectToAction(nameof(NhapKho), new { id });
        }
        TempData["Success"] = $"Đã nhập thêm {quantity:N0} {item.DonViTinh}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await context.LinhKiens.AsNoTracking().FirstOrDefaultAsync(l => l.MaLinhKien == id);
        if (item == null) return NotFound();
        ViewBag.CoLichSu = await context.ChiTietSuaChuas.AnyAsync(c => c.MaLinhKien == id);
        return View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await context.LinhKiens.FindAsync(id);
        if (item == null) return NotFound();
        if (await context.ChiTietSuaChuas.AnyAsync(c => c.MaLinhKien == id))
        {
            TempData["Error"] = "Không thể xóa linh kiện đã được sử dụng trên phiếu. Hãy chọn ngừng sử dụng.";
            return RedirectToAction(nameof(Details), new { id });
        }
        context.LinhKiens.Remove(item);
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Linh kiện đã phát sinh dữ liệu liên quan nên chưa thể xóa.";
            return RedirectToAction(nameof(Details), new { id });
        }
        TempData["Success"] = "Đã xóa linh kiện chưa phát sinh lịch sử.";
        return RedirectToAction(nameof(Index));
    }

    private IActionResult StockDisabled(int id)
    {
        TempData["Error"] = "Linh kiện đang ngừng sử dụng. Hãy mở lại trong phần chỉnh sửa trước khi nhập kho.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task ValidateItem(LinhKienEditViewModel model)
    {
        model.TenLinhKien = model.TenLinhKien?.Trim() ?? "";
        model.LoaiLinhKien = model.LoaiLinhKien?.Trim() ?? "";
        model.DonViTinh = model.DonViTinh?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(model.TenLinhKien)) ModelState.AddModelError(nameof(model.TenLinhKien), "Vui lòng nhập tên linh kiện.");
        if (string.IsNullOrWhiteSpace(model.LoaiLinhKien)) ModelState.AddModelError(nameof(model.LoaiLinhKien), "Vui lòng nhập loại linh kiện.");
        if (string.IsNullOrWhiteSpace(model.DonViTinh)) ModelState.AddModelError(nameof(model.DonViTinh), "Vui lòng nhập đơn vị tính.");
        if (model.DonGia.HasValue && decimal.Round(model.DonGia.Value, 2) != model.DonGia.Value)
            ModelState.AddModelError(nameof(model.DonGia), "Đơn giá có tối đa 2 chữ số thập phân.");
        if (await context.LinhKiens.AnyAsync(l => l.TenLinhKien == model.TenLinhKien
            && l.LoaiLinhKien == model.LoaiLinhKien && l.MaLinhKien != model.MaLinhKien))
            ModelState.AddModelError(nameof(model.TenLinhKien), "Tên và loại linh kiện đã tồn tại trong kho.");
    }

    private static void ApplyItem(LinhKien item, LinhKienEditViewModel model)
    {
        item.TenLinhKien = model.TenLinhKien; item.LoaiLinhKien = model.LoaiLinhKien;
        item.DonGia = model.DonGia!.Value; item.DonViTinh = model.DonViTinh;
        item.TrangThai = TrangThaiLinhKien.TheoTonKho(item.SoLuongTon, model.NgungSuDung);
    }
}
