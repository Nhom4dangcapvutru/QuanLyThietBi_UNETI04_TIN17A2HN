using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Data;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Repositories;

public class LinhKienRepository(ApplicationDbContext context) : ILinhKienRepository
{
    public async Task<LinhKienIndexViewModel> SearchAsync(string? tuKhoa, string? loai,
        string? trangThai, string? sapXep, int trang)
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
        return new LinhKienIndexViewModel
        {
            DanhSach = await sorted.ThenBy(l => l.MaLinhKien).Skip((trang - 1) * pageSize).Take(pageSize).ToListAsync(),
            DanhSachLoai = await context.LinhKiens.AsNoTracking().Select(l => l.LoaiLinhKien).Distinct().OrderBy(l => l).ToListAsync(),
            TuKhoa = tuKhoa, Loai = loai, TrangThai = trangThai, SapXep = sapXep,
            Trang = trang, TongSoTrang = pages, TongSoBanGhi = count
        };
    }

    public Task<LinhKien?> GetByIdAsync(int id)
        => context.LinhKiens.AsNoTracking().FirstOrDefaultAsync(l => l.MaLinhKien == id);

    public async Task<LinhKien?> GetForUpdateAsync(int id)
        => await context.LinhKiens.FindAsync(id);

    public Task<int> CountUsageAsync(int id)
        => context.ChiTietSuaChuas.CountAsync(c => c.MaLinhKien == id);

    public Task<bool> HasHistoryAsync(int id)
        => context.ChiTietSuaChuas.AnyAsync(c => c.MaLinhKien == id);

    public Task<bool> NameAndTypeExistsAsync(string tenLinhKien, string loaiLinhKien, int excludedId)
        => context.LinhKiens.AnyAsync(l => l.TenLinhKien == tenLinhKien
            && l.LoaiLinhKien == loaiLinhKien && l.MaLinhKien != excludedId);

    public async Task AddAsync(LinhKien item)
    {
        context.LinhKiens.Add(item);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LinhKien item)
    {
        context.LinhKiens.Update(item);
        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(LinhKien item)
    {
        context.LinhKiens.Remove(item);
        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> NhapKhoAsync(int id, int quantity)
    {
        if (quantity <= 0) return false;
        // Cộng tại database để hai request nhập kho không ghi đè số lượng nhau.
        var updated = await context.LinhKiens.Where(l => l.MaLinhKien == id
            && l.TrangThai != TrangThaiLinhKien.NgungSuDung && l.SoLuongTon <= int.MaxValue - quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.SoLuongTon, l => l.SoLuongTon + quantity)
                .SetProperty(l => l.TrangThai, TrangThaiLinhKien.ConHang));
        return updated > 0;
    }
}
