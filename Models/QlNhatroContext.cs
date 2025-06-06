using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Ql_NhaTro_jun.Models;

public partial class QlNhatroContext : DbContext
{
    public QlNhatroContext()
    {
    }

    public QlNhatroContext(DbContextOptions<QlNhatroContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<CaiDatHeThong> CaiDatHeThongs { get; set; }

    public virtual DbSet<DenBu> DenBus { get; set; }

    public virtual DbSet<HinhAnhPhong> HinhAnhPhongs { get; set; }

    public virtual DbSet<HoaDonTienIch> HoaDonTienIches { get; set; }

    public virtual DbSet<HoaDonTong> HoaDonTongs { get; set; }

    public virtual DbSet<HopDong> HopDongs { get; set; }

    public virtual DbSet<KhuVuc> KhuVucs { get; set; }

    public virtual DbSet<LichSuThanhToan> LichSuThanhToans { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<NhaTro> NhaTros { get; set; }

    public virtual DbSet<PhongTro> PhongTros { get; set; }

    public virtual DbSet<TheLoaiPhongTro> TheLoaiPhongTros { get; set; }

    public virtual DbSet<TinNhan> TinNhans { get; set; }

    public virtual DbSet<TinhThanh> TinhThanhs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=18.141.93.248;Initial Catalog=QL_nhatro;User ID=Ql_tro1234;Password=Manh@2005;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.BannerId).HasName("PK__Banner__32E86AD1EF562E83");

            entity.ToTable("Banner");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<CaiDatHeThong>(entity =>
        {
            entity.HasKey(e => e.CaiDatId).HasName("PK__CaiDatHe__C4138FB5426EC27D");

            entity.ToTable("CaiDatHeThong");

            entity.Property(e => e.CaiDatId).HasColumnName("CaiDatID");
            entity.Property(e => e.AiApikey)
                .HasMaxLength(255)
                .HasColumnName("AI_APIKey");
            entity.Property(e => e.CheDoGiaoDien)
                .HasMaxLength(20)
                .HasDefaultValue("Light");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TieuDeWeb).HasMaxLength(100);
        });

