﻿using Api_Ql_nhatro.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
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
                var denbus = await _context.DenBus.Include(d => d.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation)
      .Select(d => new CompensationDto
      {
          MaDenBu = d.MaDenBu,
          MaHopDong = d.MaHopDong ?? 0,
          NoiDung = d.NoiDung,
          SoTien = d.SoTien ?? 0,
          Nhatro=d.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
          NgayTao = d.NgayTao ?? default(DateTime) ,
          base64= d.hinhanh != null ? Convert.ToBase64String(d.hinhanh) : null

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
            [HttpGet("get-denbu-mahopdong/{id}")]
            public async Task<IActionResult> GetDenbuById(int id)
            {
                try
                {
                    var denbu = await _context.DenBus
                        .Where(d => d.MaHopDong == id)
                        .Select(d => new CompensationDto
                        {
                            MaDenBu = d.MaDenBu,
                            MaHopDong = d.MaHopDong ?? 0,
                            NoiDung = d.NoiDung,
                            SoTien = d.SoTien ?? 0,
                            NgayTao = d.NgayTao ?? default(DateTime),
                            base64 = d.hinhanh != null ? Convert.ToBase64String(d.hinhanh) : null
                        })
                        .FirstOrDefaultAsync();

                    if (denbu == null)
                        return NotFound(ApiResponse<object>.CreateError("Đền bù không tồn tại"));

                    return Ok(ApiResponse<CompensationDto>.CreateSuccess(
                        "Lấy thông tin đền bù thành công",
                        denbu
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lấy thông tin đền bù");
                    return StatusCode(500, ApiResponse<object>.CreateError(
                        "Đã xảy ra lỗi khi lấy thông tin đền bù"
                    ));
                }
            }
        [HttpPost("add-denbu")]
        public async Task<IActionResult> CreateDenbu([FromForm] CompensationCreateDto model)
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
                }; byte[] imageBytes = null;
                if (model.hinhanh != null && model.hinhanh.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await model.hinhanh.CopyToAsync(ms);
                    imageBytes = ms.ToArray();


                }
                denbu.hinhanh = imageBytes;
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
        public async Task<IActionResult> UpdateDenbu(int id, [FromForm] CompensationCreateDto model)
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
        [HttpGet("get-denbu-by-user/{userId}")]
        public async Task<IActionResult> GetDenbuByUser(int userId)
        {
            try
            {
                // Lấy tất cả hợp đồng của người dùng
                var hopDongIds = await _context.HopDongNguoiThues
                    .Where(h => h.MaKhachThue == userId)
                    .Select(h => h.MaHopDong)
                    .ToListAsync();

                if (!hopDongIds.Any())
                {
                    return Ok(ApiResponse<List<CompensationDto>>.CreateSuccess(
                        "Người dùng chưa có hợp đồng thuê trọ",
                        new List<CompensationDto>()
                    ));
                }

                var denbus = await _context.DenBus
                    .Include(d => d.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation)
                    .Where(d => hopDongIds.Contains(d.MaHopDong ?? 0))
                    .Select(d => new CompensationDto
                    {
                        MaDenBu = d.MaDenBu,
                        MaHopDong = d.MaHopDong ?? 0,
                        NoiDung = d.NoiDung,
                        SoTien = d.SoTien ?? 0,
                        Nhatro = d.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
                        NgayTao = d.NgayTao ?? default(DateTime),
                        base64 = d.hinhanh != null ? Convert.ToBase64String(d.hinhanh) : null
                    })
                    .OrderByDescending(d => d.NgayTao)
                    .ToListAsync();

                return Ok(ApiResponse<List<CompensationDto>>.CreateSuccess(
                    "Lấy danh sách đền bù thành công",
                    denbus
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đền bù theo người dùng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách đền bù"
                ));
            }
        }
        [HttpGet("get-my-denbu")]
        public async Task<IActionResult> GetMyDenbu()
        {
            try
            {
                // Lấy thông tin user hiện tại
                var userName = User.Identity.Name;
                if (userName == null)
                {
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));
                }

                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
                if (user == null)
                {
                    user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
                }
                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.CreateError("Người dùng không tồn tại"));
                }

                // Lấy tất cả hợp đồng của người dùng
                var hopDongIds = await _context.HopDongNguoiThues
                    .Where(h => h.MaKhachThue == user.MaNguoiDung)
                    .Select(h => h.MaHopDong)
                    .ToListAsync();

                if (!hopDongIds.Any())
                {
                    return Ok(ApiResponse<List<CompensationDto>>.CreateSuccess(
                        "Bạn chưa có hợp đồng thuê trọ nào",
                        new List<CompensationDto>()
                    ));
                }

                var denbus = await _context.DenBus
                    .Include(d => d.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation)
                    .Where(d => hopDongIds.Contains(d.MaHopDong ?? 0))
                    .Select(d => new CompensationDto
                    {
                        MaDenBu = d.MaDenBu,
                        MaHopDong = d.MaHopDong ?? 0,
                        NoiDung = d.NoiDung,
                        SoTien = d.SoTien ?? 0,
                        Nhatro = d.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
                        NgayTao = d.NgayTao ?? default(DateTime),
                        base64 = d.hinhanh != null ? Convert.ToBase64String(d.hinhanh) : null
                    })
                    .OrderByDescending(d => d.NgayTao)
                    .ToListAsync();

                return Ok(ApiResponse<List<CompensationDto>>.CreateSuccess(
                    "Lấy danh sách đền bù thành công",
                    denbus
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đền bù của user hiện tại");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách đền bù"
                ));
            }
        }
        public class CompensationCreateDto
        {
            public IFormFile hinhanh { get; set; }// SoTien
            public int MaHopDong { get; set; }              // MaHopDong
            public string NoiDung { get; set; }              // NoiDung
            public decimal SoTien { get; set; }              // SoTien
        }

        public class CompensationDto
        {
            public int MaDenBu { get; set; }          // MaDenBu
            public int MaHopDong { get; set; }              // MaHopDong
            public string NoiDung { get; set; }              // NoiDung
            public decimal SoTien { get; set; }
            public string base64 { get; set; }
            public IFormFile hinhanh { get; set; } // SoTien
            public DateTime NgayTao { get; set; }        // NgayTao
            public string Nhatro { get; set; } // NhaTro
        }

    }
}
