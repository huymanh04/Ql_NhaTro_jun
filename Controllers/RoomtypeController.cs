using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class RoomTypeController : ControllerBase
{
    private readonly ILogger<RoomTypeController> _logger;
    private readonly QlNhatroContext _context;
    private const int MaxImageSize = 5 * 1024 * 1024; // 5MB

    public RoomTypeController(ILogger<RoomTypeController> logger, QlNhatroContext context)
    {
        _logger = logger;
        _context = context;
    }

    #region Loại Phòng APIs
    /// Lấy danh sách tất cả loại phòng
    [HttpGet("get-type-room")]
    public async Task<IActionResult> GetRoomTypes()
    {
        try
        {
            var roomTypes = await _context.TheLoaiPhongTros
                .Select(t => new RoomTypeResponseDto
                {
                    MaTheLoai = t.MaTheLoai,
                    TenTheLoai = t.TenTheLoai,
                    MoTa = t.MoTa,
                    ImageBase64 = t.ImageUrl != null ? Convert.ToBase64String(t.ImageUrl) : null,
                    RedirectUrl = t.RedirectUrl,
                    SoLuongPhong = t.PhongTros.Count
                })
                .ToListAsync();

            return Ok(ApiResponse<List<RoomTypeResponseDto>>.CreateSuccess(
                "Lấy danh sách loại phòng thành công",
                roomTypes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách loại phòng");
            return StatusCode(500, ApiResponse<object>.CreateError(
                "Đã xảy ra lỗi khi lấy danh sách loại phòng"));
        }
    }

    /// Thêm mới loại phòng
    [HttpPost("add-type-room")]
    public async Task<IActionResult> AddRoomType([FromForm] RoomTypeCreateDto model)
    {
        try
        {
            #region check quyền và login
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return Unauthorized(new { message = "Bạn chưa đăng nhập" });
            }
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
            if (user == null)
            {
                user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
            }
            if (user == null)
            {
                return Unauthorized(new { message = "Người dùng không tồn tại" });
            }
            if (user.VaiTro == "0") // Kiểm tra quyền người dùng
            {
                return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
            }
            #endregion
                if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "Dữ liệu không hợp lệ",
                    GetModelStateErrors()));
            }

            // Kiểm tra trùng tên loại phòng
            if (await _context.TheLoaiPhongTros.AnyAsync(t =>
                t.TenTheLoai.ToLower() == model.TenTheLoai.ToLower().Trim()))
            {
                return BadRequest(ApiResponse<object>.CreateError("Loại phòng đã tồn tại"));
            }

            // Xử lý upload ảnh
            byte[] imageBytes = null;
            if (model.ImageFile != null)
            {
                if (model.ImageFile.Length > MaxImageSize)
                {
                    return BadRequest(ApiResponse<object>.CreateError(
                        "Kích thước ảnh không được vượt quá 5MB"));
                }

                if (!IsValidImageFile(model.ImageFile))
                {
                    return BadRequest(ApiResponse<object>.CreateError(
                        "Chỉ chấp nhận file ảnh định dạng JPG, PNG, GIF"));
                }

                using var ms = new MemoryStream();
                await model.ImageFile.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            var newRoomType = new TheLoaiPhongTro
            {
                TenTheLoai = model.TenTheLoai.Trim(),
                MoTa = model.MoTa?.Trim(),
                ImageUrl = imageBytes,
                RedirectUrl = model.RedirectUrl?.Trim()
            };

            _context.TheLoaiPhongTros.Add(newRoomType);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<RoomTypeResponseDto>.CreateSuccess(
                "Thêm loại phòng thành công",
                MapToResponseDto(newRoomType)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm loại phòng: {TenTheLoai}", model.TenTheLoai);
            return StatusCode(500, ApiResponse<object>.CreateError(
                "Đã xảy ra lỗi khi thêm loại phòng"));
        }
    }
    /// Cập nhật thông tin loại phòng 
    [HttpPut("edit-type-room/{id}")]
    public async Task<IActionResult> UpdateRoomType(int id, [FromForm] RoomTypeUpdateDto model)
    {
        try
        {
            #region check quyền và login
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return Unauthorized(new { message = "Bạn chưa đăng nhập" });
            }
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
            if (user == null)
            {
                user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
            }
            if (user == null)
            {
                return Unauthorized(new { message = "Người dùng không tồn tại" });
            }
            if (user.VaiTro == "0") // Kiểm tra quyền người dùng
            {
                return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
            }
            #endregion
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "Dữ liệu không hợp lệ",
                    GetModelStateErrors()));
            }

            var roomType = await _context.TheLoaiPhongTros.FindAsync(id);
            if (roomType == null)
            {
                return NotFound(ApiResponse<object>.CreateError("Loại phòng không tồn tại"));
            }

            // Kiểm tra trùng tên với loại phòng khác
            if (await _context.TheLoaiPhongTros.AnyAsync(t =>
                t.MaTheLoai != id &&
                t.TenTheLoai.ToLower() == model.TenTheLoai.ToLower().Trim()))
            {
                return BadRequest(ApiResponse<object>.CreateError("Tên loại phòng đã tồn tại"));
            }

            // Xử lý upload ảnh mới nếu có
            if (model.ImageFile != null)
            {
                if (model.ImageFile.Length > MaxImageSize)
                {
                    return BadRequest(ApiResponse<object>.CreateError(
                        "Kích thước ảnh không được vượt quá 5MB"));
                }

                if (!IsValidImageFile(model.ImageFile))
                {
                    return BadRequest(ApiResponse<object>.CreateError(
                        "Chỉ chấp nhận file ảnh định dạng JPG, PNG, GIF"));
                }

                using var ms = new MemoryStream();
                await model.ImageFile.CopyToAsync(ms);
                roomType.ImageUrl = ms.ToArray();
            }

            // Cập nhật thông tin
            roomType.TenTheLoai = model.TenTheLoai.Trim();
            roomType.MoTa = model.MoTa?.Trim();
            roomType.RedirectUrl = model.RedirectUrl?.Trim();

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<RoomTypeResponseDto>.CreateSuccess(
                "Cập nhật loại phòng thành công",
                MapToResponseDto(roomType)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật loại phòng: {Id}", id);
            return StatusCode(500, ApiResponse<object>.CreateError(
                "Đã xảy ra lỗi khi cập nhật loại phòng"));
        }
    }
    [HttpDelete("delete-type-room/{id}")]
    public async Task<IActionResult> DeleteRoomType(int id)
    {
        try
        {
            // Kiểm tra tồn tại và lấy thông tin loại phòng kèm theo các phòng liên quan
            var roomType = await _context.TheLoaiPhongTros
                .Include(t => t.PhongTros)
                .FirstOrDefaultAsync(t => t.MaTheLoai == id);

            if (roomType == null)
            {
                return NotFound(ApiResponse<object>.CreateError(
                    "Không tìm thấy loại phòng cần xóa"));
            }

            // Kiểm tra xem có phòng nào thuộc loại này không
            if (roomType.PhongTros != null && roomType.PhongTros.Any())
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "Không thể xóa loại phòng này vì đang có phòng thuộc loại này",
                    $"Số phòng hiện tại: {roomType.PhongTros.Count}"));
            }

            try
            {
                // Xóa file ảnh nếu có
                if (roomType.ImageUrl != null)
                {
                    // Có thể thêm logic xóa file ảnh từ storage nếu cần
                    roomType.ImageUrl = null;
                }

                // Xóa loại phòng
                _context.TheLoaiPhongTros.Remove(roomType);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa loại phòng thành công",
                    new
                    {
                        MaTheLoai = id,
                        TenTheLoai = roomType.TenTheLoai
                    }));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Lỗi khi xóa loại phòng khỏi database: {Id}", id);
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa dữ liệu",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi xóa loại phòng: {Id}", id);
            return StatusCode(500, ApiResponse<object>.CreateError(
                "Đã xảy ra lỗi không xác định khi xóa loại phòng"));
        }
    }
    /// Xóa nhiều loại phòng cùng lúc
    [HttpDelete("bulk-delete-type-room")]
    public async Task<IActionResult> BulkDeleteRoomTypes([FromBody] List<int> ids)
    {
        try
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "Danh sách loại phòng cần xóa không được để trống"));
            }

            var results = new List<object>();
            var hasErrors = false;

            foreach (var id in ids)
            {
                try
                {
                    var roomType = await _context.TheLoaiPhongTros
                        .Include(t => t.PhongTros)
                        .FirstOrDefaultAsync(t => t.MaTheLoai == id);

                    if (roomType == null)
                    {
                        results.Add(new
                        {
                            Id = id,
                            Success = false,
                            Message = "Không tìm thấy loại phòng"
                        });
                        hasErrors = true;
                        continue;
                    }

                    if (roomType.PhongTros != null && roomType.PhongTros.Any())
                    {
                        results.Add(new
                        {
                            Id = id,
                            Success = false,
                            Message = $"Không thể xóa vì có {roomType.PhongTros.Count} phòng thuộc loại này"
                        });
                        hasErrors = true;
                        continue;
                    }

                    // Xóa file ảnh nếu có
                    if (roomType.ImageUrl != null)
                    {
                        roomType.ImageUrl = null;
                    }

                    _context.TheLoaiPhongTros.Remove(roomType);
                    await _context.SaveChangesAsync();

                    results.Add(new
                    {
                        Id = id,
                        Success = true,
                        Message = "Xóa thành công"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xóa loại phòng: {Id}", id);
                    results.Add(new
                    {
                        Id = id,
                        Success = false,
                        Message = "Lỗi khi xóa"
                    });
                    hasErrors = true;
                }
            }

            return Ok(ApiResponse<object>.CreateSuccess(
                hasErrors ? "Có một số lỗi xảy ra khi xóa" : "Xóa tất cả thành công",
                new { Results = results }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thực hiện xóa hàng loạt");
            return StatusCode(500, ApiResponse<object>.CreateError(
                "Đã xảy ra lỗi khi xóa nhiều loại phòng"));
        }
    }
    #endregion

    #region Helper Methods
    private bool IsValidImageFile(IFormFile file)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        return allowedTypes.Contains(file.ContentType.ToLower());
    }

    private string GetModelStateErrors()
    {
        return string.Join(", ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
    }

    private RoomTypeResponseDto MapToResponseDto(TheLoaiPhongTro model)
    {
        return new RoomTypeResponseDto
        {
            MaTheLoai = model.MaTheLoai,
            TenTheLoai = model.TenTheLoai,
            MoTa = model.MoTa,
            ImageBase64 = model.ImageUrl != null ? Convert.ToBase64String(model.ImageUrl) : null,
            RedirectUrl = model.RedirectUrl
        };
    }
    #endregion

    #region DTOs
    public class RoomTypeCreateDto
    {
        [Required(ErrorMessage = "Tên loại phòng không được để trống")]
        [StringLength(100, ErrorMessage = "Tên loại phòng không được vượt quá 100 ký tự")]
        public string TenTheLoai { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string MoTa { get; set; }

        public IFormFile ImageFile { get; set; }

        [Url(ErrorMessage = "URL không hợp lệ")]
        public string RedirectUrl { get; set; }
    }

    public class RoomTypeUpdateDto : RoomTypeCreateDto { }

    public class RoomTypeResponseDto
    {
        public int MaTheLoai { get; set; }
        public string TenTheLoai { get; set; }
        public string MoTa { get; set; }
        public string ImageBase64 { get; set; }
        public string RedirectUrl { get; set; }
        public int SoLuongPhong { get; set; }
    }
    #endregion
}