using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class KhuVuc
{
    public int MaKhuVuc { get; set; }

    public string TenKhuVuc { get; set; } = null!;

    public string? MoTa { get; set; }

    public int MaTinh { get; set; }

    public virtual TinhThanh MaTinhNavigation { get; set; } = null!;

    public virtual ICollection<NhaTro> NhaTros { get; set; } = new List<NhaTro>();
}
