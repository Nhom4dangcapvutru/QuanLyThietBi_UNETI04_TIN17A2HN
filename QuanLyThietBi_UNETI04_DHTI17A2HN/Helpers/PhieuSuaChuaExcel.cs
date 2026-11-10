using ClosedXML.Excel;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;

internal static class PhieuSuaChuaExcel
{
    public static byte[] Create(PhieuSuaChua phieu)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Phiếu sửa chữa");
        sheet.Column(1).Width = 36;
        sheet.Column(2).Width = 15;
        sheet.Column(3).Width = 18;
        sheet.Column(4).Width = 20;
        sheet.Column(5).Width = 46;
        sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var row = 1;
        Section($"KẾT QUẢ SỬA CHỮA — PHIẾU #{phieu.MaPhieu}");
        Text("Khách hàng", phieu.KhachHang?.HoTen);
        Text("Thiết bị", phieu.ThietBi?.TenThietBi);
        Text("Số serial", phieu.ThietBi?.SerialNumber);
        Text("Trạng thái", phieu.TrangThai);
        Date("Ngày tiếp nhận trên phiếu", phieu.NgayTiepNhan);
        Date("Ngày hoàn thành", phieu.NgayHoanThanh);
        Text("Lỗi ban đầu", phieu.NoiDungLoi);
        Text("Ghi chú tiếp nhận", phieu.GhiChuTiepNhan);
        Text("Kết quả chẩn đoán", phieu.KetQuaChanDoan);
        row++;

        Section("LINH KIỆN ĐÃ SỬ DỤNG");
        string[] headers = ["Linh kiện", "Số lượng", "Đơn giá (VNĐ)", "Thành tiền (VNĐ)", "Kết quả / Ghi chú"];
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(row, i + 1).Value = headers[i];
        sheet.Range(row, 1, row, 5).Style.Font.Bold = true;
        row++;

        foreach (var chiTiet in phieu.ChiTietSuaChuas.OrderBy(c => c.MaChiTiet))
        {
            sheet.Cell(row, 1).Value = chiTiet.LinhKien?.TenLinhKien ?? $"Linh kiện #{chiTiet.MaLinhKien}";
            sheet.Cell(row, 2).Value = chiTiet.SoLuong;
            MoneyCell(sheet.Cell(row, 3), chiTiet.DonGia);
            MoneyCell(sheet.Cell(row, 4), chiTiet.ThanhTien);
            sheet.Cell(row, 5).Value = string.Join("\n",
                new[] { chiTiet.KetQuaChanDoan, chiTiet.GhiChu }
                    .Where(value => !string.IsNullOrWhiteSpace(value)).Distinct());
            sheet.Range(row, 1, row, 5).Style.Alignment.WrapText = true;
            sheet.Row(row).AdjustToContents();
            row++;
        }

        if (phieu.ChiTietSuaChuas.Count == 0)
            Text("Linh kiện", "Không có dòng linh kiện được ghi nhận.");
        row++;

        Section("CHI PHÍ SỬA CHỮA (VNĐ)");
        Money("Chi phí dự kiến (tham khảo)", phieu.ChiPhiDuKien);
        if (phieu.CoPhanTichChiPhi)
        {
            Money("Tiền linh kiện khi nghiệm thu", phieu.TienLinhKienChot);
            Money("Tiền công sửa chữa", phieu.TienCong);
            Money("Chi phí khác", phieu.ChiPhiKhac);
            if (phieu.ChiPhiKhac > 0)
                Text("Nội dung chi phí khác", phieu.NoiDungChiPhiKhac);
            Money("Giảm trừ", phieu.GiamTru);
            if (phieu.GiamTru > 0)
                Text("Lý do giảm trừ", phieu.LyDoGiamTru);
        }
        else
        {
            Money("Tiền linh kiện đã ghi nhận", phieu.ChiTietSuaChuas.Sum(c => c.ThanhTien));
            Text("Ghi chú chi phí", "Phiếu cũ chưa có phân tích chi phí đầy đủ.");
        }
        sheet.Range(row, 1, row, 5).Style.Font.Bold = true;
        Money("Tổng chi phí đã chốt", phieu.TongChiPhi);
        Text("Ghi chú", "Chi phí nghiệm thu của phiếu, chưa xác nhận tình trạng thanh toán.");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();

        void Section(string title)
        {
            var range = sheet.Range(row, 1, row, 5).Merge();
            range.FirstCell().Value = title;
            range.Style.Font.Bold = true;
            range.Style.Font.FontColor = XLColor.White;
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#215E49");
            sheet.Row(row++).Height = 25;
        }

        IXLCell Field(string label)
        {
            sheet.Cell(row, 1).Value = label;
            sheet.Cell(row, 1).Style.Font.Bold = true;
            var range = sheet.Range(row, 2, row, 5).Merge();
            row++;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            return range.FirstCell();
        }

        void Text(string label, string? value)
        {
            var cell = Field(label);
            var content = string.IsNullOrWhiteSpace(value) ? "Chưa ghi nhận" : value;
            cell.Value = content;
            cell.Style.Alignment.WrapText = true;
            // Excel không tự chỉnh chiều cao theo nội dung của ô gộp.
            var lines = content.Split('\n').Sum(line => Math.Max(1, (line.Length + 79) / 80));
            sheet.Row(row - 1).Height = Math.Min(400, Math.Max(18, lines * 16));
        }

        void Date(string label, DateTime? value)
        {
            if (!value.HasValue)
            {
                Text(label, null);
                return;
            }
            var cell = Field(label);
            cell.Value = value.Value;
            cell.Style.NumberFormat.Format = "dd/mm/yyyy hh:mm";
        }

        void Money(string label, decimal? value)
        {
            if (value.HasValue)
                MoneyCell(Field(label), value.Value);
            else
                Text(label, null);
        }
    }

    private static void MoneyCell(IXLCell cell, decimal value)
    {
        cell.Value = value;
        cell.Style.NumberFormat.Format = "#,##0.##";
    }
}
