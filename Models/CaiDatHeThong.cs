using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ql_NhaTro_jun.Models;

public partial class CaiDatHeThong
{
    public int CaiDatId { get; set; }

    public string? CheDoGiaoDien { get; set; }

    public byte[]? LogoUrl { get; set; }

    public string? TieuDeWeb { get; set; }

    public string? DiaChi { get; set; }

    public decimal? TienDien { get; set; }

    public decimal? TienNuoc { get; set; }

    public decimal? Phidv { get; set; }

    public decimal? PhiGiuXe { get; set; }

    public string? SoDienThoai { get; set; }

    public string? GoogleMapEmbed { get; set; }

    public string? AiApikey { get; set; }

    public string? MoTaThem { get; set; }
}
