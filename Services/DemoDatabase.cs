using BTL_LTWEB.Common;
using BTL_LTWEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Services;
public static class DemoDatabase
{
    public static void Initialize(QLThuVienUtcContext db)
    {
        Directory.CreateDirectory("App_Data");
        db.Database.EnsureCreated();
        if (db.TaiKhoans.Any())
            return;
        using var transaction = db.Database.BeginTransaction();
        var admin = new VaiTro
        {
            MaVaiTro = "ADMIN",
            TenVaiTro = "Quản trị"
        };
        var readerRole = new VaiTro
        {
            MaVaiTro = "BANDOC",
            TenVaiTro = "Bạn đọc"
        };
        var staffRole = new VaiTro
        {
            MaVaiTro = "QUANLY_BANDOC",
            TenVaiTro = "Quản lý bạn đọc"
        };
        db.VaiTros.AddRange(admin, readerRole, staffRole);
        db.NgonNgus.AddRange(new NgonNgu { MaNgonNgu = "VIE", TenNgonNgu = "Tiếng Việt" }, new NgonNgu { MaNgonNgu = "ENG", TenNgonNgu = "Tiếng Anh" });
        var staff = new NhanVien
        {
            MaSoNhanVien = "NV-DEMO",
            HoTen = "Quản trị demo",
            TrangThai = "Đang làm việc"
        };
        db.NhanViens.Add(staff);
        db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = "admin", MatKhauHash = PasswordHelper.HashPassword("Admin@123"), MaNhanVienNavigation = staff, TrangThai = "Hoạt động", NgayTao = DateTime.Now, MaVaiTros = new List<VaiTro> { admin } });
        for (var i = 0; i < 2; i++)
        {
            var id = i == 0 ? "242630944" : "242630945";
            var reader = new BanDoc
            {
                MaSinhVien = id,
                HoTen = i == 0 ? "Trần Quang Huy" : "Bạn đọc demo",
                Email = id + "@st.utc.edu.vn",
                TrangThai = "Đang học",
                NgayTao = DateTime.Now
            };
            reader.TheBanDoc = new TheBanDoc
            {
                SoThe = "THE-" + id,
                NgayCap = DateOnly.FromDateTime(DateTime.Today),
                NgayHetHan = DateOnly.FromDateTime(DateTime.Today.AddYears(2)),
                TrangThai = "Hoạt động"
            };
            db.BanDocs.Add(reader);
            db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = id, EmailDangNhap = reader.Email, MatKhauHash = PasswordHelper.HashPassword("UTC@123"), MaBanDocNavigation = reader, TrangThai = "Hoạt động", NgayTao = DateTime.Now, MaVaiTros = new List<VaiTro> { readerRole } });
        }

        db.SaveChanges();
        {
            var t = new TaiLieu
            {
                TenTaiLieu = "Tiếng Anh chuyên ngành Quản lý xây dựng",
                MaNgonNgu = "VIE",
                NamXuatBan = 2024,
                SoTrang = 300,
                NgayTao = DateTime.Now,
                MoTa = "Học liệu minh họa để thử giao diện và nghiệp vụ.",
                AnhBia = "/images/utc/cover-english.png",
                MaTheLoaiNavigation = new TheLoai
                {
                    TenTheLoai = "Ngoại ngữ · Xây dựng"
                },
                TaiLieuTacGia = new List<TaiLieuTacGium>
                {
                    new()
                    {
                        MaTacGiaNavigation = new TacGium
                        {
                            HoTen = "Nguyễn Hoàng Tùng, Vũ Minh Hằng"
                        },
                        VaiTroTacGia = "Tác giả"
                    }
                }
            };
            db.TaiLieus.Add(t);
            db.SaveChanges();
            for (var i = 1; i <= 4; i++)
                db.BanSaos.Add(new BanSao { MaTaiLieu = t.MaTaiLieu, MaVach = $"UTC-{t.MaTaiLieu:D5}-{i:D3}", TrangThai = "Có sẵn", TinhTrang = "Tốt", ViTriKe = "Tầng 6 · Nhà A8", NgayNhap = DateOnly.FromDateTime(DateTime.Today) });
            db.SaveChanges();
        }

        {
            var t = new TaiLieu
            {
                TenTaiLieu = "Xử lý ảnh trong công nghiệp và giao thông",
                MaNgonNgu = "VIE",
                NamXuatBan = 2024,
                SoTrang = 300,
                NgayTao = DateTime.Now,
                MoTa = "Học liệu minh họa để thử giao diện và nghiệp vụ.",
                AnhBia = "/images/utc/cover-image-processing.png",
                MaTheLoaiNavigation = new TheLoai
                {
                    TenTheLoai = "Công nghệ thông tin"
                },
                TaiLieuTacGia = new List<TaiLieuTacGium>
                {
                    new()
                    {
                        MaTacGiaNavigation = new TacGium
                        {
                            HoTen = "Lê Thị Thúy Nga"
                        },
                        VaiTroTacGia = "Tác giả"
                    }
                }
            };
            db.TaiLieus.Add(t);
            db.SaveChanges();
            for (var i = 1; i <= 4; i++)
                db.BanSaos.Add(new BanSao { MaTaiLieu = t.MaTaiLieu, MaVach = $"UTC-{t.MaTaiLieu:D5}-{i:D3}", TrangThai = "Có sẵn", TinhTrang = "Tốt", ViTriKe = "Tầng 6 · Nhà A8", NgayNhap = DateOnly.FromDateTime(DateTime.Today) });
            db.SaveChanges();
        }

        {
            var t = new TaiLieu
            {
                TenTaiLieu = "Cơ sở GIS và ứng dụng trong giao thông vận tải",
                MaNgonNgu = "VIE",
                NamXuatBan = 2024,
                SoTrang = 300,
                NgayTao = DateTime.Now,
                MoTa = "Học liệu minh họa để thử giao diện và nghiệp vụ.",
                AnhBia = "/images/utc/cover-gis.png",
                MaTheLoaiNavigation = new TheLoai
                {
                    TenTheLoai = "Dữ liệu · Giao thông"
                },
                TaiLieuTacGia = new List<TaiLieuTacGium>
                {
                    new()
                    {
                        MaTacGiaNavigation = new TacGium
                        {
                            HoTen = "Lê Khánh Giang"
                        },
                        VaiTroTacGia = "Tác giả"
                    }
                }
            };
            db.TaiLieus.Add(t);
            db.SaveChanges();
            for (var i = 1; i <= 4; i++)
                db.BanSaos.Add(new BanSao { MaTaiLieu = t.MaTaiLieu, MaVach = $"UTC-{t.MaTaiLieu:D5}-{i:D3}", TrangThai = "Có sẵn", TinhTrang = "Tốt", ViTriKe = "Tầng 6 · Nhà A8", NgayNhap = DateOnly.FromDateTime(DateTime.Today) });
            db.SaveChanges();
        }

        {
            var t = new TaiLieu
            {
                TenTaiLieu = "Quy hoạch hệ thống công trình ngầm",
                MaNgonNgu = "VIE",
                NamXuatBan = 2024,
                SoTrang = 300,
                NgayTao = DateTime.Now,
                MoTa = "Học liệu minh họa để thử giao diện và nghiệp vụ.",
                AnhBia = "/images/utc/cover-underground.png",
                MaTheLoaiNavigation = new TheLoai
                {
                    TenTheLoai = "Quy hoạch · Xây dựng"
                },
                TaiLieuTacGia = new List<TaiLieuTacGium>
                {
                    new()
                    {
                        MaTacGiaNavigation = new TacGium
                        {
                            HoTen = "Trần Tuấn Hiệp, Vũ Ngọc Phương"
                        },
                        VaiTroTacGia = "Tác giả"
                    }
                }
            };
            db.TaiLieus.Add(t);
            db.SaveChanges();
            for (var i = 1; i <= 4; i++)
                db.BanSaos.Add(new BanSao { MaTaiLieu = t.MaTaiLieu, MaVach = $"UTC-{t.MaTaiLieu:D5}-{i:D3}", TrangThai = "Có sẵn", TinhTrang = "Tốt", ViTriKe = "Tầng 6 · Nhà A8", NgayNhap = DateOnly.FromDateTime(DateTime.Today) });
            db.SaveChanges();
        }

        transaction.Commit();
    }
}
