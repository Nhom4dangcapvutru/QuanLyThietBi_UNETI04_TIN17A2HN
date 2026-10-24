using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.ViewModels;

public class LinhKienEditViewModel
{
    public int MaLinhKien { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên linh kiện"), StringLength(150)]
    [Display(Name = "Tên linh kiện")]
    public string TenLinhKien { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập loại linh kiện"), StringLength(100)]
    [Display(Name = "Loại linh kiện")]
    public string LoaiLinhKien { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập đơn giá")]
    [Range(typeof(decimal), "0", "1000000000000", ErrorMessage = "Đơn giá phải từ 0 đến 1.000.000.000.000 VNĐ")]
    [Display(Name = "Đơn giá (VNĐ)")]
    public decimal? DonGia { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập đơn vị tính"), StringLength(30)]
    [Display(Name = "Đơn vị tính")]
    public string DonViTinh { get; set; } = "Cái";

    [Display(Name = "Ngừng sử dụng linh kiện này")]
    public bool NgungSuDung { get; set; }

    [BindNever, ValidateNever]
    public int SoLuongTonHienTai { get; set; }
}

public class LinhKienCreateViewModel : LinhKienEditViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tồn kho ban đầu")]
    [Range(0, int.MaxValue, ErrorMessage = "Tồn kho ban đầu không được âm")]
    [Display(Name = "Số lượng tồn ban đầu")]
    public int? SoLuongBanDau { get; set; } = 0;
}

public class NhapKhoLinhKienViewModel
{
    public int MaLinhKien { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số lượng nhập")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0")]
    [Display(Name = "Số lượng nhập bổ sung")]
    public int? SoLuongNhap { get; set; }

    [BindNever, ValidateNever]
    public string TenLinhKien { get; set; } = "";
    [BindNever, ValidateNever]
    public string DonViTinh { get; set; } = "";
    [BindNever, ValidateNever]
    public int SoLuongTonHienTai { get; set; }
}

public class LinhKienIndexViewModel
{
    public List<LinhKien> DanhSach { get; set; } = new();
    public List<string> DanhSachLoai { get; set; } = new();
    public string? TuKhoa { get; set; }
    public string? Loai { get; set; }
    public string? TrangThai { get; set; }
    public string? SapXep { get; set; }
    public int Trang { get; set; } = 1;
    public int TongSoTrang { get; set; }
    public int TongSoBanGhi { get; set; }
}
