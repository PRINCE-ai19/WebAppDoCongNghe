using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CauHinhSanPham> CauHinhSanPhams { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DanhMuc> DanhMucs { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<GiaoHang> GiaoHangs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<HinhAnhSanPham> HinhAnhSanPhams { get; set; }

    public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }

    public virtual DbSet<LienHe> LienHes { get; set; }

    public virtual DbSet<LoaiTaiKhoan> LoaiTaiKhoans { get; set; }

    public virtual DbSet<PhanQuyen> PhanQuyens { get; set; }

    public virtual DbSet<PhieuGiamGium> PhieuGiamGia { get; set; }

    public virtual DbSet<SanPham> SanPhams { get; set; }

    public virtual DbSet<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; }

    public virtual DbSet<SanPhamYeuThich> SanPhamYeuThiches { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TaiKhoanPhanQuyen> TaiKhoanPhanQuyens { get; set; }

    public virtual DbSet<TaiKhoanPhieuGiamGium> TaiKhoanPhieuGiamGia { get; set; }

    public virtual DbSet<ThanhToanTam> ThanhToanTams { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TinTuc> TinTucs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:MyDb");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CauHinhSanPham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("CauHinhSanPham_pkey");

            entity.ToTable("CauHinhSanPham");

            entity.Property(e => e.GiaTri).HasMaxLength(255);
            entity.Property(e => e.TenThongSo).HasMaxLength(255);

            entity.HasOne(d => d.SanPham).WithMany(p => p.CauHinhSanPhams)
                .HasForeignKey(d => d.SanPhamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CauHinh_SanPham");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ChiTietDonHang_pkey");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DonGia).HasPrecision(18, 2);
            entity.Property(e => e.DonHangId).HasColumnName("DonHangID");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPhamID");

            entity.HasOne(d => d.DonHang).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.DonHangId)
                .HasConstraintName("FK_CTDH_DonHang");

            entity.HasOne(d => d.SanPham).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("FK_CTDH_SanPham");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ChiTietGioHang_pkey");

            entity.ToTable("ChiTietGioHang");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GioHangId).HasColumnName("GioHangID");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPhamID");

            entity.HasOne(d => d.GioHang).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.GioHangId)
                .HasConstraintName("FK_CTGH_GioHang");

            entity.HasOne(d => d.SanPham).WithMany(p => p.ChiTietGioHangs)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("FK_CTGH_SanPham");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DanhGia_pkey");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.NgayDanhGia).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NoiDung).HasMaxLength(255);
            entity.Property(e => e.SanPhamId).HasColumnName("SanPhamID");
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");

            entity.HasOne(d => d.SanPham).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("FK_DanhGia_SanPham");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.TaiKhoanId)
                .HasConstraintName("FK_DanhGia_TaiKhoan");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DanhMuc_pkey");

            entity.ToTable("DanhMuc");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenDanhMuc).HasMaxLength(100);
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DonHang_pkey");

            entity.ToTable("DonHang");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DiaChiGiao).HasMaxLength(255);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayDat).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");
            entity.Property(e => e.TongTien).HasPrecision(18, 2);
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.PhieuGiamGia).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.PhieuGiamGiaId)
                .HasConstraintName("FK_DonHang_PhieuGiamGia");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.TaiKhoanId)
                .HasConstraintName("FK_DonHang_TaiKhoan");
        });

        modelBuilder.Entity<GiaoHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("GiaoHang_pkey");

            entity.ToTable("GiaoHang");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DonHangId).HasColumnName("DonHangID");
            entity.Property(e => e.DonViVanChuyen).HasMaxLength(100);
            entity.Property(e => e.NgayCapNhat).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TrangThai).HasMaxLength(100);

            entity.HasOne(d => d.DonHang).WithMany(p => p.GiaoHangs)
                .HasForeignKey(d => d.DonHangId)
                .HasConstraintName("FK_GiaoHang_DonHang");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("GioHang_pkey");

            entity.ToTable("GioHang");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.NgayTao).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GioHang_TaiKhoan");
        });

        modelBuilder.Entity<HinhAnhSanPham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("HinhAnhSanPham_pkey");

            entity.ToTable("HinhAnhSanPham");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPhamID");

            entity.HasOne(d => d.SanPham).WithMany(p => p.HinhAnhSanPhams)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("FK_HinhAnhSanPham_SanPham");
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("KhuyenMai_pkey");

            entity.ToTable("KhuyenMai");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.PhanTramGiam).HasPrecision(5, 2);
            entity.Property(e => e.TenKhuyenMai).HasMaxLength(100);
        });

        modelBuilder.Entity<LienHe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("LienHe_pkey");

            entity.ToTable("LienHe");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayGui)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
        });

        modelBuilder.Entity<LoaiTaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("LoaiTaiKhoan_pkey");

            entity.ToTable("LoaiTaiKhoan");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TenLoaiTaiKhoan).HasMaxLength(100);
        });

        modelBuilder.Entity<PhanQuyen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PhanQuyen_pkey");

            entity.ToTable("PhanQuyen");

            entity.Property(e => e.MaChucNang).HasMaxLength(100);
            entity.Property(e => e.MoTa).HasMaxLength(255);
        });

        modelBuilder.Entity<PhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PhieuGiamGia_pkey");

            entity.HasIndex(e => e.MaPhieu, "PhieuGiamGia_MaPhieu_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GiaTriGiam).HasPrecision(10, 2);
            entity.Property(e => e.KieuGiam).HasMaxLength(10);
            entity.Property(e => e.MaPhieu).HasMaxLength(50);
            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.NgayBatDau).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NgayKetThuc).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SanPham_pkey");

            entity.ToTable("SanPham");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DanhMucId).HasColumnName("DanhMucID");
            entity.Property(e => e.Gia).HasPrecision(18, 2);
            entity.Property(e => e.GiaGiam).HasPrecision(18, 2);
            entity.Property(e => e.NgayThem).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TenSanPham).HasMaxLength(100);
            entity.Property(e => e.ThuongHieu).HasMaxLength(100);

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.SanPhams)
                .HasForeignKey(d => d.DanhMucId)
                .HasConstraintName("FK_SanPham_DanhMuc");
        });

        modelBuilder.Entity<SanPhamKhuyenMai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SanPham_KhuyenMai_pkey");

            entity.ToTable("SanPham_KhuyenMai");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.KhuyenMaiId).HasColumnName("KhuyenMaiID");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPhamID");

            entity.HasOne(d => d.KhuyenMai).WithMany(p => p.SanPhamKhuyenMais)
                .HasForeignKey(d => d.KhuyenMaiId)
                .HasConstraintName("FK_SPKhuyenMai_KhuyenMai");

            entity.HasOne(d => d.SanPham).WithMany(p => p.SanPhamKhuyenMais)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("FK_SPKhuyenMai_SanPham");
        });

        modelBuilder.Entity<SanPhamYeuThich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SanPhamYeuThich_pkey");

            entity.ToTable("SanPhamYeuThich");

            entity.HasIndex(e => new { e.TaiKhoanId, e.SanPhamId }, "UQ_YeuThich").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPhamID");
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");

            entity.HasOne(d => d.SanPham).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("FK_YeuThich_SanPham");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.SanPhamYeuThiches)
                .HasForeignKey(d => d.TaiKhoanId)
                .HasConstraintName("FK_YeuThich_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TaiKhoan_pkey");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.Email, "TaiKhoan_Email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HinhAnh).HasMaxLength(255);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.NgayDangKy).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Otp)
                .HasMaxLength(10)
                .HasColumnName("OTP");
            entity.Property(e => e.OtpExpire).HasColumnType("timestamp without time zone");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);

            entity.HasOne(d => d.IdLoaiTaiKhoanNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.IdLoaiTaiKhoan)
                .HasConstraintName("FK_TaiKhoan_LoaiTaiKhoan");
        });

        modelBuilder.Entity<TaiKhoanPhanQuyen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TaiKhoan_PhanQuyen_pkey");

            entity.ToTable("TaiKhoan_PhanQuyen");

            entity.Property(e => e.QuyenSua).HasDefaultValue(false);
            entity.Property(e => e.QuyenThem).HasDefaultValue(false);
            entity.Property(e => e.QuyenXem).HasDefaultValue(false);
            entity.Property(e => e.QuyenXoa).HasDefaultValue(false);

            entity.HasOne(d => d.IdChucNangNavigation).WithMany(p => p.TaiKhoanPhanQuyens)
                .HasForeignKey(d => d.IdChucNang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TK_PQ_ChucNang");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.TaiKhoanPhanQuyens)
                .HasForeignKey(d => d.IdTaiKhoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TK_PQ_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoanPhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TaiKhoan_PhieuGiamGia_pkey");

            entity.ToTable("TaiKhoan_PhieuGiamGia");

            entity.HasIndex(e => new { e.TaiKhoanId, e.PhieuGiamGiaId }, "UQ_TaiKhoan_PhieuGiamGia").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DaSuDung).HasDefaultValue(false);
            entity.Property(e => e.NgayNhan)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.NgaySuDung).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PhieuGiamGiaId).HasColumnName("PhieuGiamGiaID");
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");

            entity.HasOne(d => d.PhieuGiamGia).WithMany(p => p.TaiKhoanPhieuGiamGia)
                .HasForeignKey(d => d.PhieuGiamGiaId)
                .HasConstraintName("FK_TKPhieuGiamGia_PhieuGiamGia");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.TaiKhoanPhieuGiamGia)
                .HasForeignKey(d => d.TaiKhoanId)
                .HasConstraintName("FK_TKPhieuGiamGia_TaiKhoan");
        });

        modelBuilder.Entity<ThanhToanTam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ThanhToanTam_pkey");

            entity.ToTable("ThanhToanTam");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DonHangId).HasColumnName("DonHangID");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.NoiDung).HasMaxLength(255);
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");
            entity.Property(e => e.TongTien).HasPrecision(18, 2);
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.DonHang).WithMany(p => p.ThanhToanTams)
                .HasForeignKey(d => d.DonHangId)
                .HasConstraintName("FK_ThanhToanTam_DonHang");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.ThanhToanTams)
                .HasForeignKey(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhToanTam_TaiKhoan");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ThongBao_pkey");

            entity.ToTable("ThongBao");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DaXem).HasDefaultValue(false);
            entity.Property(e => e.NgayTao).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TaiKhoanId).HasColumnName("TaiKhoanID");
            entity.Property(e => e.TieuDe).HasMaxLength(255);

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThongBao_TaiKhoan");
        });

        modelBuilder.Entity<TinTuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TinTuc_pkey");

            entity.ToTable("TinTuc");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.NgaySua).HasColumnType("timestamp without time zone");
            entity.Property(e => e.NgayTao).HasColumnType("timestamp without time zone");
            entity.Property(e => e.TieuDe).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
