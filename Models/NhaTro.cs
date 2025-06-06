using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ql_NhaTro_jun.Models;

public partial class NhaTro
{
    public int MaNhaTro { get; set; }

    public int? MaChuTro { get; set; }

    public string? TenNhaTro { get; set; }

    public string? DiaChi { get; set; }

    public int? MaTinh { get; set; }

    public int? MaKhuVuc { get; set; }

    public string? MoTa { get; set; }
  

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung? MaChuTroNavigation { get; set; }

    public virtual KhuVuc? MaKhuVucNavigation { get; set; }

    public virtual TinhThanh? MaTinhNavigation { get; set; }

    public virtual ICollection<PhongTro> PhongTros { get; set; } = new List<PhongTro>();
}
