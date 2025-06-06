using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class PhongTro
{
    public int MaPhong { get; set; }

    public int? MaNhaTro { get; set; }

    public int MaTheLoai { get; set; }

    public string? TenPhong { get; set; }

    public decimal? Gia { get; set; }

    public double? DienTich { get; set; }

    public bool? ConTrong { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<HinhAnhPhong> HinhAnhPhongs { get; set; } = new List<HinhAnhPhong>();

    public virtual ICollection<HoaDonTienIch> HoaDonTienIches { get; set; } = new List<HoaDonTienIch>();

    public virtual ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();

    public virtual NhaTro? MaNhaTroNavigation { get; set; }

    public virtual TheLoaiPhongTro MaTheLoaiNavigation { get; set; } = null!;

    public virtual ICollection<TinNhan> TinNhans { get; set; } = new List<TinNhan>();
}
