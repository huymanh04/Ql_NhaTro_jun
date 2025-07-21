using System;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Models;

public partial class BankHistory
{
    public int HistoryId { get; set; }

    public int MaNguoiDung { get; set; }

    public decimal Amount { get; set; }

    public string? BankName { get; set; }

    public string? TransactionCode { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
