using Api_Ql_nhatro.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [HttpGet("get-denbu")]
        public async Task<IActionResult> GetDenbu()
        {
            try
            {
                var denbus = await _context.DenBus
      .Select(d => new CompensationDto
      {
          MaDenBu = d.MaDenBu,
          MaHopDong = d.MaHopDong ?? 0,
          NoiDung = d.NoiDung,
          SoTien = d.SoTien ?? 0,
          NgayTao = d.NgayTao ?? default(DateTime) 
      })
      .ToListAsync();


                return Ok(ApiResponse<List<CompensationDto>>.CreateSuccess(
                    "Lấy danh sách đền bù thành công",
                    denbus
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đền bù");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách đền bù"
                ));
            }
        }
        [HttpPost("add-denbu")]
        public async Task<IActionResult> CreateDenbu([FromBody] CompensationCreateDto model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var denbu = new DenBu
                {
                    MaHopDong = model.MaHopDong,
                    NoiDung = model.NoiDung,
                    SoTien = model.SoTien,
                    NgayTao = DateTime.Now
                };

                _context.DenBus.Add(denbu);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<CompensationDto>.CreateSuccess(
                    "Thêm mới đền bù thành công",
                    new CompensationDto
                    {
                        MaDenBu = denbu.MaDenBu,
                        MaHopDong = denbu.MaHopDong ?? 0,
                        NoiDung = denbu.NoiDung,
                        SoTien = denbu.SoTien ?? 0,
                        NgayTao = denbu.NgayTao ?? default(DateTime)
                    }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm mới đền bù");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm mới đền bù"
                ));
            }
        }
        [HttpPut("edit-denbu/{id}")]
        public async Task<IActionResult> UpdateDenbu(int id, [FromBody] CompensationCreateDto model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var denbu = await _context.DenBus.FindAsync(id);
                if (denbu == null)
                    return NotFound(ApiResponse<object>.CreateError("Đền bù không tồn tại"));

                denbu.MaHopDong = model.MaHopDong;
                denbu.NoiDung = model.NoiDung;
                denbu.SoTien = model.SoTien;

                _context.DenBus.Update(denbu);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<CompensationDto>.CreateSuccess(
                    "Cập nhật đền bù thành công",
                    new CompensationDto
                    {
                        MaDenBu = denbu.MaDenBu,
                        MaHopDong = denbu.MaHopDong ?? 0,
                        NoiDung = denbu.NoiDung,
                        SoTien = denbu.SoTien ?? 0,
                        NgayTao = denbu.NgayTao ?? default(DateTime)
                    }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật đền bù");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi cập nhật đền bù"
                ));
            }
        }
        [HttpDelete("delete-denbu/{id}")]
        public async Task<IActionResult> DeleteDenbu(int id)
        {
            try
            {
                var denbu = await _context.DenBus.FindAsync(id);
                if (denbu == null)
                    return NotFound(ApiResponse<object>.CreateError("Đền bù không tồn tại"));

                _context.DenBus.Remove(denbu);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa đền bù thành công",
                    null
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa đền bù");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa đền bù"
                ));
            }
        }
        public class CompensationCreateDto
        {
            public int MaHopDong { get; set; }              // MaHopDong
            public string NoiDung { get; set; }              // NoiDung
            public decimal SoTien { get; set; }              // SoTien
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
