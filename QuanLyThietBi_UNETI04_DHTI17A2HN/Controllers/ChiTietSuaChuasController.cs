using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Interfaces;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

public class ChiTietSuaChuasController : BaseController
{
    private readonly IChiTietSuaChuaRepository _repository;

    public ChiTietSuaChuasController(
        IChiTietSuaChuaRepository repository)
    {
        _repository = repository;
    }

    // GET: ChiTietSuaChuas
    public async Task<IActionResult> Index()
    {
        var data = await _repository.GetAllAsync();
        return View(data);
    }

    // GET: ChiTietSuaChuas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var chiTiet = await _repository.GetByIdWithDetailsAsync(id.Value);

        if (chiTiet == null)
            return NotFound();

        return View(chiTiet);
    }

    // GET: ChiTietSuaChuas/Create
    public async Task<IActionResult> Create()
    {
        await LoadDropdown();
        return View();
    }

    // POST: ChiTietSuaChuas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MaPhieu,MaKyThuatVien,MaLinhKien,SoLuong,KetQuaChanDoan,GhiChu")]
        ChiTietSuaChua chiTiet)
    {
        var phieu = await _repository.GetPhieuByIdAsync(chiTiet.MaPhieu);
        var linhKien = await _repository.GetLinhKienByIdAsync(chiTiet.MaLinhKien);
        var kyThuatVien = await _repository.GetTaiKhoanByIdAsync(
            chiTiet.MaKyThuatVien);

        if (phieu == null)
            ModelState.AddModelError("MaPhieu", "Phiếu sửa chữa không tồn tại.");
        else if (phieu.TrangThai != TrangThaiPhieu.DangXuLy)
            ModelState.AddModelError("MaPhieu", "Chỉ thay đổi linh kiện khi phiếu đang xử lý.");

        if (linhKien == null)
            ModelState.AddModelError("MaLinhKien", "Linh kiện không tồn tại.");
        else if (linhKien.TrangThai == TrangThaiLinhKien.NgungSuDung)
            ModelState.AddModelError("MaLinhKien", "Linh kiện này đã ngừng sử dụng.");

        if (kyThuatVien == null ||
            (kyThuatVien.VaiTro != VaiTroConstant.Admin &&
             kyThuatVien.VaiTro != VaiTroConstant.KyThuatVien))
        {
            ModelState.AddModelError(
                "MaKyThuatVien", "Kỹ thuật viên không hợp lệ.");
        }

        if (chiTiet.SoLuong <= 0)
            ModelState.AddModelError("SoLuong", "Số lượng phải lớn hơn 0.");

        if (linhKien != null && chiTiet.SoLuong > linhKien.SoLuongTon)
        {
            ModelState.AddModelError(
                "SoLuong",
                $"Kho chỉ còn {linhKien.SoLuongTon} {linhKien.DonViTinh}.");
        }

        if (ModelState.IsValid && phieu != null && linhKien != null)
        {
            chiTiet.DonGia = linhKien.DonGia;
            chiTiet.ThanhTien = chiTiet.SoLuong * chiTiet.DonGia;

            // Cập nhật kho và chi tiết cùng một lần lưu. Chi phí chỉ chốt khi hoàn thành.
            linhKien.SoLuongTon -= chiTiet.SoLuong;
            linhKien.TrangThai = TrangThaiLinhKien.TheoTonKho(linhKien.SoLuongTon, false);
            phieu.NgayCapNhat = DateTime.Now;

            await _repository.AddAsync(chiTiet);
            await _repository.SaveChangesAsync();

            TempData["Success"] = "Thêm chi tiết sửa chữa thành công.";
            return RedirectToAction(nameof(Index));
        }

        await LoadDropdown(
            chiTiet.MaPhieu,
            chiTiet.MaKyThuatVien,
            chiTiet.MaLinhKien);

        return View(chiTiet);
    }

    // GET: ChiTietSuaChuas/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var chiTiet = await _repository.GetByIdAsync(id.Value);

        if (chiTiet == null)
            return NotFound();

        var phieu = await _repository.GetPhieuByIdAsync(chiTiet.MaPhieu);
        if (phieu?.TrangThai != TrangThaiPhieu.DangXuLy)
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

    // POST: ChiTietSuaChuas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("MaChiTiet,MaPhieu,MaKyThuatVien,MaLinhKien,SoLuong,KetQuaChanDoan,GhiChu")]
        ChiTietSuaChua model)
    {
        if (id != model.MaChiTiet)
            return NotFound();

        var oldData = await _repository.GetByIdAsync(id);

        if (oldData == null)
            return NotFound();

        var phieuCu = await _repository.GetPhieuByIdAsync(oldData.MaPhieu);
        var phieuMoi = await _repository.GetPhieuByIdAsync(model.MaPhieu);
        if (phieuCu?.TrangThai != TrangThaiPhieu.DangXuLy || phieuMoi?.TrangThai != TrangThaiPhieu.DangXuLy)
        {
            TempData["Error"] = "Chỉ thay đổi linh kiện giữa các phiếu đang xử lý. Phiếu đã nghiệm thu được giữ nguyên chi phí.";
            return RedirectToAction(nameof(Index));
        }
        var kyThuatVien = await _repository.GetTaiKhoanByIdAsync(
            model.MaKyThuatVien);
        var linhKienMoi = await _repository.GetLinhKienByIdAsync(
            model.MaLinhKien);
        var linhKienCu = await _repository.GetLinhKienByIdAsync(
            oldData.MaLinhKien);

        if (phieuMoi == null)
            ModelState.AddModelError("MaPhieu", "Phiếu sửa chữa không tồn tại.");

        if (kyThuatVien == null ||
            (kyThuatVien.VaiTro != VaiTroConstant.Admin &&
             kyThuatVien.VaiTro != VaiTroConstant.KyThuatVien))
        {
            ModelState.AddModelError(
                "MaKyThuatVien", "Kỹ thuật viên không hợp lệ.");
        }

        if (linhKienMoi == null)
            ModelState.AddModelError("MaLinhKien", "Linh kiện không tồn tại.");
        else if (linhKienMoi.TrangThai == TrangThaiLinhKien.NgungSuDung)
            ModelState.AddModelError("MaLinhKien", "Linh kiện này đã ngừng sử dụng.");

        if (linhKienCu == null)
            ModelState.AddModelError("MaLinhKien", "Không tìm thấy linh kiện cũ để hoàn kho.");

        if (model.SoLuong <= 0)
            ModelState.AddModelError("SoLuong", "Số lượng phải lớn hơn 0.");

        // Nếu vẫn dùng linh kiện cũ thì được cộng lại số lượng cũ
        // để tính số lượng thực tế có thể sử dụng.
        if (linhKienMoi != null)
        {
            long tonKhoKhaDung = linhKienMoi.SoLuongTon;

            if (linhKienCu != null &&
                linhKienCu.MaLinhKien == linhKienMoi.MaLinhKien)
            {
                tonKhoKhaDung += oldData.SoLuong;
            }

            if (model.SoLuong > tonKhoKhaDung)
            {
                ModelState.AddModelError(
                    "SoLuong",
                    $"Kho chỉ còn {tonKhoKhaDung} {linhKienMoi.DonViTinh}.");
            }
            if (tonKhoKhaDung - model.SoLuong > int.MaxValue)
                ModelState.AddModelError("SoLuong", "Điều chỉnh số lượng vượt giới hạn tồn kho.");
        }

        if (linhKienCu != null && oldData.MaLinhKien != model.MaLinhKien
            && oldData.SoLuong > int.MaxValue - linhKienCu.SoLuongTon)
            ModelState.AddModelError("MaLinhKien", "Hoàn trả linh kiện cũ vượt giới hạn tồn kho.");

        if (ModelState.IsValid &&
            phieuMoi != null &&
            linhKienMoi != null &&
            linhKienCu != null)
        {
            // Hoàn kho cũ, sau đó trừ kho mới.
            if (oldData.MaLinhKien == model.MaLinhKien)
                linhKienMoi.SoLuongTon = (int)((long)linhKienMoi.SoLuongTon + oldData.SoLuong - model.SoLuong);
            else
            {
                linhKienCu.SoLuongTon += oldData.SoLuong;
                linhKienCu.TrangThai = TrangThaiLinhKien.TheoTonKho(linhKienCu.SoLuongTon,
                    linhKienCu.TrangThai == TrangThaiLinhKien.NgungSuDung);
                linhKienMoi.SoLuongTon -= model.SoLuong;
            }
            linhKienMoi.TrangThai = TrangThaiLinhKien.TheoTonKho(linhKienMoi.SoLuongTon, false);

            // Giữ đơn giá đã ghi nhận khi vẫn sử dụng cùng linh kiện.
            model.DonGia = oldData.MaLinhKien == model.MaLinhKien ? oldData.DonGia : linhKienMoi.DonGia;
            model.ThanhTien = model.SoLuong * model.DonGia;

            oldData.MaPhieu = model.MaPhieu;
            oldData.MaKyThuatVien = model.MaKyThuatVien;
            oldData.MaLinhKien = model.MaLinhKien;
            oldData.SoLuong = model.SoLuong;
            oldData.DonGia = model.DonGia;
            oldData.ThanhTien = model.ThanhTien;
            oldData.KetQuaChanDoan = model.KetQuaChanDoan;
            oldData.GhiChu = model.GhiChu;

            phieuCu.NgayCapNhat = DateTime.Now;
            phieuMoi.NgayCapNhat = DateTime.Now;

            await _repository.SaveChangesAsync();

            TempData["Success"] = "Cập nhật chi tiết sửa chữa thành công.";
            return RedirectToAction(nameof(Index));
        }

        await LoadDropdown(
            model.MaPhieu,
            model.MaKyThuatVien,
            model.MaLinhKien);

        return View(model);
    }

    // GET: ChiTietSuaChuas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var chiTiet = await _repository.GetByIdWithDetailsAsync(id.Value);

        if (chiTiet == null)
            return NotFound();

        if (chiTiet.PhieuSuaChua?.TrangThai != TrangThaiPhieu.DangXuLy)
        {
            TempData["Error"] = "Phiếu không ở trạng thái cho phép thay đổi linh kiện.";
            return RedirectToAction(nameof(Index));
        }

        return View(chiTiet);
    }

    // POST: ChiTietSuaChuas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var chiTiet = await _repository.GetByIdAsync(id);

        if (chiTiet == null)
            return NotFound();

        var phieu = await _repository.GetPhieuByIdAsync(chiTiet.MaPhieu);
        if (phieu?.TrangThai != TrangThaiPhieu.DangXuLy)
        {
            TempData["Error"] = "Không thể xóa linh kiện của phiếu ngoài trạng thái đang xử lý.";
            return RedirectToAction(nameof(Index));
        }

        var linhKien = await _repository.GetLinhKienByIdAsync(
            chiTiet.MaLinhKien);

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

        _repository.Delete(chiTiet);
        await _repository.SaveChangesAsync();

        TempData["Success"] = "Xóa chi tiết sửa chữa thành công.";
        return RedirectToAction(nameof(Index));
    }

    // Nạp dữ liệu cho các dropdown trên Create/Edit.
    private async Task LoadDropdown(
        int? maPhieu = null,
        int? maKyThuatVien = null,
        int? maLinhKien = null)
    {
        var phieuList = await _repository.GetAllPhieuAsync();

        ViewBag.MaPhieu = new SelectList(
            phieuList, "MaPhieu", "MaPhieu", maPhieu);

        var kyThuatVienList = await _repository.GetKyThuatViensAsync();

        ViewBag.MaKyThuatVien = new SelectList(
            kyThuatVienList, "MaTaiKhoan", "HoTen", maKyThuatVien);

        var linhKienList = await _repository.GetLinhKienConHangAsync();

        // Khi Edit, giữ lại linh kiện đang chọn kể cả khi tồn kho đã về 0.
        if (maLinhKien.HasValue &&
            linhKienList.All(x => x.MaLinhKien != maLinhKien.Value))
        {
            var linhKienCu = await _repository.GetLinhKienByIdAsync(
                maLinhKien.Value);

            if (linhKienCu != null)
                linhKienList.Add(linhKienCu);
        }

        ViewBag.MaLinhKien = new SelectList(
            linhKienList, "MaLinhKien", "TenLinhKien", maLinhKien);
    }
}
