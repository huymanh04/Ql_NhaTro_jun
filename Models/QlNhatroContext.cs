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

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<CaiDatHeThong> CaiDatHeThongs { get; set; }

    public virtual DbSet<DenBu> DenBus { get; set; }

    public virtual DbSet<HinhAnhPhongTro> HinhAnhPhongTros { get; set; }

    public virtual DbSet<HoaDonTienIch> HoaDonTienIches { get; set; }

    public virtual DbSet<HoaDonTong> HoaDonTongs { get; set; }

    public virtual DbSet<HopDong> HopDongs { get; set; }

    public virtual DbSet<HopDongNguoiThue> HopDongNguoiThues { get; set; }

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
        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bank__3213E83F554E260B");

            entity.ToTable("Bank");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SoTaiKhoan).HasMaxLength(100);
            entity.Property(e => e.Ten).HasMaxLength(50);
            entity.Property(e => e.TenNganHang).HasMaxLength(50);
        });

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.BannerId).HasName("PK__Banner__32E86AD1959C2046");

            entity.ToTable("Banner");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<CaiDatHeThong>(entity =>
        {
            entity.HasKey(e => e.CaiDatId).HasName("PK__CaiDatHe__C4138FB5AEB98718");

            entity.ToTable("CaiDatHeThong");

            entity.Property(e => e.CaiDatId).HasColumnName("CaiDatID");
            entity.Property(e => e.AiApikey)
                .HasMaxLength(255)
                .HasColumnName("AI_APIKey");
            entity.Property(e => e.CheDoGiaoDien)
                .HasMaxLength(20)
                .HasDefaultValue("Light");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.PhiGiuXe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Phidv).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.TienDien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TienNuoc).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TieuDeWeb).HasMaxLength(100);
        });

        modelBuilder.Entity<DenBu>(entity =>
        {
            entity.HasKey(e => e.MaDenBu).HasName("PK__DenBu__955500401188FA2F");

            entity.ToTable("DenBu");

            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.DenBus)
                .HasForeignKey(d => d.MaHopDong)
                .HasConstraintName("FK__DenBu__MaHopDong__412EB0B6");
        });

        modelBuilder.Entity<HinhAnhPhongTro>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HinhAnhP__A9C37A9B62B90C06");

            entity.ToTable("HinhAnhPhongTro");

            entity.Property(e => e.IsMain).HasDefaultValue(false);

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.HinhAnhPhongTros)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HinhAnhPhongTro_PhongTro");
        });

        modelBuilder.Entity<HoaDonTienIch>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDonTi__835ED13B39122E39");

            entity.ToTable("HoaDonTienIch");

            entity.Property(e => e.DaThanhToan).HasDefaultValue(false);
            entity.Property(e => e.DonGiaDien).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DonGiaNuoc).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.HoaDonTienIches)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__HoaDonTie__MaPho__44FF419A");
        });

        modelBuilder.Entity<HoaDonTong>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDonTo__835ED13BEFFEE805");

            entity.ToTable("HoaDonTong");

            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.HoaDonTongs)
                .HasForeignKey(d => d.MaHopDong)
                .HasConstraintName("FK__HoaDonTon__MaHop__48CFD27E");
        });

        modelBuilder.Entity<HopDong>(entity =>
        {
            entity.HasKey(e => e.MaHopDong).HasName("PK__HopDong__36DD4342FD62444E");

            entity.ToTable("HopDong");

            entity.Property(e => e.DaKetThuc).HasDefaultValue(false);
            entity.Property(e => e.SoNguoiO).HasDefaultValue(1);
            entity.Property(e => e.TienDatCoc).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.HopDongs)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__HopDong__MaPhong__3B75D760");
        });

        modelBuilder.Entity<HopDongNguoiThue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HopDongN__3214EC079C6DF5EA");

            entity.ToTable("HopDongNguoiThue");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.HopDongNguoiThues)
                .HasForeignKey(d => d.MaHopDong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HopDongNg__MaHop__6383C8BA");

            entity.HasOne(d => d.MaKhachThueNavigation).WithMany(p => p.HopDongNguoiThues)
                .HasForeignKey(d => d.MaKhachThue)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HopDongNg__MaKha__6477ECF3");
        });

        modelBuilder.Entity<KhuVuc>(entity =>
        {
            entity.HasKey(e => e.MaKhuVuc).HasName("PK__KhuVuc__0676EB8333E54FD9");

            entity.ToTable("KhuVuc");

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenKhuVuc).HasMaxLength(100);

            entity.HasOne(d => d.MaTinhNavigation).WithMany(p => p.KhuVucs)
                .HasForeignKey(d => d.MaTinh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhuVuc__MaTinh__286302EC");
        });

        modelBuilder.Entity<LichSuThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__LichSuTh__D4B2584425B04C05");

            entity.ToTable("LichSuThanhToan");

            entity.Property(e => e.NgayThanhToan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThuc).HasMaxLength(50);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHopDongNavigation).WithMany(p => p.LichSuThanhToans)
                .HasForeignKey(d => d.MaHopDong)
                .HasConstraintName("FK__LichSuTha__MaHop__571DF1D5");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK__NguoiDun__C539D7622D9FB67D");

            entity.ToTable("NguoiDung");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.VaiTro).HasMaxLength(20);
        });

        modelBuilder.Entity<NhaTro>(entity =>
        {
            entity.HasKey(e => e.MaNhaTro).HasName("PK__NhaTro__DCE96C16A01179F2");

            entity.ToTable("NhaTro");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenNhaTro).HasMaxLength(100);

            entity.HasOne(d => d.MaChuTroNavigation).WithMany(p => p.NhaTros)
                .HasForeignKey(d => d.MaChuTro)
                .HasConstraintName("FK__NhaTro__MaChuTro__2B3F6F97");

            entity.HasOne(d => d.MaKhuVucNavigation).WithMany(p => p.NhaTros)
                .HasForeignKey(d => d.MaKhuVuc)
                .HasConstraintName("FK__NhaTro__MaKhuVuc__2D27B809");

            entity.HasOne(d => d.MaTinhNavigation).WithMany(p => p.NhaTros)
                .HasForeignKey(d => d.MaTinh)
                .HasConstraintName("FK__NhaTro__MaTinh__2C3393D0");
        });

        modelBuilder.Entity<PhongTro>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__PhongTro__20BD5E5B4A742B72");

            entity.ToTable("PhongTro");

            entity.Property(e => e.ConTrong).HasDefaultValue(true);
            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenPhong).HasMaxLength(100);

            entity.HasOne(d => d.MaNhaTroNavigation).WithMany(p => p.PhongTros)
                .HasForeignKey(d => d.MaNhaTro)
                .HasConstraintName("FK__PhongTro__MaNhaT__32E0915F");

            entity.HasOne(d => d.MaTheLoaiNavigation).WithMany(p => p.PhongTros)
                .HasForeignKey(d => d.MaTheLoai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhongTro__MoTa__34C8D9D1");
        });

        modelBuilder.Entity<TheLoaiPhongTro>(entity =>
        {
            entity.HasKey(e => e.MaTheLoai).HasName("PK__TheLoaiP__D73FF34A01F86222");

            entity.ToTable("TheLoaiPhongTro");

            entity.Property(e => e.TenTheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<TinNhan>(entity =>
        {
            entity.HasKey(e => e.MaTinNhan).HasName("PK__TinNhan__E5B3062AF4DEF77D");

            entity.ToTable("TinNhan");

            entity.Property(e => e.DaXem).HasDefaultValue(false);
            entity.Property(e => e.NguoiGuiId).HasColumnName("NguoiGuiID");
            entity.Property(e => e.NguoiNhanId).HasColumnName("NguoiNhanID");
            entity.Property(e => e.ThoiGianGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.TinNhans)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__TinNhan__MaPhong__4D94879B");

            entity.HasOne(d => d.NguoiGui).WithMany(p => p.TinNhanNguoiGuis)
                .HasForeignKey(d => d.NguoiGuiId)
                .HasConstraintName("FK__TinNhan__NguoiGu__4E88ABD4");

            entity.HasOne(d => d.NguoiNhan).WithMany(p => p.TinNhanNguoiNhans)
                .HasForeignKey(d => d.NguoiNhanId)
                .HasConstraintName("FK__TinNhan__NguoiNh__4F7CD00D");
        });

        modelBuilder.Entity<TinhThanh>(entity =>
        {
            entity.HasKey(e => e.MaTinh).HasName("PK__TinhThan__4CC544808B653F41");

            entity.ToTable("TinhThanh");

            entity.Property(e => e.TenTinh).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
