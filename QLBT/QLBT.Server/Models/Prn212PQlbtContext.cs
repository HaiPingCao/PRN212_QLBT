using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server= (local);uid=sa;password=123;database=PRN212_P_QLBT;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiNop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bai_nop__3213E83FF2940F34");

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
            entity.HasKey(e => e.Id).HasName("PK__bai_tap__3213E83F79C8CE57");

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
            entity.HasKey(e => e.Msgv).HasName("PK__giao_vie__763F3D5671CE486E");

            entity.ToTable("giao_vien");

            entity.HasIndex(e => e.Email, "UQ__giao_vie__AB6E61649D49751C").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__lop__3213E83FF6EBE777");

            entity.ToTable("lop");

            entity.HasIndex(e => new { e.TenLop, e.KiHoc, e.ChuyenNganh }, "uq_lop").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChuyenNganh)
                .HasMaxLength(100)
                .HasColumnName("chuyen_nganh");
            entity.Property(e => e.KiHoc)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ki_hoc");
            entity.Property(e => e.Msgv)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("msgv");
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
            entity.HasKey(e => e.Mssv).HasName("PK__sinh_vie__763F1CDD64B49FAB");

            entity.ToTable("sinh_vien");

            entity.HasIndex(e => e.Email, "UQ__sinh_vie__AB6E616403991CF1").IsUnique();

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
            entity.HasKey(e => new { e.LopId, e.Mssv }).HasName("PK__sinh_vie__4B357842C86C0932");

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
