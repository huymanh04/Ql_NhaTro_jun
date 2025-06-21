using System.ComponentModel.DataAnnotations;

namespace Ql_NhaTro_jun.Models
{
    public class HoaDonTienIchModel
    {
        [Required]
        public int MaPhong { get; set; }

        [Required]
        public decimal SoDien { get; set; }

        [Required]
        public decimal SoNuoc { get; set; }
    }
} 