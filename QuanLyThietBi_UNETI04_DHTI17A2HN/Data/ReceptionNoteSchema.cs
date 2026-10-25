// Họ và tên: Đinh Thị Thùy Linh
// Mã sinh viên: 23103100110
// Nội dung thực hiện: Bổ sung cột ghi chú tiếp nhận cho Database cũ dùng EnsureCreated.
using Microsoft.EntityFrameworkCore;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Data;

public static class ReceptionNoteSchema
{
    public static void EnsureColumns(ApplicationDbContext context)
        => context.Database.ExecuteSqlRaw("""
            IF COL_LENGTH(N'dbo.PhieuSuaChua', N'GhiChuTiepNhan') IS NULL
                ALTER TABLE [dbo].[PhieuSuaChua] ADD [GhiChuTiepNhan] nvarchar(500) NULL;
            """);
}
