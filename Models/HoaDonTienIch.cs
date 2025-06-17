using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ql_NhaTro_jun.Models;

public partial class HoaDonTienIch
{
    public int MaHoaDon { get; set; }

    public int? MaPhong { get; set; }

    [NotMapped]
    public int Soxe { get; set; }
    [NotMapped]
    public decimal Phidv { get; set; }

    public int? Thang { get; set; }

    public int? Nam { get; set; }

    public double? SoDien { get; set; }

    public double? SoNuoc { get; set; }

    public decimal? DonGiaDien { get; set; }

    public decimal? DonGiaNuoc { get; set; }

    public decimal? TongTien { get; set; }

    public bool? DaThanhToan { get; set; }

    public virtual PhongTro? MaPhongNavigation { get; set; }
}