        modelBuilder.Entity<DenBu>(entity =>
        {
            entity.HasKey(e => e.MaDenBu).HasName("PK__DenBu__95550040DAC1DB3C");

            entity.ToTable("DenBu");

            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.DenBus)
                .HasForeignKey(d => d.MaHopDong)
                .HasConstraintName("FK__DenBu__MaHopDong__52593CB8");
        });

        modelBuilder.Entity<HinhAnhPhong>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh);

            entity.ToTable("HinhAnhPhongTro"); // ✅ Trùng tên trong SQL

            entity.Property(e => e.DuongDanHinh)
                .IsRequired();

            entity.Property(e => e.IsMain)
                .HasDefaultValue(false);

            entity.HasOne(e => e.MaPhongNavigation)
                .WithMany(p => p.HinhAnhPhongs)
                .HasForeignKey(e => e.MaPhong)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HinhAnhPhongTro_PhongTro");
        });

        modelBuilder.Entity<HoaDonTienIch>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDonTi__835ED13BDB7D87A6");

            entity.ToTable("HoaDonTienIch");

            entity.Property(e => e.DaThanhToan).HasDefaultValue(false);
            entity.Property(e => e.DonGiaDien).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DonGiaNuoc).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.HoaDonTienIches)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__HoaDonTie__MaPho__5629CD9C");
        });

        modelBuilder.Entity<HoaDonTong>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDonTo__835ED13B93F03B92");

            entity.ToTable("HoaDonTong");

            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.HoaDonTongs)
                .HasForeignKey(d => d.MaHopDong)
                .HasConstraintName("FK__HoaDonTon__MaHop__59FA5E80");
        });

        modelBuilder.Entity<HopDong>(entity =>
        {
            entity.HasKey(e => e.MaHopDong).HasName("PK__HopDong__36DD4342F621D924");

            entity.ToTable("HopDong");

            entity.Property(e => e.DaKetThuc).HasDefaultValue(false);
            entity.Property(e => e.SoNguoiO).HasDefaultValue(1);
            entity.Property(e => e.TienDatCoc).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaKhachThueNavigation).WithMany(p => p.HopDongs)
                .HasForeignKey(d => d.MaKhachThue)
                .HasConstraintName("FK__HopDong__MaKhach__4D94879B");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.HopDongs)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__HopDong__MaPhong__4CA06362");
        });

        modelBuilder.Entity<KhuVuc>(entity =>
        {
            entity.HasKey(e => e.MaKhuVuc).HasName("PK__KhuVuc__0676EB837BE03632");

            entity.ToTable("KhuVuc");

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenKhuVuc).HasMaxLength(100);

            entity.HasOne(d => d.MaTinhNavigation).WithMany(p => p.KhuVucs)
                .HasForeignKey(d => d.MaTinh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhuVuc__MaTinh__3A81B327");
        });

        modelBuilder.Entity<LichSuThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__LichSuTh__D4B258444C9392E3");

            entity.ToTable("LichSuThanhToan");

            entity.Property(e => e.NgayThanhToan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThuc).HasMaxLength(50);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.LichSuThanhToans)
                .HasForeignKey(d => d.MaHopDong)
                .HasConstraintName("FK__LichSuTha__MaHop__68487DD7");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D76251E75FA6");

            entity.ToTable("NguoiDung");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        modelBuilder.Entity<NhaTro>(entity =>
        {
            entity.HasKey(e => e.MaNhaTro).HasName("PK__NhaTro__DCE96C16CEAC0AF2");

            entity.ToTable("NhaTro");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenNhaTro).HasMaxLength(100);

            entity.HasOne(d => d.MaChuTroNavigation).WithMany(p => p.NhaTros)
                .HasForeignKey(d => d.MaChuTro)
                .HasConstraintName("FK__NhaTro__MaChuTro__3D5E1FD2");

            entity.HasOne(d => d.MaKhuVucNavigation).WithMany(p => p.NhaTros)
                .HasForeignKey(d => d.MaKhuVuc)
                .HasConstraintName("FK__NhaTro__MaKhuVuc__3F466844");

            entity.HasOne(d => d.MaTinhNavigation).WithMany(p => p.NhaTros)
                .HasForeignKey(d => d.MaTinh)
                .HasConstraintName("FK__NhaTro__MaTinh__3E52440B");
        });

        modelBuilder.Entity<PhongTro>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__PhongTro__20BD5E5BF320B6AD");

            entity.ToTable("PhongTro");

            entity.Property(e => e.ConTrong).HasDefaultValue(true);
            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenPhong).HasMaxLength(100);

            entity.HasOne(d => d.MaNhaTroNavigation).WithMany(p => p.PhongTros)
                .HasForeignKey(d => d.MaNhaTro)
                .HasConstraintName("FK__PhongTro__MaNhaT__44FF419A");

            entity.HasOne(d => d.MaTheLoaiNavigation).WithMany(p => p.PhongTros)
                .HasForeignKey(d => d.MaTheLoai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhongTro__MoTa__46E78A0C");
        });

        modelBuilder.Entity<TheLoaiPhongTro>(entity =>
        {
            entity.HasKey(e => e.MaTheLoai).HasName("PK__TheLoaiP__D73FF34A38FA5815");

            entity.ToTable("TheLoaiPhongTro");

            entity.Property(e => e.TenTheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<TinNhan>(entity =>
        {
            entity.HasKey(e => e.MaTinNhan).HasName("PK__TinNhan__E5B3062A123D9FCF");

            entity.ToTable("TinNhan");

            entity.Property(e => e.DaXem).HasDefaultValue(false);
            entity.Property(e => e.NguoiGuiId).HasColumnName("NguoiGuiID");
            entity.Property(e => e.NguoiNhanId).HasColumnName("NguoiNhanID");
            entity.Property(e => e.ThoiGianGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.TinNhans)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__TinNhan__MaPhong__5EBF139D");

            entity.HasOne(d => d.NguoiGui).WithMany(p => p.TinNhanNguoiGuis)
                .HasForeignKey(d => d.NguoiGuiId)
                .HasConstraintName("FK__TinNhan__NguoiGu__5FB337D6");

            entity.HasOne(d => d.NguoiNhan).WithMany(p => p.TinNhanNguoiNhans)
                .HasForeignKey(d => d.NguoiNhanId)
                .HasConstraintName("FK__TinNhan__NguoiNh__60A75C0F");
        });

        modelBuilder.Entity<TinhThanh>(entity =>
        {
            entity.HasKey(e => e.MaTinh).HasName("PK__TinhThan__4CC544809AE09AE0");

            entity.ToTable("TinhThanh");

            entity.Property(e => e.TenTinh).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
