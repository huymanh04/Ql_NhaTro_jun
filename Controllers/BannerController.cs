using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController : ControllerBase
    {
        private readonly ILogger<BannerController> _logger;
        private readonly QlNhatroContext _context;

        public BannerController(ILogger<BannerController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        #region Lấy danh sách tất cả banner
        [HttpGet("get-banner")]
        public async Task<IActionResult> GetBanners()
        {
            try
            {
                var bannerEntities = await _context.Banners.ToListAsync();

                var banners = bannerEntities.Select(b => new BannerDto
                {
                    BannerId = b.BannerId,
                    Title = b.Title,
                    Content = b.Content,
                    Text = b.Text,
                    ImageBase64 = b.ImageUrl != null ? Convert.ToBase64String(b.ImageUrl) : null,
                    RedirectUrl = b.RedirectUrl,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt
                }).ToList();
                return Ok(ApiResponse<List<BannerDto>>.CreateSuccess(
    "Lấy danh sách banner thành công",
    banners
));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách banner");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách banner"
                ));
            }
        }
        #endregion
        #region Thêm mới banner
        [HttpPost("add-banner")]
        public async Task<IActionResult> CreateBanner([FromForm] BannerCreateDto model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var banner = new Banner
                {
                    Title = model.Title,
                    Content = model.Content,
                    Text = model.Text,
                    RedirectUrl = model.RedirectUrl,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                if (model.ImageFile != null)
                {
                    if (model.ImageFile.Length > 5 * 1024 * 1024) // 5MB limit
                        return BadRequest(ApiResponse<object>.CreateError("Kích thước ảnh không được vượt quá 5MB"));

                    using var ms = new MemoryStream();
                    await model.ImageFile.CopyToAsync(ms);
                    banner.ImageUrl = ms.ToArray();
                }

                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();

                // Chuyển đổi sang DTO trước khi trả về
                var bannerDto = new BannerDto
                {
                    BannerId = banner.BannerId,
                    Title = banner.Title,
                    Content = banner.Content,
                    Text = banner.Text,
                    ImageBase64 = banner.ImageUrl != null ? Convert.ToBase64String(banner.ImageUrl) : null,
                    RedirectUrl = banner.RedirectUrl,
                    IsActive = banner.IsActive,
                    CreatedAt = banner.CreatedAt
                };

                return Ok(ApiResponse<BannerDto>.CreateSuccess(
                    "Thêm banner thành công",
                    bannerDto
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm banner");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm banner"
                ));
            }
        }
        #endregion
        #region Cập nhật banner
        [HttpPut("edit-banner/{id}")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] BannerUpdateDto model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner == null)
                    return NotFound(ApiResponse<object>.CreateError("Banner không tồn tại"));

                // Cập nhật thông tin
                banner.Title = model.Title ?? banner.Title;
                banner.Content = model.Content ?? banner.Content;
                banner.Text = model.Text ?? banner.Text;
                banner.RedirectUrl = model.RedirectUrl ?? banner.RedirectUrl;
                banner.IsActive = model.IsActive ?? banner.IsActive;

                // Xử lý ảnh nếu có
                if (model.ImageFile != null)
                {
                    if (model.ImageFile.Length > 5 * 1024 * 1024)
                        return BadRequest(ApiResponse<object>.CreateError("Kích thước ảnh không được vượt quá 5MB"));

                    using var ms = new MemoryStream();
                    await model.ImageFile.CopyToAsync(ms);
                    banner.ImageUrl = ms.ToArray();
                }

                await _context.SaveChangesAsync();

                var bannerDto = new BannerDto
                {
                    BannerId = banner.BannerId,
                    Title = banner.Title,
                    Content = banner.Content,
                    Text = banner.Text,
                    ImageBase64 = banner.ImageUrl != null ? Convert.ToBase64String(banner.ImageUrl) : null,
                    RedirectUrl = banner.RedirectUrl,
                    IsActive = banner.IsActive,
                    CreatedAt = banner.CreatedAt
                };

                return Ok(ApiResponse<BannerDto>.CreateSuccess(
                    "Cập nhật banner thành công",
                    bannerDto
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật banner");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi cập nhật banner"
                ));
            }
        }
        #endregion
        #region Xóa banner
        [HttpDelete("delete-banner/{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            try
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner == null)
                    return NotFound(ApiResponse<object>.CreateError("Banner không tồn tại"));

                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa banner thành công",
                    null
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa banner");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa banner"
                ));
            }
        }
        #endregion
    }

    #region DTO classes
    public class BannerDto
    {
        public int BannerId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public string ImageBase64 { get; set; }
        public string RedirectUrl { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class BannerCreateDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public IFormFile ImageFile { get; set; }
        public string RedirectUrl { get; set; }
        public bool? IsActive { get; set; }
    }

    public class BannerUpdateDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public IFormFile ImageFile { get; set; }
        public string RedirectUrl { get; set; }
        public bool? IsActive { get; set; }
    }
    #endregion
}