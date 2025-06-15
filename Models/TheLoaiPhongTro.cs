using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class TheLoaiPhongTro
{
    public int MaTheLoai { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public string? MoTa { get; set; }

    public byte[]? ImageUrl { get; set; }

    public string? RedirectUrl { get; set; }

    public virtual ICollection<PhongTro> PhongTros { get; set; } = new List<PhongTro>();
}
