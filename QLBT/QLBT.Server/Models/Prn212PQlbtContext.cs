using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Collections.Generic;

namespace QLBT.Server.Models;

public partial class Prn212PQlbtContext : DbContext
{
    public Prn212PQlbtContext()
    {
    }

    public Prn212PQlbtContext(DbContextOptions<Prn212PQlbtContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiNop> BaiNops { get; set; }

    public virtual DbSet<BaiTap> BaiTaps { get; set; }

    public virtual DbSet<GiaoVien> GiaoViens { get; set; }

    public virtual DbSet<Lop> Lops { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<SinhVienLop> SinhVienLops { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        var configuration = builder.Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("Default"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiNop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bai_nop__3213E83FCEF317B0");

            entity.ToTable("bai_nop");

            entity.HasIndex(e => new { e.BaiTapId, e.Mssv }, "uq_bai_nop").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BaiTapId).HasColumnName("bai_tap_id");
            entity.Property(e => e.DuongDanFile)
                .HasMaxLength(500)
                .HasColumnName("duong_dan_file");
            entity.Property(e => e.LopId).HasColumnName("lop_id");
            entity.Property(e => e.Mssv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("mssv");
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_nop");
            entity.Property(e => e.SoLanNop)
                .HasDefaultValue(1)
                .HasColumnName("so_lan_nop");
            entity.Property(e => e.TenFile)
                .HasMaxLength(255)
                .HasColumnName("ten_file");

            entity.HasOne(d => d.BaiTap).WithMany(p => p.BaiNops)
                .HasPrincipalKey(p => new { p.Id, p.LopId })
                .HasForeignKey(d => new { d.BaiTapId, d.LopId })
                .HasConstraintName("fk_bai_nop_bai_tap");

            entity.HasOne(d => d.SinhVienLop).WithMany(p => p.BaiNops)
                .HasForeignKey(d => new { d.LopId, d.Mssv })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bai_nop_sinh_vien_lop");
        });

        modelBuilder.Entity<BaiTap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bai_tap__3213E83F1606B509");

            entity.ToTable("bai_tap");

            entity.HasIndex(e => new { e.Id, e.LopId }, "uq_bai_tap_lop").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DuongDanFileDe)
                .HasMaxLength(500)
                .HasColumnName("duong_dan_file_de");
            entity.Property(e => e.HanNop)
                .HasColumnType("datetime")
                .HasColumnName("han_nop");
            entity.Property(e => e.LopId).HasColumnName("lop_id");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TenFileDe)
                .HasMaxLength(255)
                .HasColumnName("ten_file_de");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(255)
                .HasColumnName("tieu_de");

            entity.HasOne(d => d.Lop).WithMany(p => p.BaiTaps)
                .HasForeignKey(d => d.LopId)
                .HasConstraintName("fk_bai_tap_lop");
        });

        modelBuilder.Entity<GiaoVien>(entity =>
        {
            entity.HasKey(e => e.Msgv).HasName("PK__giao_vie__763F3D562EE70694");

            entity.ToTable("giao_vien");

            entity.HasIndex(e => e.Email, "UQ__giao_vie__AB6E61644B9C60C5").IsUnique();

            entity.Property(e => e.Msgv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("msgv");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("mat_khau");
        });

        modelBuilder.Entity<Lop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lop__3213E83FF903EEB5");

            entity.ToTable("lop");

            entity.HasIndex(e => new { e.TenLop, e.NienKhoa, e.ChuyenNganh }, "uq_lop").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChuyenNganh)
                .HasMaxLength(100)
                .HasColumnName("chuyen_nganh");
            entity.Property(e => e.Msgv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("msgv");
            entity.Property(e => e.NienKhoa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nien_khoa");
            entity.Property(e => e.TenLop)
                .HasMaxLength(50)
                .HasColumnName("ten_lop");

            entity.HasOne(d => d.MsgvNavigation).WithMany(p => p.Lops)
                .HasForeignKey(d => d.Msgv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lop_giao_vien");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.Mssv).HasName("PK__sinh_vie__763F1CDD254C164A");

            entity.ToTable("sinh_vien");

            entity.HasIndex(e => e.Email, "UQ__sinh_vie__AB6E6164FBA47FE3").IsUnique();

            entity.Property(e => e.Mssv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("mssv");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("mat_khau");
        });

        modelBuilder.Entity<SinhVienLop>(entity =>
        {
            entity.HasKey(e => new { e.LopId, e.Mssv }).HasName("PK__sinh_vie__4B357842C6FEB2E9");

            entity.ToTable("sinh_vien_lop");

            entity.Property(e => e.LopId).HasColumnName("lop_id");
            entity.Property(e => e.Mssv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("mssv");
            entity.Property(e => e.DangThamGia)
                .HasDefaultValue(true)
                .HasColumnName("dang_tham_gia");
            entity.Property(e => e.NgayRoiLop)
                .HasColumnType("datetime")
                .HasColumnName("ngay_roi_lop");
            entity.Property(e => e.NgayThamGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("ngay_tham_gia");

            entity.HasOne(d => d.Lop).WithMany(p => p.SinhVienLops)
                .HasForeignKey(d => d.LopId)
                .HasConstraintName("fk_svl_lop");

            entity.HasOne(d => d.MssvNavigation).WithMany(p => p.SinhVienLops)
                .HasForeignKey(d => d.Mssv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_svl_sinh_vien");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
