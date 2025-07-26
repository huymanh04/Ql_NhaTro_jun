using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.Diagnostics;

namespace Ql_NhaTro_jun.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QlNhatroContext _context;

        public HomeController(ILogger<HomeController> logger, QlNhatroContext cc)
        {
            _logger = logger;_context = cc;
        }

        public async Task<IActionResult> Index()
        {
            var m = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
            JunTech.caidat = m;
            return View();
        } 
        
        public async Task<IActionResult> About()
        {
            // Nếu cần dùng dữ liệu cấu hình, truyền qua ViewBag
            // var m = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
            // ViewBag.CaiDat = m;
            return View();
        }
        
        public async Task<IActionResult> Contact()
        {
            var m = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
            JunTech.caidat = m;
            return View();
        }

        public async Task<IActionResult> Chatbot()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }

            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Users");
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
            if (user == null)
            {
                user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
            }

            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.CurrentUserId = user.MaNguoiDung;
            ViewBag.CurrentUserRole = user.VaiTro;

            if (user.VaiTro == "0") // Khách thuê
            {
                // Lấy hợp đồng mới nhất (không phân biệt còn hiệu lực)
                var hopDongNguoiThue = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                    .ThenInclude(h => h.MaPhongNavigation)
                    .ThenInclude(p => p.MaNhaTroNavigation)
                    .ThenInclude(n => n.MaChuTroNavigation)
                    .Where(h => h.MaKhachThue == user.MaNguoiDung)
                    .OrderByDescending(h => h.MaHopDongNavigation.NgayBatDau)
                    .FirstOrDefaultAsync();

                if (hopDongNguoiThue != null)
                {
                    var chuTro = hopDongNguoiThue.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.MaChuTroNavigation;
                    ViewBag.LandlordId = chuTro?.MaNguoiDung ?? 0;
                    ViewBag.LandlordName = chuTro?.HoTen ?? "";
                    ViewBag.MaHopDong = hopDongNguoiThue.MaHopDong;
                    return View(new List<NguoiDung> { chuTro });
                }
                else
                {
                    // Không có hợp đồng thuê trọ
                    ViewBag.LandlordId = 0;
                    ViewBag.LandlordName = "";
                    ViewBag.MaHopDong = 0;
                    return View(new List<NguoiDung>());
                }
            }
            else if (user.VaiTro == "1") // Chủ trọ
            {
                // Lấy tất cả khách thuê đã từng thuê (không phân biệt hợp đồng còn hiệu lực)
                var contracts = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                    .ThenInclude(h => h.MaPhongNavigation)
                    .ThenInclude(p => p.MaNhaTroNavigation)
                    .Include(h => h.MaKhachThueNavigation)
                    .Where(h => h.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.MaChuTro == user.MaNguoiDung)
                    .Select(h => new
                    {
                        RecipientId = h.MaKhachThue,
                        RecipientName = h.MaKhachThueNavigation.HoTen,
                        MaHopDong = h.MaHopDong,
                        RoomName = h.MaHopDongNavigation.MaPhongNavigation.TenPhong
                    })
                    .ToListAsync();

                ViewBag.Contracts = contracts;
                return View(contracts.Select(c => new NguoiDung 
                { 
                    MaNguoiDung = c.RecipientId, 
                    HoTen = c.RecipientName 
                }).ToList());
            }

            return View(new List<NguoiDung>());
        }
       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
