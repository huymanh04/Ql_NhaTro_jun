using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class LichSuThanhToan
{
    public int MaThanhToan { get; set; }

    public int? MaHopDong { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public decimal? SoTien { get; set; }

    public string? PhuongThuc { get; set; }

    public string? GhiChu { get; set; }

    public virtual HopDong? MaHopDongNavigation { get; set; }
}
