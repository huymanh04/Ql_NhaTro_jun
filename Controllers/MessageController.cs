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

        [HttpGet("conversation-by-contract")]
        public async Task<IActionResult> GetConversationByContract([FromQuery] int maHopDong)
        {
            try
            {
                var hopDong = await _context.HopDongs
                    .Include(h => h.HopDongNguoiThues)
                    .Include(h => h.MaPhongNavigation)
                    .ThenInclude(p => p.MaNhaTroNavigation)
                    .ThenInclude(n => n.MaChuTroNavigation)
                    .FirstOrDefaultAsync(h => h.MaHopDong == maHopDong);

                if (hopDong == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Hợp đồng không tồn tại"));
                }

                var khachThueIds = hopDong.HopDongNguoiThues.Select(ht => ht.MaKhachThue).ToList();
                var chuTroId = hopDong.MaPhongNavigation?.MaNhaTroNavigation?.MaChuTro;

                if (!chuTroId.HasValue)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Không tìm thấy thông tin chủ trọ"));
                }

                var messages = await _context.TinNhans
                    .Where(m => 
                        (m.NguoiGuiId == chuTroId && khachThueIds.Contains(m.NguoiNhanId ?? 0)) ||
                        (khachThueIds.Contains(m.NguoiGuiId ?? 0) && m.NguoiNhanId == chuTroId)
                    )
                    .OrderBy(m => m.ThoiGianGui)
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

                if (messages == null) messages = new List<TinNhanDTO>();
                return Ok(ApiResponse<List<TinNhanDTO>>.CreateSuccess(
                    "Lấy cuộc trò chuyện thành công",
                    messages
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cuộc trò chuyện theo hợp đồng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy cuộc trò chuyện"
                ));
            }
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
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));
                }

                if (model.NguoiGuiID != currentUserId)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn chỉ có thể gửi tin nhắn với tư cách của mình"));
                }

                var hasPermission = await CheckChatPermission(model.NguoiGuiID, model.NguoiNhanID);
                if (!hasPermission)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền gửi tin nhắn cho người này"));
                }

                // Nếu MaPhong bị null hoặc 0, tự động lấy từ hợp đồng
                int? maPhong = model.MaPhong > 0 ? model.MaPhong : null;
                if (!maPhong.HasValue && model.MaHopDong > 0)
                {
                    var hopDong = await _context.HopDongs.FirstOrDefaultAsync(h => h.MaHopDong == model.MaHopDong);
                    if (hopDong != null && hopDong.MaPhong.HasValue)
                    {
                        maPhong = hopDong.MaPhong.Value;
                    }
                }

                var message = new TinNhan
                {
                    MaPhong = maPhong,
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

        [HttpGet("conversations-for-landlord")]
        public async Task<IActionResult> GetConversationsForLandlord()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));
                }

                // Kiểm tra xem người dùng có phải là chủ trọ không
                var user = await _context.NguoiDungs.FindAsync(currentUserId);
                if (user == null || user.VaiTro != "1")
                {
                    return BadRequest(ApiResponse<object>.CreateError("Chỉ chủ trọ mới có thể truy cập tính năng này"));
                }

                // Lấy danh sách hợp đồng của chủ trọ này (KHÔNG phân biệt đã kết thúc hay chưa)
                var conversations = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                    .ThenInclude(h => h.MaPhongNavigation)
                    .ThenInclude(p => p.MaNhaTroNavigation)
                    .Include(h => h.MaKhachThueNavigation)
                    .Where(h => h.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.MaChuTro == currentUserId)
                    .Select(h => new
                    {
                        RecipientId = h.MaKhachThue,
                        RecipientName = h.MaKhachThueNavigation.HoTen,
                        MaHopDong = h.MaHopDong,
                        RoomName = h.MaHopDongNavigation.MaPhongNavigation.TenPhong,
                        MotelName = h.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
                        LastMessage = _context.TinNhans
                            .Where(m => 
                                ((m.NguoiGuiId == currentUserId && m.NguoiNhanId == h.MaKhachThue) ||
                                 (m.NguoiGuiId == h.MaKhachThue && m.NguoiNhanId == currentUserId)) &&
                                m.ThoiGianGui.HasValue)
                            .OrderByDescending(m => m.ThoiGianGui)
                            .Select(m => new { m.NoiDung, m.ThoiGianGui, m.DaXem })
                            .FirstOrDefault(),
                        UnreadCount = _context.TinNhans
                            .Count(m => m.NguoiGuiId == h.MaKhachThue && 
                                       m.NguoiNhanId == currentUserId && 
                                       m.DaXem == false)
                    })
                    .ToListAsync();

                var result = conversations.Select(c => new ConversationDTO
                {
                    RecipientId = c.RecipientId,
                    RecipientName = c.RecipientName,
                    MaHopDong = c.MaHopDong,
                    RoomName = c.RoomName,
                    MotelName = c.MotelName,
                    LastMessage = c.LastMessage?.NoiDung ?? "Chưa có tin nhắn",
                    LastMessageTime = c.LastMessage?.ThoiGianGui,
                    UnreadCount = c.UnreadCount
                }).OrderByDescending(c => c.LastMessageTime).ToList();

                return Ok(ApiResponse<List<ConversationDTO>>.CreateSuccess(
                    "Lấy danh sách cuộc trò chuyện thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cuộc trò chuyện cho chủ trọ");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách cuộc trò chuyện"
                ));
            }
        }

        [HttpGet("conversations-for-tenant")]
        public async Task<IActionResult> GetConversationsForTenant()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));

                var user = await _context.NguoiDungs.FindAsync(currentUserId);
                if (user == null || user.VaiTro != "0")
                    return BadRequest(ApiResponse<object>.CreateError("Chỉ khách thuê mới có thể truy cập tính năng này"));

                // Lấy tất cả hợp đồng của khách thuê này
                var hopDongList = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaPhongNavigation)
                        .ThenInclude(p => p.MaNhaTroNavigation)
                        .ThenInclude(n => n.MaChuTroNavigation)
                    .Where(h => h.MaKhachThue == currentUserId)
                    .ToListAsync();

                var result = new List<ConversationDTO>();

                foreach (var h in hopDongList)
                {
                    var chuTro = h.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.MaChuTroNavigation;
                    if (chuTro == null) continue;

                    var lastMessage = await _context.TinNhans
                        .Where(m =>
                            ((m.NguoiGuiId == currentUserId && m.NguoiNhanId == chuTro.MaNguoiDung) ||
                             (m.NguoiGuiId == chuTro.MaNguoiDung && m.NguoiNhanId == currentUserId)) &&
                            m.ThoiGianGui.HasValue)
                        .OrderByDescending(m => m.ThoiGianGui)
                        .Select(m => new { m.NoiDung, m.ThoiGianGui, m.DaXem })
                        .FirstOrDefaultAsync();

                    var unreadCount = await _context.TinNhans
                        .CountAsync(m => m.NguoiGuiId == chuTro.MaNguoiDung &&
                                       m.NguoiNhanId == currentUserId &&
                                       m.DaXem == false);

                    result.Add(new ConversationDTO
                    {
                        RecipientId = chuTro.MaNguoiDung,
                        RecipientName = chuTro.HoTen,
                        MaHopDong = h.MaHopDong,
                        RoomName = h.MaHopDongNavigation.MaPhongNavigation.TenPhong,
                        MotelName = h.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
                        LastMessage = lastMessage?.NoiDung ?? "Chưa có tin nhắn",
                        LastMessageTime = lastMessage?.ThoiGianGui,
                        UnreadCount = unreadCount
                    });
                }

                // Sắp xếp theo thời gian tin nhắn cuối cùng
                result = result.OrderByDescending(c => c.LastMessageTime).ToList();

                return Ok(ApiResponse<List<ConversationDTO>>.CreateSuccess(
                    "Lấy danh sách cuộc trò chuyện thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cuộc trò chuyện cho khách thuê");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách cuộc trò chuyện"
                ));
            }
        }

        [HttpGet("conversation-between")]
        public async Task<IActionResult> GetConversationBetween([FromQuery] int userId1, [FromQuery] int userId2)
        {
            try
            {
                var messages = await _context.TinNhans
                    .Where(m =>
                        (m.NguoiGuiId == userId1 && m.NguoiNhanId == userId2) ||
                        (m.NguoiGuiId == userId2 && m.NguoiNhanId == userId1)
                    )
                    .OrderBy(m => m.ThoiGianGui)
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
                    "Lấy lịch sử chat thành công",
                    messages ?? new List<TinNhanDTO>()
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử chat giữa 2 người");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy lịch sử chat"
                ));
            }
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return 0;

            var user = _context.NguoiDungs.FirstOrDefault(u => u.SoDienThoai == userName);
            if (user == null)
                user = _context.NguoiDungs.FirstOrDefault(u => u.Email == userName);

            return user?.MaNguoiDung ?? 0;
        }

        private async Task<bool> CheckChatPermission(int senderId, int receiverId)
        {
            var sender = await _context.NguoiDungs.FindAsync(senderId);
            var receiver = await _context.NguoiDungs.FindAsync(receiverId);
            if (sender == null || receiver == null)
                return false;

            // Nếu người gửi là khách thuê (vai trò 0)
            if (sender.VaiTro == "0")
            {
                // Tìm hợp đồng mà khách thuê là senderId và chủ trọ là receiverId
                var hopDong = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaPhongNavigation)
                        .ThenInclude(p => p.MaNhaTroNavigation)
                    .Where(h => h.MaKhachThue == senderId &&
                                h.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.MaChuTro == receiverId)
                    .FirstOrDefaultAsync();
                return hopDong != null;
            }
            // Nếu người gửi là chủ trọ (vai trò 1)
            else if (sender.VaiTro == "1")
            {
                // Tìm hợp đồng mà khách thuê là receiverId và chủ trọ là senderId
                var hopDong = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaPhongNavigation)
                        .ThenInclude(p => p.MaNhaTroNavigation)
                    .Where(h => h.MaKhachThue == receiverId &&
                                h.MaHopDongNavigation.MaPhongNavigation.MaNhaTroNavigation.MaChuTro == senderId)
                    .FirstOrDefaultAsync();
                return hopDong != null;
            }
            return false;
        }

        public class TinNhanDTO
        {
            public int MaTinNhan { get; set; }
            public int? MaPhong { get; set; }
            public int NguoiGuiID { get; set; }
            public int NguoiNhanID { get; set; }
            public string NoiDung { get; set; }
            public DateTime ThoiGianGui { get; set; }
            public bool DaXem { get; set; }
            public int? MaHopDong { get; set; }
        }

        public class ConversationDTO
        {
            public int RecipientId { get; set; }
            public string RecipientName { get; set; }
            public int MaHopDong { get; set; }
            public string RoomName { get; set; }
            public string MotelName { get; set; }
            public string LastMessage { get; set; }
            public DateTime? LastMessageTime { get; set; }
            public int UnreadCount { get; set; }
        }
    }
}
