using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class Bank
{
    //hihi
    public int Id { get; set; }

    public string Ten { get; set; } = null!;

    public string SoTaiKhoan { get; set; } = null!;

    public string TenNganHang { get; set; } = null!;

    public string? GhiChu { get; set; }
}
