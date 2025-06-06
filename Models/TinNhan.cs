using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class TinNhan
{
    public int MaTinNhan { get; set; }

    public int? MaPhong { get; set; }

    public int? NguoiGuiId { get; set; }

    public int? NguoiNhanId { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? ThoiGianGui { get; set; }

    public bool? DaXem { get; set; }

    public virtual PhongTro? MaPhongNavigation { get; set; }

    public virtual NguoiDung? NguoiGui { get; set; }

    public virtual NguoiDung? NguoiNhan { get; set; }
}
