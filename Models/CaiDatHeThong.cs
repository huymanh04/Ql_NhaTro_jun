using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class CaiDatHeThong
{
    public int CaiDatId { get; set; }

    public string? CheDoGiaoDien { get; set; }

    public byte[]? LogoUrl { get; set; }

    public string? TieuDeWeb { get; set; }

    public string? DiaChi { get; set; }

    public string? SoDienThoai { get; set; }

    public string? GoogleMapEmbed { get; set; }

    public string? AiApikey { get; set; }

    public string? MoTaThem { get; set; }
}
