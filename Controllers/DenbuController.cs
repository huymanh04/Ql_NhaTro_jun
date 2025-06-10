using Api_Ql_nhatro.Controllers;
using Microsoft.AspNetCore.Mvc;
using Ql_NhaTro_jun.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DenbuController : ControllerBase
    {

        private readonly ILogger<DenbuController> _logger;
        QlNhatroContext _context;
        public DenbuController(ILogger<DenbuController> logger, QlNhatroContext cc)
        {
            _logger = logger; _context = cc;
        }
        public class CompensationDto
        {
            public int MaDenBu { get; set; }          // MaDenBu
            public int MaHopDong { get; set; }              // MaHopDong
            public string NoiDung { get; set; }              // NoiDung
            public decimal SoTien { get; set; }              // SoTien
            public DateTime NgayTao { get; set; }        // NgayTao
        }

    }
}
