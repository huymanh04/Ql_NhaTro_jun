using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ql_NhaTro_jun.Models;

public partial class HoaDonTong
{
    public int MaHoaDon { get; set; }

    public int? MaHopDong { get; set; }

    public DateOnly? NgayXuat { get; set; }

    public decimal? TongTien { get; set; }

    public string? GhiChu { get; set; }

    public virtual HopDong? MaHopDongNavigation { get; set; }
}
