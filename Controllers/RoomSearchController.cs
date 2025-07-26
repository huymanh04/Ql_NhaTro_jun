using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ql_NhaTro_jun.Controllers
{
    public class RoomSearchController : Controller
    {
        private readonly QlNhatroContext _context;
        public RoomSearchController(QlNhatroContext context)
        {
            _context = context;
        }

        // GET: /RoomSearch/Search?location=...&minPrice=...&maxPrice=...
        public async Task<IActionResult> Search(string location, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.PhongTros.Include(p => p.MaNhaTroNavigation).AsQueryable();
            if (!string.IsNullOrEmpty(location))
            {
                if (int.TryParse(location, out int maTinh))
                {
                    query = query.Where(p => p.MaNhaTroNavigation.MaTinh == maTinh);
                }
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Gia >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Gia <= maxPrice.Value);
            }
            var rooms = await query.ToListAsync();
            var roomIds = rooms.Select(r => r.MaPhong).ToList();
            var images = await _context.HinhAnhPhongTros
                .Where(i => roomIds.Contains(i.MaPhong) && i.IsMain == true)
                .ToListAsync();
            ViewBag.Images = images;
            ViewBag.Total = rooms.Count;
            return View(rooms);
        }
    }
} 