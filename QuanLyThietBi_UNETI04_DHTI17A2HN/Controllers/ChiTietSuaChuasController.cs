using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Interfaces;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

public class ChiTietSuaChuasController : Controller
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

        if (linhKien == null)
            ModelState.AddModelError("MaLinhKien", "Linh kiện không tồn tại.");

        if (kyThuatVien == null ||
            (kyThuatVien.VaiTro != "NhanVien" &&
             kyThuatVien.VaiTro != "KyThuatVien"))
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

            // Cập nhật kho và tổng chi phí cùng một lần SaveChanges.
            linhKien.SoLuongTon -= chiTiet.SoLuong;
            phieu.TongChiPhi =
                (phieu.TongChiPhi ?? 0m) + chiTiet.ThanhTien;
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

        var phieuMoi = await _repository.GetPhieuByIdAsync(model.MaPhieu);
        var kyThuatVien = await _repository.GetTaiKhoanByIdAsync(
            model.MaKyThuatVien);
        var linhKienMoi = await _repository.GetLinhKienByIdAsync(
            model.MaLinhKien);
        var linhKienCu = await _repository.GetLinhKienByIdAsync(
            oldData.MaLinhKien);

        if (phieuMoi == null)
            ModelState.AddModelError("MaPhieu", "Phiếu sửa chữa không tồn tại.");

        if (kyThuatVien == null ||
            (kyThuatVien.VaiTro != "NhanVien" &&
             kyThuatVien.VaiTro != "KyThuatVien"))
        {
            ModelState.AddModelError(
                "MaKyThuatVien", "Kỹ thuật viên không hợp lệ.");
        }

        if (linhKienMoi == null)
            ModelState.AddModelError("MaLinhKien", "Linh kiện không tồn tại.");

        if (model.SoLuong <= 0)
            ModelState.AddModelError("SoLuong", "Số lượng phải lớn hơn 0.");

        // Nếu vẫn dùng linh kiện cũ thì được cộng lại số lượng cũ
        // để tính số lượng thực tế có thể sử dụng.
        if (linhKienMoi != null)
        {
            int tonKhoKhaDung = linhKienMoi.SoLuongTon;

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
        }

        if (ModelState.IsValid &&
            phieuMoi != null &&
            linhKienMoi != null &&
            linhKienCu != null)
        {
            int maPhieuCu = oldData.MaPhieu;
            int maLinhKienCu = oldData.MaLinhKien;

            // Hoàn kho cũ, sau đó trừ kho mới.
            linhKienCu.SoLuongTon += oldData.SoLuong;
            linhKienMoi.SoLuongTon -= model.SoLuong;

            model.DonGia = linhKienMoi.DonGia;
            model.ThanhTien = model.SoLuong * model.DonGia;

            oldData.MaPhieu = model.MaPhieu;
            oldData.MaKyThuatVien = model.MaKyThuatVien;
            oldData.MaLinhKien = model.MaLinhKien;
            oldData.SoLuong = model.SoLuong;
            oldData.DonGia = model.DonGia;
            oldData.ThanhTien = model.ThanhTien;
            oldData.KetQuaChanDoan = model.KetQuaChanDoan;
            oldData.GhiChu = model.GhiChu;

            // Tính lại tổng tiền phiếu cũ và phiếu mới.
            decimal tongPhieuCu = await _repository.GetTongChiPhiAsync(
                maPhieuCu, oldData.MaChiTiet);

            if (maPhieuCu == model.MaPhieu)
            {
                phieuMoi.TongChiPhi = tongPhieuCu + model.ThanhTien;
                phieuMoi.NgayCapNhat = DateTime.Now;
            }
            else
            {
                var phieuCu = await _repository.GetPhieuByIdAsync(maPhieuCu);

                if (phieuCu != null)
                {
                    phieuCu.TongChiPhi = tongPhieuCu;
                    phieuCu.NgayCapNhat = DateTime.Now;
                }

                decimal tongPhieuMoi = await _repository.GetTongChiPhiAsync(
                    model.MaPhieu, oldData.MaChiTiet);

                phieuMoi.TongChiPhi = tongPhieuMoi + model.ThanhTien;
                phieuMoi.NgayCapNhat = DateTime.Now;
            }

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

        var linhKien = await _repository.GetLinhKienByIdAsync(
            chiTiet.MaLinhKien);
        var phieu = await _repository.GetPhieuByIdAsync(
            chiTiet.MaPhieu);

        if (linhKien != null)
            linhKien.SoLuongTon += chiTiet.SoLuong;

        if (phieu != null)
        {
            decimal tongConLai = await _repository.GetTongChiPhiAsync(
                chiTiet.MaPhieu, chiTiet.MaChiTiet);

            phieu.TongChiPhi = tongConLai;
            phieu.NgayCapNhat = DateTime.Now;
        }

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
