// Họ và tên: Cả nhóm thống nhất thực hiện
// Mã sinh viên: Dùng chung 5 thành viên
// Nội dung thực hiện: DbContext Entity Framework Core 10 - Cấu hình quan hệ và ràng buộc CSDL

using Microsoft.EntityFrameworkCore;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;
using System.Reflection.Emit;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<LoaiThietBi> LoaiThietBis => Set<LoaiThietBi>();
        public DbSet<ThietBi> ThietBis => Set<ThietBi>();
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<PhieuSuaChua> PhieuSuaChuas => Set<PhieuSuaChua>();
        public DbSet<LinhKien> LinhKiens => Set<LinhKien>();
        public DbSet<ChiTietSuaChua> ChiTietSuaChuas => Set<ChiTietSuaChua>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Unique Indexes
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            modelBuilder.Entity<ThietBi>()
                .HasIndex(t => t.SerialNumber)
                .IsUnique();

            modelBuilder.Entity<LoaiThietBi>()
                .HasIndex(l => l.TenLoai)
                .IsUnique();

            // 2. Cấu hình quan hệ 1 - 1 hoặc 1 - 0..1 giữa TaiKhoan và KhachHang
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(k => k.MaTaiKhoan)
                .OnDelete(DeleteBehavior.SetNull);

            // 3. Quan hệ LoaiThietBi - ThietBi (1 - N)
            modelBuilder.Entity<ThietBi>()
                .HasOne(t => t.LoaiThietBi)
                .WithMany(l => l.ThietBis)
                .HasForeignKey(t => t.MaLoai)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Quan hệ KhachHang - ThietBi (1 - N)
            modelBuilder.Entity<ThietBi>()
                .HasOne(t => t.KhachHang)
                .WithMany(k => k.ThietBis)
                .HasForeignKey(t => t.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Quan hệ PhieuSuaChua - ThietBi & KhachHang (Tránh lỗi Multiple Cascade Paths)
            modelBuilder.Entity<PhieuSuaChua>()
                .HasOne(p => p.ThietBi)
                .WithMany(t => t.PhieuSuaChuas)
                .HasForeignKey(p => p.MaThietBi)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuSuaChua>()
                .HasOne(p => p.KhachHang)
                .WithMany(k => k.PhieuSuaChuas)
                .HasForeignKey(p => p.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Quan hệ ChiTietSuaChua với PhieuSuaChua, LinhKien, TaiKhoan (KTV)
            modelBuilder.Entity<ChiTietSuaChua>()
                .HasOne(c => c.PhieuSuaChua)
                .WithMany(p => p.ChiTietSuaChuas)
                .HasForeignKey(c => c.MaPhieu)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietSuaChua>()
                .HasOne(c => c.LinhKien)
                .WithMany(l => l.ChiTietSuaChuas)
                .HasForeignKey(c => c.MaLinhKien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietSuaChua>()
                .HasOne(c => c.KyThuatVien)
                .WithMany(t => t.ChiTietSuaChuas)
                .HasForeignKey(c => c.MaKyThuatVien)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}