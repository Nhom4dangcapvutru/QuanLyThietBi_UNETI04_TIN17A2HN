// Họ và tên: Cả nhóm thống nhất thực hiện
// Mã sinh viên: Dùng chung 5 thành viên
// Nội dung thực hiện: Tạo dữ liệu mẫu (Seed Data) chuẩn số lượng yêu cầu để kiểm thử 5 Module

using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Constants;
using QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Entities;

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Nếu đã có dữ liệu thì không seed lại
            if (context.TaiKhoans.Any()) return;

            // ==============================================================
            // 1. TÀI KHOẢN (2 Admin, 3 Kỹ thuật viên, 5 Khách hàng có tài khoản)
            // ==============================================================
            var taiKhoans = new List<TaiKhoan>
            {
                new() { TenDangNhap = "admin", MatKhau = "123456", HoTen = "Quản Trị Viên 1", Email = "admin1@uneti.edu.vn", VaiTro = VaiTroConstant.Admin, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "admin2", MatKhau = "123456", HoTen = "Quản Trị Viên 2", Email = "admin2@uneti.edu.vn", VaiTro = VaiTroConstant.Admin, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "ktv_nam", MatKhau = "123456", HoTen = "KTV Trần Văn Nam", Email = "namtv@uneti.edu.vn", VaiTro = VaiTroConstant.KyThuatVien, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "ktv_tuan", MatKhau = "123456", HoTen = "KTV Lê Anh Tuấn", Email = "tuanla@uneti.edu.vn", VaiTro = VaiTroConstant.KyThuatVien, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "ktv_long", MatKhau = "123456", HoTen = "KTV Hoàng Phi Long", Email = "longhp@uneti.edu.vn", VaiTro = VaiTroConstant.KyThuatVien, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "user_kh01", MatKhau = "123456", HoTen = "Nguyễn Văn An", Email = "an.nv@gmail.com", VaiTro = VaiTroConstant.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "user_kh02", MatKhau = "123456", HoTen = "Trần Thị Bình", Email = "binh.tt@gmail.com", VaiTro = VaiTroConstant.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "user_kh03", MatKhau = "123456", HoTen = "Lê Hoàng Cường", Email = "cuong.lh@gmail.com", VaiTro = VaiTroConstant.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "user_kh04", MatKhau = "123456", HoTen = "Phạm Hồng Dung", Email = "dung.ph@gmail.com", VaiTro = VaiTroConstant.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong },
                new() { TenDangNhap = "user_locked", MatKhau = "123456", HoTen = "Khách Hàng Khóa", Email = "locked@gmail.com", VaiTro = VaiTroConstant.KhachHang, TrangThai = TrangThaiTaiKhoan.BiKhoa }
            };
            context.TaiKhoans.AddRange(taiKhoans);
            context.SaveChanges();

            // ==============================================================
            // 2. LOẠI THIẾT BỊ (Đủ 5 loại chuẩn)
            // ==============================================================
            var loaiThietBis = new List<LoaiThietBi>
            {
                new() { TenLoai = "Laptop", HangSanXuat = "Dell / Asus / HP / Apple / Lenovo", MoTa = "Máy tính xách tay các hãng", ThoiHanBaoHanhMacDinh = 24 }, // MaLoai = 1
                new() { TenLoai = "Điện thoại thông minh", HangSanXuat = "Apple / Samsung", MoTa = "Smartphone cao cấp và tầm trung", ThoiHanBaoHanhMacDinh = 12 },   // MaLoai = 2
                new() { TenLoai = "Máy tính bảng", HangSanXuat = "Apple / Xiaomi", MoTa = "Tablet học tập và làm việc", ThoiHanBaoHanhMacDinh = 12 },               // MaLoai = 3
                new() { TenLoai = "Màn hình máy tính", HangSanXuat = "LG / Samsung", MoTa = "Màn hình đồ họa và văn phòng", ThoiHanBaoHanhMacDinh = 36 },           // MaLoai = 4
                new() { TenLoai = "Máy in văn phòng", HangSanXuat = "Canon / HP", MoTa = "Máy in laser và phun màu", ThoiHanBaoHanhMacDinh = 12 }                   // MaLoai = 5
            };
            context.LoaiThietBis.AddRange(loaiThietBis);
            context.SaveChanges();

            // ==============================================================
            // 3. KHÁCH HÀNG (Đủ 30 khách hàng)
            // ==============================================================
            var khachHangs = new List<KhachHang>();
            for (int i = 1; i <= 30; i++)
            {
                khachHangs.Add(new KhachHang
                {
                    MaTaiKhoan = i <= 4 ? taiKhoans[5 + i - 1].MaTaiKhoan : null,
                    HoTen = i switch
                    {
                        1 => "Nguyễn Văn An",
                        2 => "Trần Thị Bình",
                        3 => "Lê Hoàng Cường",
                        4 => "Phạm Hồng Dung",
                        _ => $"Khách Hàng Mẫu {i:D2}"
                    },
                    SoDienThoai = $"090{i:D7}",
                    Email = $"khachhang{i:D2}@gmail.com",
                    DiaChi = $"{i * 12} Phố Lĩnh Nam, Hoàng Mai, Hà Nội",
                    CCCD = $"001202{i:D6}",
                    NgayTao = DateTime.Now.AddDays(-i * 5),
                    LoaiKhachHang = (i % 5 == 0) ? LoaiKhachHangConstant.DoanhNghiep : LoaiKhachHangConstant.CaNhan,
                    TrangThai = "Hoạt động",
                    MaSoThue = (i % 5 == 0) ? $"010998877{i}" : null
                });
            }
            context.KhachHangs.AddRange(khachHangs);
            context.SaveChanges();

            // ==============================================================
            // 4. LINH KIỆN (10 loại linh kiện)
            // ==============================================================
            var linhKiens = new List<LinhKien>
            {
                new() { TenLinhKien = "Màn hình OLED iPhone 14", LoaiLinhKien = "Màn hình", DonGia = 3500000, SoLuongTon = 15, DonViTinh = "Cái", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "Pin Laptop Dell XPS 13", LoaiLinhKien = "Pin", DonGia = 1450000, SoLuongTon = 8, DonViTinh = "Cái", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "Bàn phím cơ Asus ROG", LoaiLinhKien = "Bàn phím", DonGia = 950000, SoLuongTon = 20, DonViTinh = "Chiếc", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "SSD NVMe 1TB Kingston", LoaiLinhKien = "Ổ cứng", DonGia = 1850000, SoLuongTon = 25, DonViTinh = "Chiếc", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "RAM DDR5 16GB Crucial", LoaiLinhKien = "RAM", DonGia = 1200000, SoLuongTon = 30, DonViTinh = "Thanh", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "Cụm quạt tản nhiệt HP Pavilion", LoaiLinhKien = "Tản nhiệt", DonGia = 450000, SoLuongTon = 12, DonViTinh = "Bộ", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "Nguồn máy in Canon 2900", LoaiLinhKien = "Nguồn", DonGia = 650000, SoLuongTon = 5, DonViTinh = "Cái", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "Cáp màn hình LG 27 Inch", LoaiLinhKien = "Dây cáp", DonGia = 250000, SoLuongTon = 40, DonViTinh = "Sợi", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "Vỏ mặt A Laptop Dell Inspiron", LoaiLinhKien = "Vỏ máy", DonGia = 750000, SoLuongTon = 4, DonViTinh = "Bộ", TrangThai = "Còn hàng" },
                new() { TenLinhKien = "Keo tản nhiệt cao cấp MX-4", LoaiLinhKien = "Phụ phẩm", DonGia = 150000, SoLuongTon = 50, DonViTinh = "Tuýp", TrangThai = "Còn hàng" }
            };
            context.LinhKiens.AddRange(linhKiens);
            context.SaveChanges();

            // ==============================================================
            // 5. THIẾT BỊ (ĐỦ 15 THIẾT BỊ GÁN CHÍNH XÁC THEO ĐÚNG LOẠI THIẾT BỊ)
            // ==============================================================
            // 1: Laptop | 2: Điện thoại thông minh | 3: Máy tính bảng | 4: Màn hình máy tính | 5: Máy in văn phòng
            var danhSachThietBiChuan = new[]
            {
                // Nhóm Laptop (LoaiId = 1)
                new { Name = "Dell XPS 13 9310", LoaiId = 1, Gia = 25000000m },
                new { Name = "MacBook Pro M2 2023", LoaiId = 1, Gia = 32000000m },
                new { Name = "Asus ROG Zephyrus G14", LoaiId = 1, Gia = 28000000m },
                new { Name = "HP Envy 15 x360", LoaiId = 1, Gia = 21000000m },
                new { Name = "Lenovo ThinkPad X1 Carbon", LoaiId = 1, Gia = 29000000m },
                new { Name = "Dell Latitude 5420", LoaiId = 1, Gia = 18000000m },
                new { Name = "Asus TUF Gaming F15", LoaiId = 1, Gia = 22000000m },

                // Nhóm Điện thoại thông minh (LoaiId = 2)
                new { Name = "iPhone 14 Pro Max 256GB", LoaiId = 2, Gia = 24000000m },
                new { Name = "Samsung Galaxy S23 Ultra", LoaiId = 2, Gia = 20000000m },

                // Nhóm Máy tính bảng (LoaiId = 3)
                new { Name = "iPad Pro 11 inch M2", LoaiId = 3, Gia = 19000000m },
                new { Name = "Xiaomi Pad 6 Pro", LoaiId = 3, Gia = 9500000m },

                // Nhóm Màn hình máy tính (LoaiId = 4)
                new { Name = "Màn hình LG 27UP850 4K", LoaiId = 4, Gia = 8500000m },
                new { Name = "Màn hình Samsung Odyssey G5", LoaiId = 4, Gia = 6500000m },

                // Nhóm Máy in văn phòng (LoaiId = 5)
                new { Name = "Máy in Canon LBP 2900", LoaiId = 5, Gia = 3800000m },
                new { Name = "Máy in HP LaserJet M404dn", LoaiId = 5, Gia = 5200000m }
            };

            var thietBis = new List<ThietBi>();
            for (int i = 0; i < danhSachThietBiChuan.Length; i++)
            {
                var item = danhSachThietBiChuan[i];
                int khId = (i % 10) + 1;
                bool conBaoHanh = (i % 2 == 0);
                DateTime ngayMua = conBaoHanh ? DateTime.Now.AddMonths(-6) : DateTime.Now.AddMonths(-30);
                DateTime hanBaoHanh = conBaoHanh ? DateTime.Now.AddMonths(18) : DateTime.Now.AddMonths(-6);

                thietBis.Add(new ThietBi
                {
                    MaLoai = item.LoaiId, // Gán chính xác theo danh mục loại thiết bị
                    MaKhachHang = khId,
                    SerialNumber = $"SN-UNETI-{2024 + i:D4}-{i + 100}",
                    TenThietBi = item.Name,
                    NgayMua = ngayMua,
                    HanBaoHanh = hanBaoHanh,
                    MoTa = $"Thiết bị chính hãng, phân phối cho khách hàng ID {khId}",
                    TrangThai = (i % 3 == 0) ? TrangThaiThietBi.DangSuaChua : TrangThaiThietBi.HoatDong,
                    GiaTri = item.Gia,
                    GhiChu = conBaoHanh ? "Còn trong diện bảo hành chính hãng" : "Hết hạn bảo hành, sửa tính phí"
                });
            }
            context.ThietBis.AddRange(thietBis);
            context.SaveChanges();

            // ==============================================================
            // 6. PHIẾU SỬA CHỮA (45 phiếu với đủ trạng thái)
            // ==============================================================
            var phieuSuaChuas = new List<PhieuSuaChua>();
            string[] trangThais = {
                TrangThaiPhieu.ChoTiepNhan,
                TrangThaiPhieu.DangXuLy,
                TrangThaiPhieu.HoanThanh,
                TrangThaiPhieu.DaHuy,
                TrangThaiPhieu.TuChoi
            };

            for (int i = 1; i <= 45; i++)
            {
                var tb = thietBis[(i - 1) % 15];
                string st = trangThais[(i - 1) % trangThais.Length];
                DateTime ngayNhan = DateTime.Now.AddDays(-50 + i);

                phieuSuaChuas.Add(new PhieuSuaChua
                {
                    MaThietBi = tb.MaThietBi,
                    MaKhachHang = tb.MaKhachHang,
                    NgayTiepNhan = ngayNhan,
                    NoiDungLoi = $"Thiết bị gặp sự cố lỗi nguồn/màn hình/bàn phím số #{i}",
                    TrangThai = st,
                    MucDo = (i % 4 == 0) ? MucDoUuTien.KhanCap : MucDoUuTien.TrungBinh,
                    HanDuKien = ngayNhan.AddDays(3),
                    NgayHoanThanh = (st == TrangThaiPhieu.HoanThanh) ? ngayNhan.AddDays(2) : null
                });
            }
            context.PhieuSuaChuas.AddRange(phieuSuaChuas);
            context.SaveChanges();

            // ==============================================================
            // 7. CHI TIẾT SỬA CHỮA (18 bản ghi)
            // ==============================================================
            var chiTiets = new List<ChiTietSuaChua>();
            int ktvNamId = taiKhoans[2].MaTaiKhoan;
            int ktvTuanId = taiKhoans[3].MaTaiKhoan;
            int ktvLongId = taiKhoans[4].MaTaiKhoan;

            for (int i = 1; i <= 18; i++)
            {
                var lk = linhKiens[(i - 1) % linhKiens.Count];
                int sl = (i % 2) + 1;
                int ktvId = (i % 3 == 0) ? ktvNamId : (i % 3 == 1 ? ktvTuanId : ktvLongId);

                chiTiets.Add(new ChiTietSuaChua
                {
                    MaPhieu = i,
                    MaKyThuatVien = ktvId,
                    MaLinhKien = lk.MaLinhKien,
                    SoLuong = sl,
                    DonGia = lk.DonGia,
                    ThanhTien = lk.DonGia * sl,
                    KetQuaChanDoan = $"Phát hiện lỗi linh kiện, đã thay thế linh kiện {lk.TenLinhKien} và kiểm tra hoạt động ổn định.",
                    GhiChu = "Đã kiểm thử 24h trước khi bàn giao"
                });
            }
            context.ChiTietSuaChuas.AddRange(chiTiets);
            context.SaveChanges();
        }
    }
}