using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ql_NhaTro_jun.Models;

public partial class DenBu
{
    public int MaDenBu { get; set; }

    public int? MaHopDong { get; set; }

    public string? NoiDung { get; set; }

    public decimal? SoTien { get; set; }

    [NotMapped]
    public byte[]? hinhanh { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual HopDong? MaHopDongNavigation { get; set; }
}
