using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Migrations
{
    public partial class AddRepairCostBreakdown : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'ChiPhiKhac') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [ChiPhiKhac] decimal(18,2) NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'GiamTru') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [GiamTru] decimal(18,2) NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'LyDoGiamTru') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [LyDoGiamTru] nvarchar(500) NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'NoiDungChiPhiKhac') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [NoiDungChiPhiKhac] nvarchar(500) NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'TienCong') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [TienCong] decimal(18,2) NULL;
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'TienLinhKienChot') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [TienLinhKienChot] decimal(18,2) NULL;
                """);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(name: "ChiPhiKhac", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "GiamTru", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "LyDoGiamTru", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "NoiDungChiPhiKhac", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "TienCong", table: "PhieuSuaChua");
            migrationBuilder.DropColumn(name: "TienLinhKienChot", table: "PhieuSuaChua");
        }
    }
}
