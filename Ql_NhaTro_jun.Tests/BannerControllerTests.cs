using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ql_NhaTro_jun.Tests
{
    [TestFixture]
    public class BannerControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<BannerController>> _mockLogger;
        private BannerController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"BannerDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<BannerController>>();
            _controller = new BannerController(_mockLogger.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private IFormFile CreateFakeImage(string fileName = "test.jpg", string contentType = "image/jpeg", int sizeBytes = 1024)
        {
            var content = new byte[sizeBytes];
            var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, sizeBytes, "ImageFile", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
            return formFile;
        }

        // ─── GetBanners ─────────────────────────────────────────────

        [Test]
        public async Task GetBanners_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.GetBanners();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetBanners_ReturnsOk_WithBanners()
        {
            _context.Banners.Add(new Banner
            {
                BannerId = 1,
                Title = "Banner 1",
                Content = "Noi dung",
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            var result = await _controller.GetBanners();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetBanners_ReturnsOk_BannerWithImage()
        {
            _context.Banners.Add(new Banner
            {
                BannerId = 2,
                Title = "Banner With Image",
                Content = "Content",
                ImageUrl = Encoding.UTF8.GetBytes("fake-image-bytes"),
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            var result = await _controller.GetBanners();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── CreateBanner ───────────────────────────────────────────

        [Test]
        public async Task CreateBanner_ReturnsOk_WithoutImage()
        {
            var model = new BannerCreateDto
            {
                Title = "Banner Moi",
                Content = "Noi dung",
                Text = "Text",
                RedirectUrl = "https://example.com",
                IsActive = true
            };

            var result = await _controller.CreateBanner(model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateBanner_ReturnsOk_WithValidImage()
        {
            var model = new BannerCreateDto
            {
                Title = "Banner With Image",
                Content = "Content",
                IsActive = true,
                ImageFile = CreateFakeImage("test.jpg", "image/jpeg", 1024)
            };

            var result = await _controller.CreateBanner(model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateBanner_ReturnsBadRequest_WhenImageTooLarge()
        {
            var model = new BannerCreateDto
            {
                Title = "Banner Large",
                Content = "Content",
                IsActive = true,
                ImageFile = CreateFakeImage("big.jpg", "image/jpeg", 6 * 1024 * 1024) // 6MB > 5MB limit
            };

            var result = await _controller.CreateBanner(model);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ─── UpdateBanner ───────────────────────────────────────────

        [Test]
        public async Task UpdateBanner_ReturnsBadRequest_WhenModelIsNull()
        {
            var result = await _controller.UpdateBanner(1, default!);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateBanner_ReturnsNotFound_WhenBannerNotExist()
        {
            var model = new BannerUpdateDto { Title = "Updated Title" };

            var result = await _controller.UpdateBanner(999, model);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateBanner_ReturnsOk_WhenFound()
        {
            _context.Banners.Add(new Banner { BannerId = 1, Title = "Old", Content = "", IsActive = true, CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            var model = new BannerUpdateDto { Title = "Updated", Content = "New Content", IsActive = false };

            var result = await _controller.UpdateBanner(1, model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateBanner_ReturnsBadRequest_WhenNewImageTooLarge()
        {
            _context.Banners.Add(new Banner { BannerId = 1, Title = "Old", Content = "", IsActive = true, CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            var model = new BannerUpdateDto
            {
                Title = "Updated",
                ImageFile = CreateFakeImage("big.jpg", "image/jpeg", 6 * 1024 * 1024)
            };

            var result = await _controller.UpdateBanner(1, model);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ─── DeleteBanner ───────────────────────────────────────────

        [Test]
        public async Task DeleteBanner_ReturnsOk_WhenFound()
        {
            _context.Banners.Add(new Banner { BannerId = 1, Title = "To Delete", Content = "", CreatedAt = DateTime.Now });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteBanner(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteBanner_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.DeleteBanner(999);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }
    }
}
