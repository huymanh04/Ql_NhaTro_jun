using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentHistoryController : ControllerBase
    {
        private readonly ILogger<PaymentHistoryController> _logger;

      private readonly QlNhatroContext _context;

        public PaymentHistoryController(ILogger<PaymentHistoryController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet("get-payment-history/{id}")]
        public async Task<IActionResult> GetPaymentHistory(int id)
        {
            try
            {
                var paymentHistory = await _context.LichSuThanhToans.Where(p => p.MaHopDong == id)
                    .Select(p => new LichSuThanhToanDTO
                    {
                        MaThanhToan = p.MaThanhToan,
                        MaHopDong = p.MaHopDong ?? 0,
                        NgayThanhToan = p.NgayThanhToan ?? DateTime.MinValue,
                        SoTien = p.SoTien ?? 0,
                        PhuongThuc = p.PhuongThuc,
                        GhiChu = p.GhiChu
                    })
                    .ToListAsync();
               

                return Ok(ApiResponse<List<LichSuThanhToanDTO>>.CreateSuccess(
                    "Lấy danh sách lịch sử thanh toán thành công",
                    paymentHistory
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch sử thanh toán");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách lịch sử thanh toán"
                ));
            }
        }
        [HttpPost("add-payment-history")]
        public async Task<IActionResult> CreatePaymentHistory([FromBody] LichSuThanhToanDTO model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var paymentHistory = new LichSuThanhToan
                {
                    MaHopDong = model.MaHopDong,
                    NgayThanhToan = model.NgayThanhToan,
                    SoTien = model.SoTien,
                    PhuongThuc = model.PhuongThuc,
                    GhiChu = model.GhiChu
                };

                _context.LichSuThanhToans.Add(paymentHistory);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<LichSuThanhToanDTO>.CreateSuccess(
                    "Thêm lịch sử thanh toán thành công",
                    model
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm lịch sử thanh toán");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm lịch sử thanh toán"
                ));
            }
        }
        public class LichSuThanhToanDTO
        {
            public int MaThanhToan { get; set; }
            public int MaHopDong { get; set; }
            public DateTime NgayThanhToan { get; set; }
            public decimal SoTien { get; set; }
            public string PhuongThuc { get; set; }
            public string GhiChu { get; set; }
        }
    }
}
