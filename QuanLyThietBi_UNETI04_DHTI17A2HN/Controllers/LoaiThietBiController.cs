// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Quản lý loại thiết bị - CRUD, kiểm tra trùng tên và ràng buộc xóa.
// Áp dụng Repository Pattern (ILoaiThietBiRepository) để tách biệt logic truy vấn CSDL khỏi Controller.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class LoaiThietBiController : BaseController
    {
        private readonly ILoaiThietBiRepository _repository;

        public LoaiThietBiController(ILoaiThietBiRepository repository)
        {
            _repository = repository;
        }

        // GET: /LoaiThietBi
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5;

            // Gọi Repository lấy toàn bộ danh sách (đã bao gồm logic tìm kiếm)
            var allItems = await _repository.GetAllAsync(searchString);

            // Phân trang bằng LINQ
            var totalItems = allItems.Count();
            var items = allItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;

            return View(items);
        }

        // GET: /LoaiThietBi/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var loai = await _repository.GetByIdWithThietBiAsync(id);
            if (loai == null) return NotFound();
            return View(loai);
        }

        // GET: /LoaiThietBi/Create
        public IActionResult Create() => View();

        // POST: /LoaiThietBi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiThietBi loai)
        {
            // Kiểm tra trùng tên loại thiết bị qua Repository
            if (await _repository.CheckTenLoaiExistsAsync(loai.TenLoai))
            {
                ModelState.AddModelError("TenLoai", "Tên loại thiết bị đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(loai);
                await _repository.SaveChangesAsync();
                TempData["Success"] = "Thêm loại thiết bị thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(loai);
        }

        // GET: /LoaiThietBi/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var loai = await _repository.GetByIdAsync(id);
            if (loai == null) return NotFound();
            return View(loai);
        }

        // POST: /LoaiThietBi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LoaiThietBi loai)
        {
            if (id != loai.MaLoai) return NotFound();

            // Kiểm tra trùng tên (trừ chính nó) qua Repository
            if (await _repository.CheckTenLoaiExistsAsync(loai.TenLoai, id))
            {
                ModelState.AddModelError("TenLoai", "Tên loại thiết bị đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _repository.Update(loai);
                    await _repository.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật loại thiết bị thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _repository.GetByIdAsync(id) == null) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(loai);
        }

        // GET: /LoaiThietBi/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var loai = await _repository.GetByIdWithThietBiAsync(id);
            if (loai == null) return NotFound();
            return View(loai);
        }

        // POST: /LoaiThietBi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Tải loại thiết bị kèm danh sách Thiết bị (để kiểm tra ràng buộc xóa)
            var loai = await _repository.GetByIdWithThietBiAsync(id);
            if (loai == null) return NotFound();

            // Không xóa nếu đã có thiết bị tham chiếu
            if (loai.ThietBis.Any())
            {
                TempData["Error"] = $"Không thể xóa loại '{loai.TenLoai}' vì đang có {loai.ThietBis.Count} thiết bị sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            _repository.Delete(loai);
            await _repository.SaveChangesAsync();
            TempData["Success"] = "Xóa loại thiết bị thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}