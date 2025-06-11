using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;
        private readonly QlNhatroContext _context;
        public SearchController(ILogger<SearchController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
    

    }
}
