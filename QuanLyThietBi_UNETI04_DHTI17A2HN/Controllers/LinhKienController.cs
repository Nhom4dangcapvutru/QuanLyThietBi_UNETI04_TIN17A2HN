using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers;

// Kho linh kiện: Admin quản lý; kỹ thuật viên chỉ xem danh mục và tồn kho.
public class LinhKienController(ILinhKienRepository repository) : BaseController
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
        return View(await repository.SearchAsync(tuKhoa, loai, trangThai, sapXep, trang));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await repository.GetByIdAsync(id);
        if (item == null) return NotFound();
        ViewBag.SoLuotSuDung = await repository.CountUsageAsync(id);
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
        await repository.AddAsync(item);
        TempData["Success"] = "Đã thêm linh kiện mới vào kho.";
        return RedirectToAction(nameof(Details), new { id = item.MaLinhKien });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await repository.GetByIdAsync(id);
        if (item == null) return NotFound();
        return View(new LinhKienEditViewModel { MaLinhKien = id, TenLinhKien = item.TenLinhKien,
            LoaiLinhKien = item.LoaiLinhKien, DonGia = item.DonGia, DonViTinh = item.DonViTinh,
            NgungSuDung = item.TrangThai == TrangThaiLinhKien.NgungSuDung, SoLuongTonHienTai = item.SoLuongTon });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LinhKienEditViewModel model)
    {
        if (id != model.MaLinhKien) return NotFound();
        var item = await repository.GetForUpdateAsync(id);
        if (item == null) return NotFound();
        model.SoLuongTonHienTai = item.SoLuongTon;
        await ValidateItem(model);
        if (!ModelState.IsValid) return View(model);
        ApplyItem(item, model);
        await repository.UpdateAsync(item);
        TempData["Success"] = "Đã cập nhật thông tin linh kiện. Đơn giá trên các phiếu cũ được giữ nguyên.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> NhapKho(int id)
    {
        var item = await repository.GetByIdAsync(id);
        if (item == null) return NotFound();
        if (item.TrangThai == TrangThaiLinhKien.NgungSuDung) return StockDisabled(id);
        return View(new NhapKhoLinhKienViewModel { MaLinhKien = id, TenLinhKien = item.TenLinhKien,
            DonViTinh = item.DonViTinh, SoLuongTonHienTai = item.SoLuongTon });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapKho(int id, NhapKhoLinhKienViewModel model)
    {
        if (id != model.MaLinhKien) return NotFound();
        var item = await repository.GetByIdAsync(id);
        if (item == null) return NotFound();
        if (item.TrangThai == TrangThaiLinhKien.NgungSuDung) return StockDisabled(id);
        model.TenLinhKien = item.TenLinhKien; model.DonViTinh = item.DonViTinh;
        model.SoLuongTonHienTai = item.SoLuongTon;
        if (model.SoLuongNhap > int.MaxValue - item.SoLuongTon)
            ModelState.AddModelError(nameof(model.SoLuongNhap), "Số lượng nhập vượt giới hạn tồn kho.");
        if (!ModelState.IsValid) return View(model);
        var quantity = model.SoLuongNhap!.Value;
        if (!await repository.NhapKhoAsync(id, quantity))
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
        var item = await repository.GetByIdAsync(id);
        if (item == null) return NotFound();
        ViewBag.CoLichSu = await repository.HasHistoryAsync(id);
        return View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await repository.GetForUpdateAsync(id);
        if (item == null) return NotFound();
        if (await repository.HasHistoryAsync(id))
        {
            TempData["Error"] = "Không thể xóa linh kiện đã được sử dụng trên phiếu. Hãy chọn ngừng sử dụng.";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (!await repository.DeleteAsync(item))
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
        if (await repository.NameAndTypeExistsAsync(model.TenLinhKien, model.LoaiLinhKien, model.MaLinhKien))
            ModelState.AddModelError(nameof(model.TenLinhKien), "Tên và loại linh kiện đã tồn tại trong kho.");
    }

    private static void ApplyItem(LinhKien item, LinhKienEditViewModel model)
    {
        item.TenLinhKien = model.TenLinhKien; item.LoaiLinhKien = model.LoaiLinhKien;
        item.DonGia = model.DonGia!.Value; item.DonViTinh = model.DonViTinh;
        item.TrangThai = TrangThaiLinhKien.TheoTonKho(item.SoLuongTon, model.NgungSuDung);
    }
}
