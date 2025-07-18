using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using Ql_NhaTro_jun.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly QlNhatroContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        
        public MessageController(ILogger<MessageController> logger, QlNhatroContext context, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _context = context;
            _hubContext = hubContext;
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

                _logger.LogInformation($"[add-message] currentUserId: {currentUserId}, model.NguoiGuiID: {model.NguoiGuiID}, model.NguoiNhanID: {model.NguoiNhanID}, NoiDung: {model.NoiDung}, MaPhong: {model.MaPhong}");

                if (model.NguoiGuiID != currentUserId)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn chỉ có thể gửi tin nhắn với tư cách của mình"));
                }

                var sender = await _context.NguoiDungs.FindAsync(model.NguoiGuiID);
                var receiver = await _context.NguoiDungs.FindAsync(model.NguoiNhanID);
                _logger.LogInformation($"[add-message] Sender: {sender?.HoTen} (Role: {sender?.VaiTro}), Receiver: {receiver?.HoTen} (Role: {receiver?.VaiTro})");

                _logger.LogInformation($"Checking permission for message: Sender={model.NguoiGuiID}, Receiver={model.NguoiNhanID}, MaPhong={model.MaPhong}");
                var hasPermission = await CheckChatPermission(model.NguoiGuiID, model.NguoiNhanID, model.MaPhong);
                _logger.LogInformation($"Permission result: {hasPermission}");
                
                if (!hasPermission)
                {
                    _logger.LogWarning($"Permission denied for message: Sender={model.NguoiGuiID}, Receiver={model.NguoiNhanID}, MaPhong={model.MaPhong}");
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền gửi tin nhắn cho người này hoặc mã phòng không hợp lệ"));
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

                _logger.LogInformation($"[add-message] Tin nhắn đã lưu: MaTinNhan={message.MaTinNhan}, NguoiGuiId={message.NguoiGuiId}, NguoiNhanId={message.NguoiNhanId}, NoiDung={message.NoiDung}, MaPhong={message.MaPhong}");

                // Gửi tin nhắn real-time qua SignalR đến cả người gửi và người nhận
                _logger.LogInformation($"[SignalR] Gửi tin nhắn đến người nhận: user_{message.NguoiNhanId}");
                try {
                    await _hubContext.Clients.Group($"user_{message.NguoiNhanId}").SendAsync("ReceiveMessage", 
                        message.NguoiGuiId.ToString(), 
                        message.NoiDung, 
                        sender?.HoTen ?? "Unknown");
                    _logger.LogInformation($"[SignalR] Đã gửi tin nhắn thành công đến người nhận: user_{message.NguoiNhanId}");
                } catch (Exception ex) {
                    _logger.LogError(ex, $"[SignalR] Lỗi khi gửi tin nhắn đến người nhận: user_{message.NguoiNhanId}");
                }
                
                // Gửi tin nhắn đến người gửi để cập nhật UI ngay lập tức
                _logger.LogInformation($"[SignalR] Gửi tin nhắn đến người gửi: user_{message.NguoiGuiId}");
                try {
                    await _hubContext.Clients.Group($"user_{message.NguoiGuiId}").SendAsync("ReceiveMessage", 
                        message.NguoiGuiId.ToString(), 
                        message.NoiDung, 
                        sender?.HoTen ?? "Unknown");
                    _logger.LogInformation($"[SignalR] Đã gửi tin nhắn thành công đến người gửi: user_{message.NguoiGuiId}");
                } catch (Exception ex) {
                    _logger.LogError(ex, $"[SignalR] Lỗi khi gửi tin nhắn đến người gửi: user_{message.NguoiGuiId}");
                }

                _logger.LogInformation($"[SignalR] Đã gửi tin nhắn thành công qua SignalR");

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

        [HttpGet("conversations-for-admin")]
        public async Task<IActionResult> GetConversationsForAdmin()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                _logger.LogInformation($"GetConversationsForAdmin - CurrentUserId: {currentUserId}");
                
                if (currentUserId == 0)
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));

                var user = await _context.NguoiDungs.FindAsync(currentUserId);
                if (user == null)
                    return BadRequest(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                    
                if (user.VaiTro != "2") // Admin là role "2"
                    return BadRequest(ApiResponse<object>.CreateError("Chỉ Admin mới có thể truy cập tính năng này"));

                _logger.LogInformation($"Admin found: {user.HoTen}, Role: {user.VaiTro}");

                // Lấy tất cả người dùng (khách thuê, quản lý) để admin có thể nhắn tin
                var allUsers = await _context.NguoiDungs
                    .Where(u => u.MaNguoiDung != currentUserId && (u.VaiTro == "0" || u.VaiTro == "1"))
                    .ToListAsync();

                _logger.LogInformation($"Admin can chat with {allUsers.Count} users: {string.Join(", ", allUsers.Select(u => $"{u.HoTen}({u.VaiTro})"))}");

                // Lấy thông tin phòng cho khách thuê
                var tenantRooms = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaPhongNavigation)
                    .Where(h => h.MaKhachThue != currentUserId)
                    .GroupBy(h => h.MaKhachThue)
                    .ToDictionaryAsync(g => g.Key, g => g.First().MaHopDongNavigation.MaPhongNavigation.MaPhong);

                var result = new List<ConversationDTO>();

                foreach (var u in allUsers)
                {
                    try
                    {
                        // Lấy tin nhắn cuối cùng giữa admin và user này
                        var lastMessage = await _context.TinNhans
                            .Where(m =>
                                ((m.NguoiGuiId == currentUserId && m.NguoiNhanId == u.MaNguoiDung) ||
                                 (m.NguoiGuiId == u.MaNguoiDung && m.NguoiNhanId == currentUserId)) &&
                                m.ThoiGianGui.HasValue)
                            .OrderByDescending(m => m.ThoiGianGui)
                            .Select(m => new { m.NoiDung, m.ThoiGianGui, m.DaXem })
                            .FirstOrDefaultAsync();

                        // Đếm tin nhắn chưa đọc từ user này gửi cho admin
                        var unreadCount = await _context.TinNhans
                            .CountAsync(m => m.NguoiGuiId == u.MaNguoiDung &&
                                           m.NguoiNhanId == currentUserId &&
                                           m.DaXem == false);

                        // Lấy MaPhong cho khách thuê
                        int maPhong = 0;
                        if (u.VaiTro == "0" && tenantRooms.ContainsKey(u.MaNguoiDung))
                        {
                            maPhong = tenantRooms[u.MaNguoiDung];
                        }

                        result.Add(new ConversationDTO
                        {
                            RecipientId = u.MaNguoiDung,
                            RecipientName = u.HoTen ?? "Unknown",
                            MaHopDong = 0, // Admin không cần hợp đồng
                            MaPhong = maPhong, // MaPhong cho khách thuê
                            RoomName = u.VaiTro == "0" ? "Khách thuê" : "Quản lý",
                            MotelName = u.VaiTro == "0" ? "Khách hàng" : "Nhân viên",
                            LastMessage = lastMessage?.NoiDung ?? "Chưa có tin nhắn",
                            LastMessageTime = lastMessage?.ThoiGianGui,
                            UnreadCount = unreadCount
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing user {u.MaNguoiDung}");
                        continue;
                    }
                }

                // Sắp xếp theo thời gian tin nhắn cuối cùng
                result = result.OrderByDescending(c => c.LastMessageTime).ToList();

                _logger.LogInformation($"Admin conversations - Found {result.Count} users");
                foreach (var convUser in result)
                {
                    _logger.LogInformation($"User: ID={convUser.RecipientId}, Name={convUser.RecipientName}, Role={convUser.RoomName}, Unread={convUser.UnreadCount}");
                }

                return Ok(ApiResponse<List<ConversationDTO>>.CreateSuccess(
                    "Lấy danh sách người dùng thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng cho admin");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách người dùng: " + ex.Message
                ));
            }
        }

        [HttpGet("conversations-for-landlord-simple")]
        public async Task<IActionResult> GetConversationsForLandlordSimple()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                _logger.LogInformation($"GetConversationsForLandlordSimple - CurrentUserId: {currentUserId}");
                
                if (currentUserId == 0)
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));

                var user = await _context.NguoiDungs.FindAsync(currentUserId);
                if (user == null)
                    return BadRequest(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                    
                if (user.VaiTro != "1")
                    return BadRequest(ApiResponse<object>.CreateError("Chỉ Quản lý mới có thể truy cập tính năng này"));

                _logger.LogInformation($"Manager found: {user.HoTen}, Role: {user.VaiTro}");

                // Lấy tất cả khách thuê để quản lý có thể nhắn tin
                var allTenants = await _context.NguoiDungs
                    .Where(u => u.VaiTro == "0")
                    .ToListAsync();

                var result = new List<ConversationDTO>();

                foreach (var tenant in allTenants)
                {
                    try
                    {
                        // Lấy tin nhắn cuối cùng giữa quản lý và khách thuê này
                        var lastMessage = await _context.TinNhans
                            .Where(m =>
                                ((m.NguoiGuiId == currentUserId && m.NguoiNhanId == tenant.MaNguoiDung) ||
                                 (m.NguoiGuiId == tenant.MaNguoiDung && m.NguoiNhanId == currentUserId)) &&
                                m.ThoiGianGui.HasValue)
                            .OrderByDescending(m => m.ThoiGianGui)
                            .Select(m => new { m.NoiDung, m.ThoiGianGui, m.DaXem })
                            .FirstOrDefaultAsync();

                        // Đếm tin nhắn chưa đọc từ khách thuê này gửi cho quản lý
                        var unreadCount = await _context.TinNhans
                            .CountAsync(m => m.NguoiGuiId == tenant.MaNguoiDung &&
                                           m.NguoiNhanId == currentUserId &&
                                           m.DaXem == false);

                        // Lấy thông tin phòng của khách thuê này (nếu có)
                        var tenantContract = await _context.HopDongNguoiThues
                            .Include(h => h.MaHopDongNavigation)
                                .ThenInclude(hd => hd.MaPhongNavigation)
                                .ThenInclude(p => p.MaNhaTroNavigation)
                            .Where(h => h.MaKhachThue == tenant.MaNguoiDung)
                            .FirstOrDefaultAsync();

                        var roomName = tenantContract?.MaHopDongNavigation?.MaPhongNavigation?.TenPhong ?? "Khách thuê";
                        var motelName = tenantContract?.MaHopDongNavigation?.MaPhongNavigation?.MaNhaTroNavigation?.TenNhaTro ?? "Khách hàng";
                        var maPhong = tenantContract?.MaHopDongNavigation?.MaPhongNavigation?.MaPhong ?? 0;

                        result.Add(new ConversationDTO
                        {
                            RecipientId = tenant.MaNguoiDung,
                            RecipientName = tenant.HoTen ?? "Unknown",
                            MaHopDong = tenantContract?.MaHopDong ?? 0,
                            MaPhong = maPhong,
                            RoomName = roomName,
                            MotelName = motelName,
                            LastMessage = lastMessage?.NoiDung ?? "Chưa có tin nhắn",
                            LastMessageTime = lastMessage?.ThoiGianGui,
                            UnreadCount = unreadCount
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing tenant {tenant.MaNguoiDung}");
                        continue;
                    }
                }

                // Sắp xếp theo thời gian tin nhắn cuối cùng
                result = result.OrderByDescending(c => c.LastMessageTime).ToList();

                _logger.LogInformation($"Manager conversations - Found {result.Count} tenants");
                foreach (var tenant in result)
                {
                    _logger.LogInformation($"Tenant: ID={tenant.RecipientId}, Name={tenant.RecipientName}, Unread={tenant.UnreadCount}");
                }

                return Ok(ApiResponse<List<ConversationDTO>>.CreateSuccess(
                    "Lấy danh sách khách thuê cho quản lý thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách khách thuê cho quản lý");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách khách thuê: " + ex.Message
                ));
            }
        }

        [HttpGet("conversations-for-tenant")]
        public async Task<IActionResult> GetConversationsForTenant()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                _logger.LogInformation($"GetConversationsForTenant - CurrentUserId: {currentUserId}");
                
                if (currentUserId == 0)
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));

                var user = await _context.NguoiDungs.FindAsync(currentUserId);
                if (user == null)
                    return BadRequest(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                
                if (user.VaiTro != "0" && user.VaiTro != "2")
                    return BadRequest(ApiResponse<object>.CreateError("Chỉ khách thuê hoặc admin mới có thể truy cập tính năng này"));

                _logger.LogInformation($"User found: {user.HoTen}, Role: {user.VaiTro}");

                // Lấy tất cả hợp đồng của khách thuê này
                var hopDongList = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                        .ThenInclude(hd => hd.MaPhongNavigation)
                        .ThenInclude(p => p.MaNhaTroNavigation)
                    .Where(h => h.MaKhachThue == currentUserId)
                    .ToListAsync();

                _logger.LogInformation($"Found {hopDongList.Count} contracts for user {currentUserId}");

                var result = new List<ConversationDTO>();

                // Lấy tất cả quản lý và admin mà khách thuê có thể nhắn tin
                var allManagersAndAdmins = new List<NguoiDung>();

                // Lấy tất cả quản lý (role 1)
                var quanLyList = await _context.NguoiDungs
                    .Where(u => u.VaiTro == "1" && u.MaNguoiDung != currentUserId)
                    .ToListAsync();
                
                foreach (var quanLy in quanLyList)
                {
                    allManagersAndAdmins.Add(quanLy);
                }

                // Lấy tất cả admin (role 2)
                var adminList = await _context.NguoiDungs
                    .Where(u => u.VaiTro == "2" && u.MaNguoiDung != currentUserId)
                    .ToListAsync();
                
                foreach (var admin in adminList)
                {
                    allManagersAndAdmins.Add(admin);
                }

                _logger.LogInformation($"Tenant can chat with {allManagersAndAdmins.Count} users: {string.Join(", ", allManagersAndAdmins.Select(u => $"{u.HoTen}({u.VaiTro})"))}");

                // Tạo conversation cho từng quản lý/admin
                foreach (var managerOrAdmin in allManagersAndAdmins)
                {
                    try
                    {
                        var lastMessage = await _context.TinNhans
                            .Where(m =>
                                ((m.NguoiGuiId == currentUserId && m.NguoiNhanId == managerOrAdmin.MaNguoiDung) ||
                                 (m.NguoiGuiId == managerOrAdmin.MaNguoiDung && m.NguoiNhanId == currentUserId)) &&
                                m.ThoiGianGui.HasValue)
                            .OrderByDescending(m => m.ThoiGianGui)
                            .Select(m => new { m.NoiDung, m.ThoiGianGui, m.DaXem })
                            .FirstOrDefaultAsync();

                        var unreadCount = await _context.TinNhans
                            .CountAsync(m => m.NguoiGuiId == managerOrAdmin.MaNguoiDung &&
                                           m.NguoiNhanId == currentUserId &&
                                           m.DaXem == false);

                        // Lấy thông tin phòng từ hợp đồng đầu tiên
                        var firstContract = hopDongList.FirstOrDefault();
                        var roomName = firstContract?.MaHopDongNavigation?.MaPhongNavigation?.TenPhong ?? "Unknown";
                        var motelName = firstContract?.MaHopDongNavigation?.MaPhongNavigation?.MaNhaTroNavigation?.TenNhaTro ?? "Unknown";
                        var maPhong = firstContract?.MaHopDongNavigation?.MaPhongNavigation?.MaPhong ?? 0;

                        result.Add(new ConversationDTO
                        {
                            RecipientId = managerOrAdmin.MaNguoiDung,
                            RecipientName = managerOrAdmin.HoTen ?? "Unknown",
                            MaHopDong = firstContract?.MaHopDong ?? 0,
                            MaPhong = maPhong,
                            RoomName = roomName,
                            MotelName = motelName,
                            LastMessage = lastMessage?.NoiDung ?? "Chưa có tin nhắn",
                            LastMessageTime = lastMessage?.ThoiGianGui,
                            UnreadCount = unreadCount
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing manager/admin {managerOrAdmin.MaNguoiDung}");
                        continue;
                    }
                }

                // Bổ sung: Lấy tất cả admin để khách thuê có thể chat với admin
                var admins = await _context.NguoiDungs
                    .Where(u => u.VaiTro == "2")
                    .ToListAsync();

                // Admin đã được thêm vào allManagersAndAdmins ở trên, không cần thêm lại

                // Sắp xếp theo thời gian tin nhắn cuối cùng
                result = result.OrderByDescending(c => c.LastMessageTime).ToList();

                _logger.LogInformation($"Returning {result.Count} conversations");

                return Ok(ApiResponse<List<ConversationDTO>>.CreateSuccess(
                    "Lấy danh sách cuộc trò chuyện thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cuộc trò chuyện cho khách thuê");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách cuộc trò chuyện: " + ex.Message
                ));
            }
        }

        [HttpGet("test-user")]
        public async Task<IActionResult> TestUser()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var userName = User.Identity?.Name;
                
                var user = await _context.NguoiDungs.FindAsync(currentUserId);
                
                string roleDescription = user?.VaiTro switch
                {
                    "0" => "Khách thuê",
                    "1" => "Quản lý",
                    "2" => "Admin",
                    _ => "Không xác định"
                };
                
                return Ok(new
                {
                    currentUserId = currentUserId,
                    userName = userName,
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    userRole = user?.VaiTro,
                    userHoTen = user?.HoTen,
                    roleDescription = roleDescription,
                    message = "Test user info"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("conversation-between")]
        public async Task<IActionResult> GetConversationBetween([FromQuery] int userId1, [FromQuery] int userId2)
        {
            try
            {
                _logger.LogInformation($"GetConversationBetween - userId1: {userId1}, userId2: {userId2}");
                
                if (userId1 == 0 || userId2 == 0)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Tham số userId không hợp lệ"));
                }

                // Kiểm tra xem 2 người dùng có tồn tại không
                var user1 = await _context.NguoiDungs.FindAsync(userId1);
                var user2 = await _context.NguoiDungs.FindAsync(userId2);
                
                _logger.LogInformation($"User1: {user1?.HoTen} (ID: {userId1}), User2: {user2?.HoTen} (ID: {userId2})");
                
                if (user1 == null || user2 == null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Một trong hai người dùng không tồn tại"));
                }

                _logger.LogInformation($"User1: {user1.HoTen} (Role: {user1.VaiTro}), User2: {user2.HoTen} (Role: {user2.VaiTro})");

                // Lấy tất cả tin nhắn giữa 2 user (cả 2 chiều)
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
                        DaXem = m.DaXem ?? false,
                        TenNguoiGui = m.NguoiGuiId == userId1 ? user1.HoTen : user2.HoTen
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {messages.Count} messages between users {userId1} and {userId2}");
                foreach (var msg in messages)
                {
                    _logger.LogInformation($"Message: ID={msg.MaTinNhan}, From={msg.NguoiGuiID} ({msg.TenNguoiGui}), To={msg.NguoiNhanID}, Content={msg.NoiDung}, MaPhong={msg.MaPhong}");
                }

                _logger.LogInformation($"Found {messages.Count} messages between users {userId1} and {userId2}");
                foreach (var msg in messages)
                {
                    _logger.LogInformation($"Message: ID={msg.MaTinNhan}, From={msg.NguoiGuiID} ({msg.TenNguoiGui}), To={msg.NguoiNhanID}, Content={msg.NoiDung}");
                }

                _logger.LogInformation($"Found {messages.Count} messages between users {userId1} and {userId2}");

                // Đánh dấu tin nhắn từ user2 gửi cho user1 là đã đọc
                var unreadMessages = await _context.TinNhans
                    .Where(m => m.NguoiGuiId == userId2 && m.NguoiNhanId == userId1 && m.DaXem == false)
                    .ToListAsync();

                if (unreadMessages.Any())
                {
                    foreach (var msg in unreadMessages)
                    {
                        msg.DaXem = true;
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Marked {unreadMessages.Count} messages as read");
                }

                return Ok(ApiResponse<List<TinNhanDTO>>.CreateSuccess(
                    "Lấy lịch sử chat thành công",
                    messages ?? new List<TinNhanDTO>()
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử chat giữa 2 người");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy lịch sử chat: " + ex.Message
                ));
            }
        }

        [HttpPost("mark-messages-as-read")]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] MarkAsReadRequest request)
        {
            try
            {
                _logger.LogInformation($"MarkMessagesAsRead - CurrentUserId: {request.CurrentUserId}, RecipientId: {request.RecipientId}");
                
                if (request.CurrentUserId == 0 || request.RecipientId == 0)
                {
                    return BadRequest(new { message = "Invalid user IDs" });
                }

                // Đánh dấu tất cả tin nhắn từ recipientId gửi cho currentUserId là đã đọc
                var unreadMessages = await _context.TinNhans
                    .Where(m => m.NguoiGuiId == request.RecipientId && 
                               m.NguoiNhanId == request.CurrentUserId && 
                               m.DaXem == false)
                    .ToListAsync();

                foreach (var message in unreadMessages)
                {
                    message.DaXem = true;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Marked {unreadMessages.Count} messages as read");
                
                return Ok(new { 
                    success = true, 
                    message = $"Đã đánh dấu {unreadMessages.Count} tin nhắn đã đọc",
                    count = unreadMessages.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking messages as read: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("conversations-for-all")]
        public async Task<IActionResult> GetConversationsForAll()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));

                var allUsers = await _context.NguoiDungs
                    .Where(u => u.MaNguoiDung != currentUserId)
                    .ToListAsync();

                var result = new List<ConversationDTO>();
                foreach (var u in allUsers)
                {
                    var lastMessage = await _context.TinNhans
                        .Where(m =>
                            ((m.NguoiGuiId == currentUserId && m.NguoiNhanId == u.MaNguoiDung) ||
                             (m.NguoiGuiId == u.MaNguoiDung && m.NguoiNhanId == currentUserId)) &&
                            m.ThoiGianGui.HasValue)
                        .OrderByDescending(m => m.ThoiGianGui)
                        .Select(m => new { m.NoiDung, m.ThoiGianGui, m.DaXem })
                        .FirstOrDefaultAsync();

                    var unreadCount = await _context.TinNhans
                        .CountAsync(m => m.NguoiGuiId == u.MaNguoiDung &&
                                       m.NguoiNhanId == currentUserId &&
                                       m.DaXem == false);

                    result.Add(new ConversationDTO
                    {
                        RecipientId = u.MaNguoiDung,
                        RecipientName = u.HoTen ?? "Unknown",
                        MaHopDong = 0,
                        MaPhong = 0,
                        RoomName = u.VaiTro == "0" ? "Khách thuê" : (u.VaiTro == "1" ? "Quản lý" : "Admin"),
                        MotelName = "",
                        LastMessage = lastMessage?.NoiDung ?? "Chưa có tin nhắn",
                        LastMessageTime = lastMessage?.ThoiGianGui,
                        UnreadCount = unreadCount
                    });
                }

                result = result.OrderByDescending(c => c.LastMessageTime).ToList();

                return Ok(ApiResponse<List<ConversationDTO>>.CreateSuccess(
                    "Lấy danh sách hội thoại thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách hội thoại: " + ex.Message
                ));
            }
        }

        [HttpGet("conversations-for-allowed")]
        public async Task<IActionResult> GetConversationsForAllowed()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                    return Unauthorized(ApiResponse<object>.CreateError("Bạn chưa đăng nhập"));

                var user = await _context.NguoiDungs.FindAsync(currentUserId);
                if (user == null)
                    return BadRequest(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));

                List<NguoiDung> allowedUsers = new List<NguoiDung>();
                if (user.VaiTro == "0") // Khách hàng
                    allowedUsers = await _context.NguoiDungs.Where(u => u.MaNguoiDung != currentUserId && (u.VaiTro == "1" || u.VaiTro == "2")).ToListAsync();
                else if (user.VaiTro == "1") // Quản lý
                    allowedUsers = await _context.NguoiDungs.Where(u => u.MaNguoiDung != currentUserId && (u.VaiTro == "0" || u.VaiTro == "2")).ToListAsync();
                else if (user.VaiTro == "2") // Admin
                    allowedUsers = await _context.NguoiDungs.Where(u => u.MaNguoiDung != currentUserId && (u.VaiTro == "0" || u.VaiTro == "1")).ToListAsync();

                var result = new List<ConversationDTO>();
                foreach (var u in allowedUsers)
                {
                    var lastMessage = await _context.TinNhans
                        .Where(m =>
                            ((m.NguoiGuiId == currentUserId && m.NguoiNhanId == u.MaNguoiDung) ||
                             (m.NguoiGuiId == u.MaNguoiDung && m.NguoiNhanId == currentUserId)) &&
                            m.ThoiGianGui.HasValue)
                        .OrderByDescending(m => m.ThoiGianGui)
                        .Select(m => new { m.NoiDung, m.ThoiGianGui, m.DaXem })
                        .FirstOrDefaultAsync();

                    var unreadCount = await _context.TinNhans
                        .CountAsync(m => m.NguoiGuiId == u.MaNguoiDung &&
                                       m.NguoiNhanId == currentUserId &&
                                       m.DaXem == false);

                    result.Add(new ConversationDTO
                    {
                        RecipientId = u.MaNguoiDung,
                        RecipientName = u.HoTen ?? "Unknown",
                        MaHopDong = 0,
                        MaPhong = 0,
                        RoomName = u.VaiTro == "0" ? "Khách thuê" : (u.VaiTro == "1" ? "Quản lý" : "Admin"),
                        MotelName = "",
                        LastMessage = lastMessage?.NoiDung ?? "Chưa có tin nhắn",
                        LastMessageTime = lastMessage?.ThoiGianGui,
                        UnreadCount = unreadCount
                    });
                }

                result = result.OrderByDescending(c => c.LastMessageTime).ToList();

                return Ok(ApiResponse<List<ConversationDTO>>.CreateSuccess(
                    "Lấy danh sách hội thoại thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách hội thoại: " + ex.Message
                ));
            }
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userName = User.Identity?.Name;
            _logger.LogInformation($"GetCurrentUserId - UserName: {userName}");
            
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("UserName is null or empty");
                return 0;
            }

            var user = _context.NguoiDungs.FirstOrDefault(u => u.SoDienThoai == userName);
            if (user == null)
            {
                _logger.LogInformation($"User not found by phone, trying email: {userName}");
                user = _context.NguoiDungs.FirstOrDefault(u => u.Email == userName);
            }

            var userId = user?.MaNguoiDung ?? 0;
            _logger.LogInformation($"GetCurrentUserId result: {userId}");
            return userId;
        }

        private async Task<bool> CheckChatPermission(int senderId, int receiverId, int? maPhong)
        {
            var sender = await _context.NguoiDungs.FindAsync(senderId);
            var receiver = await _context.NguoiDungs.FindAsync(receiverId);
            
            _logger.LogInformation($"CheckChatPermission - Sender: {sender?.HoTen} (Role: {sender?.VaiTro}), Receiver: {receiver?.HoTen} (Role: {receiver?.VaiTro}), MaPhong: {maPhong}");
            
            if (sender == null || receiver == null)
            {
                _logger.LogWarning("Sender or receiver not found");
                return false;
            }

            // Admin (role 2) có thể nhắn tin với tất cả
            if (sender.VaiTro == "2" || receiver.VaiTro == "2")
            {
                _logger.LogInformation("Admin involved - allowing chat");
                return true;
            }

            // Quản lý (role 1) có thể nhắn tin với khách thuê (role 0)
            if (sender.VaiTro == "1" && receiver.VaiTro == "0")
            {
                _logger.LogInformation("Manager to tenant - allowing chat");
                return true;
            }

            if (sender.VaiTro == "0" && receiver.VaiTro == "1")
            {
                _logger.LogInformation("Tenant to manager - allowing chat");
                return true;
            }

            // Khách thuê (role 0) có thể nhắn tin với quản lý (role 1) hoặc admin (role 2)
            if (sender.VaiTro == "0" && (receiver.VaiTro == "1" || receiver.VaiTro == "2"))
            {
                _logger.LogInformation("Tenant to manager/admin - allowing chat");
                return true;
            }

            _logger.LogWarning($"No permission rule matched - Sender: {sender.VaiTro}, Receiver: {receiver.VaiTro}");
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
            public string? TenNguoiGui { get; set; }
        }

        public class ConversationDTO
        {
            public int RecipientId { get; set; }
            public string RecipientName { get; set; }
            public int MaHopDong { get; set; }
            public int MaPhong { get; set; }
            public string RoomName { get; set; }
            public string MotelName { get; set; }
            public string LastMessage { get; set; }
            public DateTime? LastMessageTime { get; set; }
            public int UnreadCount { get; set; }
        }

        public class MarkAsReadRequest
        {
            public int CurrentUserId { get; set; }
            public int RecipientId { get; set; }
        }
    }
}
