using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class TinhThanh
{
    public int MaTinh { get; set; }

    public string TenTinh { get; set; } = null!;

    public virtual ICollection<KhuVuc> KhuVucs { get; set; } = new List<KhuVuc>();

    public virtual ICollection<NhaTro> NhaTros { get; set; } = new List<NhaTro>();
}
