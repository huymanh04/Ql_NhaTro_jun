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

    public int SoXe { get; set; }

    public virtual ICollection<DenBu> DenBus { get; set; } = new List<DenBu>();

    public virtual ICollection<HoaDonTong> HoaDonTongs { get; set; } = new List<HoaDonTong>();

    public virtual ICollection<HopDongNguoiThue> HopDongNguoiThues { get; set; } = new List<HopDongNguoiThue>();

    public virtual ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();

    public virtual PhongTro? MaPhongNavigation { get; set; }
}
