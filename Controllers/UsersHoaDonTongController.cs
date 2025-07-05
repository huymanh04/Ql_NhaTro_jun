using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Ql_NhaTro_jun.Controllers
{
    public class UsersHoaDonTongController : Controller
    {
        private readonly QlNhatroContext _context;
        public UsersHoaDonTongController(QlNhatroContext context)
        {
            _context = context;
        }

        // Trang danh sách hóa đơn tổng
        public async Task<IActionResult> Index()
        {
            return View(); // View: Views/Users/HoaDonTong.cshtml
        }

        // API lấy danh sách hóa đơn tổng (dùng cho JS fetch)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.HoaDonTongs
                .Select(h => new HoaDonTongDTO
                {
                    MaHoaDon = h.MaHoaDon,
                    MaHopDong = h.MaHopDong ?? 0,
                    NgayXuat = h.NgayXuat.HasValue ? h.NgayXuat.Value.ToDateTime(TimeOnly.MinValue) : default,
                    TongTien = h.TongTien ?? 0,
                    GhiChu = h.GhiChu
                })
                .OrderByDescending(x => x.NgayXuat)
                .ToListAsync();
            return Json(new { success = true, data = list });
        }

        // API lấy chi tiết hóa đơn tổng theo id
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var h = await _context.HoaDonTongs
                .Where(x => x.MaHoaDon == id)
                .Select(hd => new HoaDonTongDTO
                {
                    MaHoaDon = hd.MaHoaDon,
                    MaHopDong = hd.MaHopDong ?? 0,
                    NgayXuat = hd.NgayXuat.HasValue ? hd.NgayXuat.Value.ToDateTime(TimeOnly.MinValue) : default,
                    TongTien = hd.TongTien ?? 0,
                    GhiChu = hd.GhiChu
                })
                .FirstOrDefaultAsync();
            if (h == null) return Json(new { success = false });
            return Json(new { success = true, data = h });
        }

        public class HoaDonTongDTO
        {
            public int MaHoaDon { get; set; }
            public int MaHopDong { get; set; }
            public DateTime NgayXuat { get; set; }
            public decimal TongTien { get; set; }
            public string GhiChu { get; set; }
        }
    }
} 