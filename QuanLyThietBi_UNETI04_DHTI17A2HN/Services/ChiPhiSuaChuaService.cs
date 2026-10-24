using System.ComponentModel.DataAnnotations;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Services;

public static class ChiPhiSuaChuaService
{
    public static decimal TinhTong(decimal linhKien, decimal? tienCong, decimal? chiPhiKhac,
        string? noiDungChiPhiKhac, decimal? giamTru, string? lyDoGiamTru)
    {
        const decimal gioiHan = 1_000_000_000_000m;
        if (linhKien < 0 || linhKien > gioiHan)
            throw new ValidationException("Tiền linh kiện không hợp lệ. Vui lòng kiểm tra các hạng mục sửa chữa.");
        if (!tienCong.HasValue || !chiPhiKhac.HasValue || !giamTru.HasValue)
            throw new ValidationException("Nhập đầy đủ tiền công, chi phí khác và giảm trừ; nhập 0 nếu không phát sinh.");
        if (new[] { tienCong.Value, chiPhiKhac.Value, giamTru.Value }
            .Any(x => x < 0 || x > gioiHan || decimal.Round(x, 2) != x))
            throw new ValidationException("Các khoản chi phí phải không âm, tối đa 1.000.000.000.000 VNĐ và có tối đa 2 chữ số thập phân.");
        if (chiPhiKhac > 0 && string.IsNullOrWhiteSpace(noiDungChiPhiKhac))
            throw new ValidationException("Vui lòng giải thích khoản chi phí khác.");
        if (giamTru > 0 && string.IsNullOrWhiteSpace(lyDoGiamTru))
            throw new ValidationException("Vui lòng nhập lý do giảm trừ bảo hành / ưu đãi.");
        var truocGiam = linhKien + tienCong.Value + chiPhiKhac.Value;
        if (giamTru.Value > truocGiam)
            throw new ValidationException("Giảm trừ không được lớn hơn tổng linh kiện, tiền công và chi phí khác.");
        return truocGiam - giamTru.Value;
    }
}
