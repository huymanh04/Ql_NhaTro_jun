using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class HopDong
{
    public int MaHopDong { get; set; }

    public int? MaPhong { get; set; }

    public int? MaKhachThue { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public int? SoNguoiO { get; set; }

    public decimal? TienDatCoc { get; set; }

    public bool? DaKetThuc { get; set; }

    public virtual ICollection<DenBu> DenBus { get; set; } = new List<DenBu>();

    public virtual ICollection<HoaDonTong> HoaDonTongs { get; set; } = new List<HoaDonTong>();

    public virtual ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();

    public virtual NguoiDung? MaKhachThueNavigation { get; set; }

    public virtual PhongTro? MaPhongNavigation { get; set; }
}
