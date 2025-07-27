using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class DenBu
{
    public int MaDenBu { get; set; }

    public int? MaHopDong { get; set; }

    public string? NoiDung { get; set; }

    public decimal? SoTien { get; set; }
    public byte[] hinhanh { get; set; } = null!;

    public DateTime? NgayTao { get; set; }

    public virtual HopDong? MaHopDongNavigation { get; set; }
}
