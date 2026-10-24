using Microsoft.EntityFrameworkCore;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Data;

// Tương thích Database cũ đã được tạo bằng EnsureCreated, không có lịch sử Migration.
// Chỉ thêm cột nullable; tuyệt đối không suy diễn hoặc cập nhật tổng tiền của phiếu cũ.
public static class RepairCostSchema
{
    public const string AddColumnsSql = """
        IF COL_LENGTH(N'dbo.PhieuSuaChua', N'TienLinhKienChot') IS NULL
            ALTER TABLE [dbo].[PhieuSuaChua] ADD [TienLinhKienChot] decimal(18,2) NULL;
        IF COL_LENGTH(N'dbo.PhieuSuaChua', N'TienCong') IS NULL
            ALTER TABLE [dbo].[PhieuSuaChua] ADD [TienCong] decimal(18,2) NULL;
        IF COL_LENGTH(N'dbo.PhieuSuaChua', N'ChiPhiKhac') IS NULL
            ALTER TABLE [dbo].[PhieuSuaChua] ADD [ChiPhiKhac] decimal(18,2) NULL;
        IF COL_LENGTH(N'dbo.PhieuSuaChua', N'NoiDungChiPhiKhac') IS NULL
            ALTER TABLE [dbo].[PhieuSuaChua] ADD [NoiDungChiPhiKhac] nvarchar(500) NULL;
        IF COL_LENGTH(N'dbo.PhieuSuaChua', N'GiamTru') IS NULL
            ALTER TABLE [dbo].[PhieuSuaChua] ADD [GiamTru] decimal(18,2) NULL;
        IF COL_LENGTH(N'dbo.PhieuSuaChua', N'LyDoGiamTru') IS NULL
            ALTER TABLE [dbo].[PhieuSuaChua] ADD [LyDoGiamTru] nvarchar(500) NULL;
        """;

    public static void EnsureColumns(ApplicationDbContext context)
        => context.Database.ExecuteSqlRaw(AddColumnsSql);
}
