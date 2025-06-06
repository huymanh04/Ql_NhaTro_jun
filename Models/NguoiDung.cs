using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Ql_NhaTro_jun.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string? HoTen { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? MatKhau { get; set; }

    public string? VaiTro { get; set; }
    [NotMapped]
    [JsonPropertyName("g-recaptcha-response")]
    public string? RecaptchaResponse { get; set; }
    public virtual ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();

    public virtual ICollection<NhaTro> NhaTros { get; set; } = new List<NhaTro>();

    public virtual ICollection<TinNhan> TinNhanNguoiGuis { get; set; } = new List<TinNhan>();

    public virtual ICollection<TinNhan> TinNhanNguoiNhans { get; set; } = new List<TinNhan>();
}
