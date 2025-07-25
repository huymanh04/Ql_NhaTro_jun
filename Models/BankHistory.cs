using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class BankHistory
{
    public int HistoryId { get; set; }

    public int MaPhong { get; set; }

    public decimal Amount { get; set; }

    public string? BankName { get; set; }

    public string? TransactionCode { get; set; }

    public string? Note { get; set; }
    public string? Phuong_thuc { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual PhongTro MaPhongNavigation { get; set; } = null!;
}
