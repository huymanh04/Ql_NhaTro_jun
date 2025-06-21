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
        }  public async Task< IActionResult> About()
        {

            return View(await _context.CaiDatHeThongs.FirstOrDefaultAsync());
        }
        public async Task<IActionResult> Contact()
        {
            var m = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
            JunTech.caidat = m;
            return View();
        }
       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
