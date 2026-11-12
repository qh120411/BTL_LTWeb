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
    public virtual DbSet<Lop> Lops { get; set; }
    public virtual DbSet<NhaXuatBan> NhaXuatBans { get; set; }
    public virtual DbSet<NhanVien> NhanViens { get; set; }
    public virtual DbSet<PhieuMuon> PhieuMuons { get; set; }
    public virtual DbSet<TacGium> TacGia { get; set; }
    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }
    public virtual DbSet<TaiLieu> TaiLieus { get; set; }
    public virtual DbSet<TaiLieuTacGium> TaiLieuTacGia { get; set; }
    public virtual DbSet<ThanhToan> ThanhToans { get; set; }
    public virtual DbSet<TheLoai> TheLoais { get; set; }
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
            entity.HasKey(e => e.MaBanDoc);
            entity.ToTable("BanDoc");
            entity.Property(e => e.SoCCCD).HasMaxLength(20);
            entity.Property(e => e.NganhHoc).HasMaxLength(150);
            entity.Property(e => e.SoTaiKhoanNganHang).HasMaxLength(50);
            entity.Property(e => e.TenNganHang).HasMaxLength(150);
            entity.Property(e => e.AvatarUrl).HasMaxLength(255);

            entity.HasIndex(e => e.HoTen, "IX_BanDoc_HoTen");
            entity.HasIndex(e => e.MaLop, "IX_BanDoc_MaLop");
            entity.HasIndex(e => e.MaSinhVien).IsUnique();

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaLop).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.MaSinhVien).HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(30).HasDefaultValue("Hoạt động");

            entity.Property(e => e.SoThe).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.NgayCapThe);
            entity.Property(e => e.NgayHetHanThe);
            entity.Property(e => e.TrangThaiThe).HasMaxLength(60);

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.BanDocs)
                .HasForeignKey(d => d.MaLop)
                .HasConstraintName("FK_BanDoc_Lop");
        });

        modelBuilder.Entity<BanSao>(entity =>
        {
            entity.HasKey(e => e.MaBanSao);
            entity.ToTable("BanSao");

            entity.HasIndex(e => e.MaTaiLieu, "IX_BanSao_MaTaiLieu");
            entity.HasIndex(e => e.MaVach).IsUnique();

            entity.Property(e => e.MaVach).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.TinhTrang).HasMaxLength(30).HasDefaultValue("Tốt");
            entity.Property(e => e.TrangThai).HasMaxLength(30).HasDefaultValue("Có sẵn");
            entity.Property(e => e.ViTriKe).HasMaxLength(100);

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.BanSaos)
                .HasForeignKey(d => d.MaTaiLieu)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ChiTietMuon>(entity =>
        {
            entity.HasKey(e => e.MaChiTietMuon);
            entity.ToTable("ChiTietMuon");

            entity.HasIndex(e => e.MaBanSao, "IX_ChiTietMuon_BanSao");

            entity.Property(e => e.TinhTrangKhiTra).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasMaxLength(30).HasDefaultValue("Đang mượn");

            entity.HasOne(d => d.MaBanSaoNavigation).WithMany(p => p.ChiTietMuons)
                .HasForeignKey(d => d.MaBanSao)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.MaPhieuMuonNavigation).WithMany(p => p.ChiTietMuons)
                .HasForeignKey(d => d.MaPhieuMuon)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Lop>(entity =>
        {
            entity.HasKey(e => e.MaLop);
            entity.ToTable("Lop");

            entity.Property(e => e.MaLop).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.KhoaHoc).HasMaxLength(30);
            entity.Property(e => e.TenLop).HasMaxLength(100);
        });

        modelBuilder.Entity<NhaXuatBan>(entity =>
        {
            entity.HasKey(e => e.MaNhaXuatBan);
            entity.ToTable("NhaXuatBan");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.QuocGia).HasMaxLength(100);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.TenNhaXuatBan).HasMaxLength(150);
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien);
            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.MaSoNhanVien).IsUnique();

            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaSoNhanVien).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(30).HasDefaultValue("Hoạt động");
        });

        modelBuilder.Entity<PhieuMuon>(entity =>
        {
            entity.HasKey(e => e.MaPhieuMuon);
            entity.ToTable("PhieuMuon");

            entity.HasIndex(e => e.MaBanDoc, "IX_PhieuMuon_BanDoc");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayMuon).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasMaxLength(30).HasDefaultValue("Đang mượn");

            entity.HasOne(d => d.MaBanDocNavigation).WithMany(p => p.PhieuMuons)
                .HasForeignKey(d => d.MaBanDoc)
                .HasConstraintName("FK_PhieuMuon_BanDoc");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhieuMuons)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TacGium>(entity =>
        {
            entity.HasKey(e => e.MaTacGia);
            entity.ToTable("TacGia");

            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.QuocTich).HasMaxLength(100);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan);
            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.MaBanDoc).IsUnique();
            entity.HasIndex(e => e.MaNhanVien).IsUnique();
            entity.HasIndex(e => e.TenDangNhap).IsUnique();

            entity.Property(e => e.EmailDangNhap).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.MatKhauHash).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TenDangNhap).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(30).HasDefaultValue("Hoạt động");
            entity.Property(e => e.VaiTro).HasMaxLength(50);

            entity.HasOne(d => d.MaBanDocNavigation).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaBanDoc);

            entity.HasOne(d => d.MaNhanVienNavigation).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaNhanVien);
        });

        modelBuilder.Entity<TaiLieu>(entity =>
        {
            entity.HasKey(e => e.MaTaiLieu);
            entity.ToTable("TaiLieu");

            entity.HasIndex(e => e.TenTaiLieu, "IX_TaiLieu_Ten");

            entity.Property(e => e.AnhBia).HasMaxLength(255);
            entity.Property(e => e.DinhDang).HasMaxLength(100);
            entity.Property(e => e.NgonNgu).HasMaxLength(100);
            entity.Property(e => e.Isbn).HasMaxLength(20).IsUnicode(false).HasColumnName("ISBN");
            entity.Property(e => e.LanXuatBan).HasDefaultValue(1);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TenTaiLieu).HasMaxLength(255);

            entity.HasOne(d => d.MaNhaXuatBanNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaNhaXuatBan);

            entity.HasOne(d => d.MaTheLoaiNavigation).WithMany(p => p.TaiLieus)
                .HasForeignKey(d => d.MaTheLoai);
        });

        modelBuilder.Entity<TaiLieuTacGium>(entity =>
        {
            entity.HasKey(e => new { e.MaTaiLieu, e.MaTacGia });
            entity.ToTable("TaiLieu_TacGia");

            entity.Property(e => e.VaiTroTacGia).HasMaxLength(50).HasDefaultValue("Tác giả");

            entity.HasOne(d => d.MaTacGiaNavigation).WithMany(p => p.TaiLieuTacGia)
                .HasForeignKey(d => d.MaTacGia)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.MaTaiLieuNavigation).WithMany(p => p.TaiLieuTacGia)
                .HasForeignKey(d => d.MaTaiLieu)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan);
            entity.ToTable("ThanhToan");
            entity.Property(e => e.TienNopMuon).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TienDenBu).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LoaiKhoanThu).HasMaxLength(50);
            entity.Property(e => e.HinhThucXuLy).HasMaxLength(150);
            entity.Property(e => e.GhiChu).HasMaxLength(500);

            entity.Property(e => e.NgayThanhToan).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PhuongThuc).HasMaxLength(50).HasDefaultValue("Tiền mặt");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(30).HasDefaultValue("Thành công");
            entity.Property(e => e.NoiDung).HasMaxLength(500);

            entity.HasOne(d => d.MaBanDocNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaBanDoc)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.HasKey(e => e.MaTheLoai);
            entity.ToTable("TheLoai");

            entity.Property(e => e.TenTheLoai).HasMaxLength(100);

            entity.HasOne(d => d.MaTheLoaiChaNavigation).WithMany(p => p.InverseMaTheLoaiChaNavigation)
                .HasForeignKey(d => d.MaTheLoaiCha);
        });

        modelBuilder.Entity<ViPham>(entity =>
        {
            entity.HasKey(e => e.MaViPham);
            entity.ToTable("ViPham");

            entity.Property(e => e.LoaiViPham).HasMaxLength(50);
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.NgayViPham).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoTienPhat).HasColumnType("decimal(18, 2)").HasDefaultValueSql("((0))");
            entity.Property(e => e.TrangThaiNop).HasMaxLength(60).HasDefaultValue("Chưa nộp");

            entity.HasOne(d => d.MaBanDocNavigation).WithMany(p => p.ViPhams)
                .HasForeignKey(d => d.MaBanDoc)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.MaChiTietMuonNavigation).WithMany(p => p.ViPhams)
                .HasForeignKey(d => d.MaChiTietMuon);

            entity.HasOne(d => d.MaNhanVienThuNavigation).WithMany(p => p.ViPhams)
                .HasForeignKey(d => d.MaNhanVienThu);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
