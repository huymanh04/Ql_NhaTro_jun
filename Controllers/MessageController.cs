using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly QlNhatroContext _context;
        public MessageController(ILogger<MessageController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet("get-messages")]
        public async Task<IActionResult> GetMessages([FromQuery] int userId)
        {
            try
            {
                var messages = await _context.TinNhans
                    .Where(m => (m.NguoiGuiId ?? 0) == userId || (m.NguoiNhanId ?? 0) == userId)
                    .Select(m => new TinNhanDTO
                    {
                        MaTinNhan = m.MaTinNhan,
                        MaPhong = m.MaPhong ?? 0,
                        NguoiGuiID = m.NguoiGuiId ?? 0,
                        NguoiNhanID = m.NguoiNhanId ?? 0,
                        NoiDung = m.NoiDung,
                        ThoiGianGui = m.ThoiGianGui ?? DateTime.MinValue,
                        DaXem = m.DaXem ?? false
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<TinNhanDTO>>.CreateSuccess(
                    "Lấy danh sách tin nhắn thành công",
                    messages
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tin nhắn");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách tin nhắn"
                ));
            }
        }
        [HttpPut("tinnhan/{id}/daxem")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var tinNhan = await _context.TinNhans.FindAsync(id);
            if (tinNhan == null)
            {
                return NotFound(ApiResponse<object>.CreateError("Tin nhắn không tồn tại"));
            }

            tinNhan.DaXem = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.CreateSuccess("Đã đánh dấu tin nhắn đã xem",tinNhan));
        }
        [HttpPost("add-message")]
        public async Task<IActionResult> CreateMessage([FromBody] TinNhanDTO model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var message = new TinNhan
                {
                    MaPhong = model.MaPhong,
                    NguoiGuiId = model.NguoiGuiID,
                    NguoiNhanId = model.NguoiNhanID,
                    NoiDung = model.NoiDung,
                    ThoiGianGui = DateTime.Now,
                    DaXem = false
                };

                _context.TinNhans.Add(message);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<TinNhanDTO>.CreateSuccess(
                    "Tạo tin nhắn thành công",
                    new TinNhanDTO
                    {
                        MaTinNhan = message.MaTinNhan,
                        MaPhong = message.MaPhong ?? 0,
                        NguoiGuiID = message.NguoiGuiId ?? 0,
                        NguoiNhanID = message.NguoiNhanId ?? 0,
                        NoiDung = message.NoiDung,
                        ThoiGianGui = message.ThoiGianGui ?? default(DateTime),
                        DaXem = message.DaXem ?? false
                    }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tin nhắn");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi tạo tin nhắn"
                ));
            }
        }
        [HttpDelete("delete-message/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var message = await _context.TinNhans.FindAsync(id);
                if (message == null)
                    return NotFound(ApiResponse<object>.CreateError("Tin nhắn không tồn tại"));

                _context.TinNhans.Remove(message);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Xóa tin nhắn thành công", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tin nhắn");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa tin nhắn"
                ));
            }
        }
        public class TinNhanDTO
        {
            public int MaTinNhan { get; set; }
            public int MaPhong { get; set; }
            public int NguoiGuiID { get; set; }
            public int NguoiNhanID { get; set; }
            public string NoiDung { get; set; }
            public DateTime ThoiGianGui { get; set; }
            public bool DaXem { get; set; }
        }
    }
}
