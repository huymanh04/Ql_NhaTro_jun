using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class HinhAnhPhong
{
    public int MaHinhAnh { get; set; }

    public int MaPhong { get; set; } // ✅ Bắt buộc

    public byte[] DuongDanHinh { get; set; } = null!; // ✅ Dữ liệu ảnh dạng nhị phân, không null

    public bool IsMain { get; set; } = false; // ✅ Nên có trong entity

    public virtual PhongTro MaPhongNavigation { get; set; } = null!;
}

