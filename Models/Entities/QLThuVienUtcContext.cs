using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTWEB.Models.Entities;

public partial class QLThuVienUtcContext : DbContext
{
    public QLThuVienUtcContext()
    {
    }

    public QLThuVienUtcContext(DbContextOptions<QLThuVienUtcContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BanDoc> BanDocs { get; set; }

    public virtual DbSet<BanSao> BanSaos { get; set; }

    public virtual DbSet<ChiTietMuon> ChiTietMuons { get; set; }

    public virtual DbSet<ChiTietThanhToan> ChiTietThanhToans { get; set; }

    public virtual DbSet<DatTruoc> DatTruocs { get; set; }

    public virtual DbSet<DinhDang> DinhDangs { get; set; }

    public virtual DbSet<GiaHan> GiaHans { get; set; }

    public virtual DbSet<LichSuDatTruoc> LichSuDatTruocs { get; set; }

    public virtual DbSet<Lop> Lops { get; set; }

    public virtual DbSet<NgonNgu> NgonNgus { get; set; }

    public virtual DbSet<NhaXuatBan> NhaXuatBans { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhieuMuon> PhieuMuons { get; set; }

    public virtual DbSet<QuocGium> QuocGia { get; set; }

    public virtual DbSet<Quyen> Quyens { get; set; }

    public virtual DbSet<TacGium> TacGia { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TaiLieu> TaiLieus { get; set; }

    public virtual DbSet<TaiLieuTacGium> TaiLieuTacGia { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<TheBanDoc> TheBanDocs { get; set; }

    public virtual DbSet<TheLoai> TheLoais { get; set; }

    public virtual DbSet<TienPhat> TienPhats { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<ViPham> ViPhams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BanDoc>(entity =>
        {
            entity.HasKey(e => e.MaBanDoc).HasName("PK__BanDoc__5D4A327723E3E35B");

            entity.ToTable("BanDoc");

            entity.HasIndex(e => e.HoTen, "IX_BanDoc_HoTen");

            entity.HasIndex(e => e.MaLop, "IX_BanDoc_MaLop");

            entity.HasIndex(e => e.MaSinhVien, "UQ__BanDoc__939AE774D3472031").IsUnique();

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaLop)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Hoạt động");

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.BanDocs)
                .HasForeignKey(d => d.MaLop)
                .HasConstraintName("FK_BanDoc_Lop");
        });

        modelBuilder.Entity<BanSao>(entity =>
        {
            entity.HasKey(e => e.MaBanSao).HasName("PK__BanSao__488BCC42E5484FAF");

            entity.ToTable("BanSao");

            entity.HasIndex(e => e.MaTaiLieu, "IX_BanSao_MaTaiLieu");

            entity.HasIndex(e => e.MaVach, "UQ__BanSao__8BBF4A1C722CE485").IsUnique();

            entity.Property(e => e.MaVach)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(30)
                .HasDefaultValue("Tốt");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Có sẵn");
            entity.Property(e => e.ViTriKe).HasMaxLength(100);

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.BanSaos)
                .HasForeignKey(d => d.MaTaiLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BanSao__MaTaiLie__24B26D99");
        });

        modelBuilder.Entity<ChiTietMuon>(entity =>
        {
            entity.HasKey(e => e.MaChiTietMuon).HasName("PK__ChiTietM__C9A9D6BE39B54BB3");

            entity.ToTable("ChiTietMuon");

            entity.HasIndex(e => e.MaBanSao, "IX_ChiTietMuon_BanSao");

            entity.Property(e => e.TinhTrangKhiTra).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đang mượn");

            entity.HasOne(d => d.MaBanSaoNavigation).WithMany(p => p.ChiTietMuons)
                .HasForeignKey(d => d.MaBanSao)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietMu__MaBan__2F2FFC0C");

            entity.HasOne(d => d.MaPhieuMuonNavigation).WithMany(p => p.ChiTietMuons)
                .HasForeignKey(d => d.MaPhieuMuon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietMu__MaPhi__2E3BD7D3");
        });

        modelBuilder.Entity<ChiTietThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaChiTietThanhToan).HasName("PK__ChiTietT__D73899C3FD1D852C");

            entity.ToTable("ChiTietThanhToan");

            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaThanhToanNavigation).WithMany(p => p.ChiTietThanhToans)
                .HasForeignKey(d => d.MaThanhToan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietTh__MaTha__52793849");

            entity.HasOne(d => d.MaTienPhatNavigation).WithMany(p => p.ChiTietThanhToans)
                .HasForeignKey(d => d.MaTienPhat)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietTh__MaTie__536D5C82");
        });

        modelBuilder.Entity<DatTruoc>(entity =>
        {
            entity.HasKey(e => e.MaDatTruoc).HasName("PK__DatTruoc__81E1C492AB3C75EC");

            entity.ToTable("DatTruoc");

            entity.HasIndex(e => e.MaBanDoc, "IX_DatTruoc_BanDoc");

            entity.Property(e => e.NgayDat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đang chờ");

            entity.HasOne(d => d.MaBanDocNavigation).WithMany(p => p.DatTruocs)
                .HasForeignKey(d => d.MaBanDoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DatTruoc__MaBanD__39AD8A7F");

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.DatTruocs)
                .HasForeignKey(d => d.MaTaiLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DatTruoc__MaTaiL__3AA1AEB8");
        });

        modelBuilder.Entity<DinhDang>(entity =>
        {
            entity.HasKey(e => e.MaDinhDang).HasName("PK__DinhDang__31E76237803DBF86");

            entity.ToTable("DinhDang");

            entity.HasIndex(e => e.TenDinhDang, "UQ__DinhDang__74BC616FE3903D09").IsUnique();

            entity.Property(e => e.TenDinhDang).HasMaxLength(50);
        });

        modelBuilder.Entity<GiaHan>(entity =>
        {
            entity.HasKey(e => e.MaGiaHan).HasName("PK__GiaHan__C3260BA4520C09F7");

            entity.ToTable("GiaHan");

            entity.Property(e => e.LyDo).HasMaxLength(255);
            entity.Property(e => e.NgayGiaHan).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.MaChiTietMuonNavigation).WithMany(p => p.GiaHans)
                .HasForeignKey(d => d.MaChiTietMuon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GiaHan__MaChiTie__33008CF0");

            entity.HasOne(d => d.MaNhanVienDuyetNavigation).WithMany(p => p.GiaHans)
                .HasForeignKey(d => d.MaNhanVienDuyet)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GiaHan__MaNhanVi__33F4B129");
        });

        modelBuilder.Entity<LichSuDatTruoc>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LichSuDa__C443222A3C989FBF");

            entity.ToTable("LichSuDatTruoc");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.ThoiGian).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThaiCu).HasMaxLength(30);
            entity.Property(e => e.TrangThaiMoi).HasMaxLength(30);

            entity.HasOne(d => d.MaDatTruocNavigation).WithMany(p => p.LichSuDatTruocs)
                .HasForeignKey(d => d.MaDatTruoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuDat__MaDat__3E723F9C");
        });

