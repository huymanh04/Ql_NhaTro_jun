using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class HopDongNguoiThue
{
    public int Id { get; set; }

    public int MaHopDong { get; set; }

    public int MaKhachThue { get; set; }

    public virtual HopDong MaHopDongNavigation { get; set; } = null!;

    public virtual NguoiDung MaKhachThueNavigation { get; set; } = null!;
}
