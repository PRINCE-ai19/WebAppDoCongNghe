using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAppDoCongNghe.Models.Entity;

public partial class WebAppDoCongNgheContext : DbContext
{
    public WebAppDoCongNgheContext()
    {
    }

    public WebAppDoCongNgheContext(DbContextOptions<WebAppDoCongNgheContext> options)
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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-NGC9U4DE;Database=WebAppDoCongNghe;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CauHinhSanPham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CauHinhS__3214EC07B84EC819");

            entity.HasOne(d => d.SanPham).WithMany(p => p.CauHinhSanPhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CauHinh_SanPham");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietD__3214EC278A912A9F");

            entity.HasOne(d => d.DonHang).WithMany(p => p.ChiTietDonHangs).HasConstraintName("FK__ChiTietDo__DonHa__4222D4EF");

            entity.HasOne(d => d.SanPham).WithMany(p => p.ChiTietDonHangs).HasConstraintName("FK__ChiTietDo__SanPh__4316F928");
        });

        modelBuilder.Entity<ChiTietGioHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChiTietG__3214EC27F700348E");

            entity.HasOne(d => d.GioHang).WithMany(p => p.ChiTietGioHangs).HasConstraintName("FK__ChiTietGi__GioHa__440B1D61");

            entity.HasOne(d => d.SanPham).WithMany(p => p.ChiTietGioHangs).HasConstraintName("FK__ChiTietGi__SanPh__44FF419A");
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhGia__3214EC2717BEC8FF");

            entity.HasOne(d => d.SanPham).WithMany(p => p.DanhGia).HasConstraintName("FK__DanhGia__SanPham__45F365D3");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.DanhGia).HasConstraintName("FK__DanhGia__TaiKhoa__46E78A0C");
        });

        modelBuilder.Entity<DanhMuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhMuc__3214EC2715DB05BE");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DonHang__3214EC271571A034");

            entity.HasOne(d => d.PhieuGiamGia).WithMany(p => p.DonHangs).HasConstraintName("FK_DonHang_PhieuGiamGia");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.DonHangs).HasConstraintName("FK__DonHang__TaiKhoa__47DBAE45");
        });

        modelBuilder.Entity<GiaoHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GiaoHang__3214EC270CA64F2A");

            entity.HasOne(d => d.DonHang).WithMany(p => p.GiaoHangs).HasConstraintName("FK__GiaoHang__DonHan__48CFD27E");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GioHang__3214EC27507B5154");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.GioHangs).HasConstraintName("FK__GioHang__TaiKhoa__49C3F6B7");
        });

        modelBuilder.Entity<HinhAnhSanPham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HinhAnhS__3214EC2752736E1D");

            entity.HasOne(d => d.SanPham).WithMany(p => p.HinhAnhSanPhams).HasConstraintName("FK_HinhAnhSanPham_SanPham");
        });

        modelBuilder.Entity<KhuyenMai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhuyenMa__3214EC274E4E1AB0");
        });

        modelBuilder.Entity<LienHe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LienHe__3214EC27C086C4B1");

            entity.Property(e => e.NgayGui).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<LoaiTaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoaiTaiK__3214EC274CBDB9E6");
        });

        modelBuilder.Entity<PhanQuyen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PhanQuye__3214EC073E79928D");
        });

        modelBuilder.Entity<PhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PhieuGia__3214EC27DEF40ADA");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanPham__3214EC2798958DB8");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.SanPhams).HasConstraintName("FK__SanPham__DanhMuc__4BAC3F29");
        });

        modelBuilder.Entity<SanPhamKhuyenMai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanPham___3214EC27028AF96A");

            entity.HasOne(d => d.KhuyenMai).WithMany(p => p.SanPhamKhuyenMais).HasConstraintName("FK__SanPham_K__Khuye__4CA06362");

            entity.HasOne(d => d.SanPham).WithMany(p => p.SanPhamKhuyenMais).HasConstraintName("FK__SanPham_K__SanPh__4D94879B");
        });

        modelBuilder.Entity<SanPhamYeuThich>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanPhamY__3214EC272EF53965");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.SanPham).WithMany(p => p.SanPhamYeuThiches).HasConstraintName("FK_YeuThich_SanPham");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.SanPhamYeuThiches).HasConstraintName("FK_YeuThich_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaiKhoan__3214EC275E403E2F");

            entity.HasOne(d => d.IdLoaiTaiKhoanNavigation).WithMany(p => p.TaiKhoans).HasConstraintName("FK__TaiKhoan__IdLoai__4E88ABD4");
        });

        modelBuilder.Entity<TaiKhoanPhanQuyen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaiKhoan__3214EC071AED5285");

            entity.Property(e => e.QuyenSua).HasDefaultValue(false);
            entity.Property(e => e.QuyenThem).HasDefaultValue(false);
            entity.Property(e => e.QuyenXem).HasDefaultValue(false);
            entity.Property(e => e.QuyenXoa).HasDefaultValue(false);

            entity.HasOne(d => d.IdChucNangNavigation).WithMany(p => p.TaiKhoanPhanQuyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TK_PQ_ChucNang");

            entity.HasOne(d => d.IdTaiKhoanNavigation).WithMany(p => p.TaiKhoanPhanQuyens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TK_PQ_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoanPhieuGiamGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaiKhoan__3214EC27EB17F0AF");

            entity.Property(e => e.DaSuDung).HasDefaultValue(false);
            entity.Property(e => e.NgayNhan).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PhieuGiamGia).WithMany(p => p.TaiKhoanPhieuGiamGia).HasConstraintName("FK_TaiKhoanPhieuGiamGia_PhieuGiamGia");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.TaiKhoanPhieuGiamGia).HasConstraintName("FK_TaiKhoanPhieuGiamGia_TaiKhoan");
        });

        modelBuilder.Entity<ThanhToanTam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThanhToa__3214EC27514E0DAB");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.DonHang).WithMany(p => p.ThanhToanTams).HasConstraintName("FK_ThanhToanTam_DonHang");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.ThanhToanTams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhToanTam_TaiKhoan");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ThongBao__3214EC279D086805");

            entity.Property(e => e.DaXem).HasDefaultValue(false);

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.ThongBaos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ThongBao__TaiKho__5165187F");
        });

        modelBuilder.Entity<TinTuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TinTuc__3214EC279C7DA153");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
