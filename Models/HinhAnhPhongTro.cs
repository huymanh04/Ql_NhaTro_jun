using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class HinhAnhPhongTro
{
    public int MaHinhAnh { get; set; }

    public int MaPhong { get; set; }

    public byte[] DuongDanHinh { get; set; } = null!;

    public bool? IsMain { get; set; }

    public virtual PhongTro MaPhongNavigation { get; set; } = null!;
}
