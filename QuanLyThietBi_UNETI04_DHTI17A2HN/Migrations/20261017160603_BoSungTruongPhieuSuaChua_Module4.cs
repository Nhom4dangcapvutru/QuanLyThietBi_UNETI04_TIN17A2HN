using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Migrations
{
    public partial class BoSungTruongPhieuSuaChua_Module4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'ChiPhiDuKien') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [ChiPhiDuKien] decimal(18,2) NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'KetQuaChanDoan') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [KetQuaChanDoan] nvarchar(1000) NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'LyDoHuy') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [LyDoHuy] nvarchar(500) NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'MaKyThuatVienTiepNhan') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [MaKyThuatVienTiepNhan] int NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'NgayCapNhat') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [NgayCapNhat] datetime2 NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'NgayHenTra') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [NgayHenTra] datetime2 NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'TongChiPhi') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [TongChiPhi] decimal(18,2) NULL;
                """);
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.PhieuSuaChua') AND name = N'IX_PhieuSuaChua_MaKyThuatVienTiepNhan')
                    CREATE INDEX [IX_PhieuSuaChua_MaKyThuatVienTiepNhan] ON [dbo].[PhieuSuaChua] ([MaKyThuatVienTiepNhan]);
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.PhieuSuaChua') AND name = N'FK_PhieuSuaChua_TaiKhoan_MaKyThuatVienTiepNhan')
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD CONSTRAINT [FK_PhieuSuaChua_TaiKhoan_MaKyThuatVienTiepNhan]
                    FOREIGN KEY ([MaKyThuatVienTiepNhan]) REFERENCES [dbo].[TaiKhoan] ([MaTaiKhoan]);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_PhieuSuaChua_TaiKhoan_MaKyThuatVienTiepNhan", table: "PhieuSuaChua");
            migrationBuilder.DropIndex(name: "IX_PhieuSuaChua_MaKyThuatVienTiepNhan", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "ChiPhiDuKien", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "KetQuaChanDoan", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "LyDoHuy", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "MaKyThuatVienTiepNhan", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "NgayCapNhat", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "NgayHenTra", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "TongChiPhi", table: "PhieuSuaChua");
        }
    }
}