        modelBuilder.Entity<Lop>(entity =>
        {
            entity.HasKey(e => e.MaLop).HasName("PK__Lop__3B98D273BE8D3203");

            entity.ToTable("Lop");

            entity.Property(e => e.MaLop)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.KhoaHoc).HasMaxLength(30);
            entity.Property(e => e.TenLop).HasMaxLength(100);
        });

        modelBuilder.Entity<NgonNgu>(entity =>
        {
            entity.HasKey(e => e.MaNgonNgu).HasName("PK__NgonNgu__131924EE8A850802");

            entity.ToTable("NgonNgu");

            entity.Property(e => e.MaNgonNgu)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenNgonNgu).HasMaxLength(50);
        });

        modelBuilder.Entity<NhaXuatBan>(entity =>
        {
            entity.HasKey(e => e.MaNhaXuatBan).HasName("PK__NhaXuatB__1AED0BFA0E8AA364");

            entity.ToTable("NhaXuatBan");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.MaQuocGia)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenNhaXuatBan).HasMaxLength(150);

            entity.HasOne(d => d.MaQuocGiaNavigation).WithMany(p => p.NhaXuatBans)
                .HasForeignKey(d => d.MaQuocGia)
                .HasConstraintName("FK__NhaXuatBa__MaQuo__0FB750B3");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA47E286EE90");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.MaSoNhanVien, "UQ__NhanVien__9913448808075356").IsUnique();

            entity.Property(e => e.ChucVu).HasMaxLength(100);
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaSoNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đang làm việc");
        });

        modelBuilder.Entity<PhieuMuon>(entity =>
        {
            entity.HasKey(e => e.MaPhieuMuon).HasName("PK__PhieuMuo__C4C8222215CFB375");

            entity.ToTable("PhieuMuon");

            entity.HasIndex(e => e.MaTheBanDoc, "IX_PhieuMuon_The");

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.NgayMuon).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đang mượn");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhieuMuons)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuMuon__MaNha__2A6B46EF");

            entity.HasOne(d => d.MaTheBanDocNavigation).WithMany(p => p.PhieuMuons)
                .HasForeignKey(d => d.MaTheBanDoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhieuMuon__MaThe__297722B6");
        });

        modelBuilder.Entity<QuocGium>(entity =>
        {
            entity.HasKey(e => e.MaQuocGia).HasName("PK__QuocGia__30D69ACBAA211267");

            entity.Property(e => e.MaQuocGia)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TenQuocGia).HasMaxLength(100);
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen).HasName("PK__Quyen__1D4B7ED4584C7774");

            entity.ToTable("Quyen");

            entity.Property(e => e.MaQuyen)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenQuyen).HasMaxLength(100);
        });

        modelBuilder.Entity<TacGium>(entity =>
        {
            entity.HasKey(e => e.MaTacGia).HasName("PK__TacGia__F24E67561784170A");

            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.MaQuocGia)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.MaQuocGiaNavigation).WithMany(p => p.TacGia)
                .HasForeignKey(d => d.MaQuocGia)
                .HasConstraintName("FK__TacGia__MaQuocGi__1293BD5E");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529732704A3");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.MaBanDoc, "UQ_TaiKhoan_BanDoc")
                .IsUnique()
                .HasFilter("([MaBanDoc] IS NOT NULL)");

            entity.HasIndex(e => e.EmailDangNhap, "UQ_TaiKhoan_Email")
                .IsUnique()
                .HasFilter("([EmailDangNhap] IS NOT NULL)");

            entity.HasIndex(e => e.MaNhanVien, "UQ_TaiKhoan_NhanVien")
                .IsUnique()
                .HasFilter("([MaNhanVien] IS NOT NULL)");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC07ED003B9").IsUnique();

            entity.Property(e => e.EmailDangNhap)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.MatKhauHash).HasMaxLength(500);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Hoạt động");
            entity.Property(e => e.VaiTro).HasMaxLength(50);

            entity.HasOne(d => d.MaBanDocNavigation).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaBanDoc)
                .HasConstraintName("FK_TaiKhoan_BanDoc");

            entity.HasOne(d => d.MaNhanVienNavigation).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaNhanVien)
                .HasConstraintName("FK_TaiKhoan_NhanVien");

            entity.HasMany(d => d.MaVaiTros).WithMany(p => p.MaTaiKhoans)
                .UsingEntity<Dictionary<string, object>>(
                    "TaiKhoanVaiTro",
                    r => r.HasOne<VaiTro>().WithMany()
                        .HasForeignKey("MaVaiTro")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TaiKhoan___MaVai__77DFC722"),
                    l => l.HasOne<TaiKhoan>().WithMany()
                        .HasForeignKey("MaTaiKhoan")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TaiKhoan___MaTai__76EBA2E9"),
                    j =>
                    {
                        j.HasKey("MaTaiKhoan", "MaVaiTro").HasName("PK__TaiKhoan__4158A1359EFB6305");
                        j.ToTable("TaiKhoan_VaiTro");
                        j.IndexerProperty<string>("MaVaiTro")
                            .HasMaxLength(20)
                            .IsUnicode(false);
                    });
        });

        modelBuilder.Entity<TaiLieu>(entity =>
        {
            entity.HasKey(e => e.MaTaiLieu).HasName("PK__TaiLieu__FD18A657303086AE");

            entity.ToTable("TaiLieu");

            entity.HasIndex(e => e.TenTaiLieu, "IX_TaiLieu_Ten");

            entity.Property(e => e.AnhBia).HasMaxLength(500);
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ISBN");
            entity.Property(e => e.MaNgonNgu)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TenTaiLieu).HasMaxLength(250);

            entity.HasOne(d => d.MaDinhDangNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaDinhDang)
                .HasConstraintName("FK__TaiLieu__MaDinhD__1940BAED");

            entity.HasOne(d => d.MaNgonNguNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaNgonNgu)
                .HasConstraintName("FK__TaiLieu__MaNgonN__1758727B");

            entity.HasOne(d => d.MaNhaXuatBanNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaNhaXuatBan)
                .HasConstraintName("FK__TaiLieu__MaNhaXu__16644E42");

            entity.HasOne(d => d.MaTheLoaiNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaTheLoai)
                .HasConstraintName("FK__TaiLieu__MaTheLo__184C96B4");
        });

        modelBuilder.Entity<TaiLieuTacGium>(entity =>
        {
            entity.HasKey(e => new { e.MaTaiLieu, e.MaTacGia }).HasName("PK__TaiLieu___823C4022A66C6F30");

            entity.ToTable("TaiLieu_TacGia");

            entity.Property(e => e.VaiTroTacGia).HasMaxLength(50);

            entity.HasOne(d => d.MaTacGiaNavigation).WithMany(p => p.TaiLieuTacGia)
                .HasForeignKey(d => d.MaTacGia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiLieu_T__MaTac__1EF99443");

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.TaiLieuTacGia)
                .HasForeignKey(d => d.MaTaiLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaiLieu_T__MaTai__1E05700A");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__ThanhToa__D4B258444FC08800");

            entity.ToTable("ThanhToan");

            entity.Property(e => e.NgayThanhToan).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PhuongThuc).HasMaxLength(50);
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đã thanh toán");

            entity.HasOne(d => d.MaBanDocNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaBanDoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThanhToan__MaBan__4DB4832C");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThanhToan__MaNha__4EA8A765");
        });

        modelBuilder.Entity<TheBanDoc>(entity =>
        {
            entity.HasKey(e => e.MaTheBanDoc).HasName("PK__TheBanDo__9541C9A65F749DC6");

            entity.ToTable("TheBanDoc");

            entity.HasIndex(e => e.SoThe, "UQ__TheBanDo__26CB8FE8C3E0C368").IsUnique();

            entity.HasIndex(e => e.MaBanDoc, "UQ__TheBanDo__5D4A3276455F7B25").IsUnique();

            entity.Property(e => e.SoThe)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Hoạt động");

            entity.HasOne(d => d.MaBanDocNavigation).WithOne(p => p.TheBanDoc)
                .HasForeignKey<TheBanDoc>(d => d.MaBanDoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TheBanDoc__MaBan__0169315C");
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.HasKey(e => e.MaTheLoai).HasName("PK__TheLoai__D73FF34AC6FFA9C1");

            entity.ToTable("TheLoai");

            entity.HasIndex(e => e.TenTheLoai, "UQ__TheLoai__327F958F4132D0D9").IsUnique();

            entity.Property(e => e.TenTheLoai).HasMaxLength(100);

            entity.HasOne(d => d.MaTheLoaiChaNavigation).WithMany(p => p.InverseMaTheLoaiChaNavigation)
                .HasForeignKey(d => d.MaTheLoaiCha)
                .HasConstraintName("FK__TheLoai__MaTheLo__09FE775D");
        });

        modelBuilder.Entity<TienPhat>(entity =>
        {
            entity.HasKey(e => e.MaTienPhat).HasName("PK__TienPhat__3CB5ED381F409B99");

            entity.ToTable("TienPhat");

            entity.HasIndex(e => e.MaViPham, "UQ__TienPhat__F1921D88DA646F90").IsUnique();

            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Chưa thanh toán");

            entity.HasOne(d => d.MaViPhamNavigation).WithOne(p => p.TienPhat)
                .HasForeignKey<TienPhat>(d => d.MaViPham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TienPhat__MaViPh__47FBA9D6");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF9904DE1B");

            entity.ToTable("VaiTro");

            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenVaiTro).HasMaxLength(100);

            entity.HasMany(d => d.MaQuyens).WithMany(p => p.MaVaiTros)
                .UsingEntity<Dictionary<string, object>>(
                    "VaiTroQuyen",
                    r => r.HasOne<Quyen>().WithMany()
                        .HasForeignKey("MaQuyen")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__VaiTro_Qu__MaQuy__7BB05806"),
                    l => l.HasOne<VaiTro>().WithMany()
                        .HasForeignKey("MaVaiTro")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__VaiTro_Qu__MaVai__7ABC33CD"),
                    j =>
                    {
                        j.HasKey("MaVaiTro", "MaQuyen").HasName("PK__VaiTro_Q__9398F622FC9E8234");
                        j.ToTable("VaiTro_Quyen");
                        j.IndexerProperty<string>("MaVaiTro")
                            .HasMaxLength(20)
                            .IsUnicode(false);
                        j.IndexerProperty<string>("MaQuyen")
                            .HasMaxLength(50)
                            .IsUnicode(false);
                    });
        });

        modelBuilder.Entity<ViPham>(entity =>
        {
            entity.HasKey(e => e.MaViPham).HasName("PK__ViPham__F1921D892F515354");

            entity.ToTable("ViPham");

            entity.Property(e => e.LoaiViPham).HasMaxLength(100);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.NgayViPham).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.MaBanDocNavigation).WithMany(p => p.ViPhams)
                .HasForeignKey(d => d.MaBanDoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ViPham__MaBanDoc__4242D080");

            entity.HasOne(d => d.MaChiTietMuonNavigation).WithMany(p => p.ViPhams)
                .HasForeignKey(d => d.MaChiTietMuon)
                .HasConstraintName("FK__ViPham__MaChiTie__4336F4B9");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
