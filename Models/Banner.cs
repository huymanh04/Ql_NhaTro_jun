using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class Banner
{
    public int BannerId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Text { get; set; }

    public byte[]? ImageUrl { get; set; }

    public string? RedirectUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
