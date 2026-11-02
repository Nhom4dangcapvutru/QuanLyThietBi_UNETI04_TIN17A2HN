// Họ và tên: Bùi Thị Thu Trang
// Mã sinh viên: 23103100103
// Nội dung thực hiện: Quản lý tài khoản - CRUD, khóa/mở khóa, kiểm tra trùng tên đăng nhập.
// Áp dụng Repository Pattern (ITaiKhoanRepository) để tách biệt logic truy vấn CSDL khỏi Controller.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Controllers
{
    public class TaiKhoanController : BaseController
    {
        private readonly ITaiKhoanRepository _repository;

        public TaiKhoanController(ITaiKhoanRepository repository)
        {
            _repository = repository;
        }

        // GET: /TaiKhoan
        public async Task<IActionResult> Index(string searchString, string vaiTro, string trangThai, int page = 1)
        {
            int pageSize = 5;

            // Gọi Repository lấy toàn bộ danh sách (đã bao gồm logic tìm kiếm và lọc)
            var allItems = await _repository.GetAllAsync(searchString, vaiTro, trangThai);

            // Phân trang bằng LINQ to Objects (vì Repository trả về IEnumerable)
            var totalItems = allItems.Count();
            var items = allItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;
            ViewBag.VaiTro = vaiTro;
            ViewBag.TrangThai = trangThai;

            return View(items);
        }

        // GET: /TaiKhoan/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var taiKhoan = await _repository.GetByIdAsync(id);
            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        // GET: /TaiKhoan/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /TaiKhoan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoan taiKhoan)
        {
            // Kiểm tra trùng tên đăng nhập qua Repository
            if (await _repository.CheckTenDangNhapExistsAsync(taiKhoan.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
            }

            // Kiểm tra trùng email qua Repository
            if (await _repository.CheckEmailExistsAsync(taiKhoan.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(taiKhoan);
                await _repository.SaveChangesAsync();
                TempData["Success"] = "Thêm tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // GET: /TaiKhoan/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var taiKhoan = await _repository.GetByIdAsync(id);
            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        // POST: /TaiKhoan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.MaTaiKhoan) return NotFound();

            // Kiểm tra trùng tên đăng nhập (trừ chính nó)
            if (await _repository.CheckTenDangNhapExistsAsync(taiKhoan.TenDangNhap, id))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
            }

            // Kiểm tra trùng email (trừ chính nó)
            if (await _repository.CheckEmailExistsAsync(taiKhoan.Email, id))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _repository.Update(taiKhoan);
                    await _repository.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật tài khoản thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _repository.GetByIdAsync(id) == null) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // GET: /TaiKhoan/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var taiKhoan = await _repository.GetByIdAsync(id);
            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        // POST: /TaiKhoan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taiKhoan = await _repository.GetByIdAsync(id);
            if (taiKhoan == null) return NotFound();

            // Không xóa nếu đã phát sinh lịch sử (ChiTietSuaChuas đã được Include trong Repository)
            if (taiKhoan.ChiTietSuaChuas.Any())
            {
                TempData["Error"] = "Không thể xóa tài khoản đã có lịch sử sửa chữa. Hãy khóa thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu là khách hàng -> Repository sẽ tự động xử lý việc này
            _repository.Delete(taiKhoan);
            await _repository.SaveChangesAsync();
            TempData["Success"] = "Xóa tài khoản thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /TaiKhoan/ToggleStatus/5 - Khóa / Mở khóa
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var taiKhoan = await _repository.GetByIdAsync(id);
            if (taiKhoan == null) return NotFound();

            taiKhoan.TrangThai = taiKhoan.TrangThai == TrangThaiTaiKhoan.HoatDong
                ? TrangThaiTaiKhoan.BiKhoa
                : TrangThaiTaiKhoan.HoatDong;

            _repository.Update(taiKhoan);
            await _repository.SaveChangesAsync();

            TempData["Success"] = $"Đã {(taiKhoan.TrangThai == TrangThaiTaiKhoan.BiKhoa ? "khóa" : "mở khóa")} tài khoản {taiKhoan.TenDangNhap}.";
            return RedirectToAction(nameof(Index));
        }
    }
}