using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Migrations
{
    /// <inheritdoc />
    public partial class AddReceptionNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cột có thể đã được bổ sung khi khởi động Database cũ dùng EnsureCreated.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PhieuSuaChua', N'GhiChuTiepNhan') IS NULL
                    ALTER TABLE [dbo].[PhieuSuaChua] ADD [GhiChuTiepNhan] nvarchar(500) NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhiChuTiepNhan",
                table: "PhieuSuaChua");
        }
    }
}
