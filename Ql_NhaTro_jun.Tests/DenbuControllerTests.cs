using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static Ql_NhaTro_jun.Controllers.DenbuController;

namespace Ql_NhaTro_jun.Tests
{
    [TestFixture]
    public class DenbuControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<DenbuController>> _mockLogger;
        private DenbuController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"DenbuDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<DenbuController>>();
            _controller = new DenbuController(_mockLogger.Object, _context);

            // Setup fake HttpContext với user đã login
            SetupFakeUser("0987654321");
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupFakeUser(string userName)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, userName) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // ─── GetDenbu ───────────────────────────────────────────────

        [Test]
        public async Task GetDenbu_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.GetDenbu();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── GetDenbuById (by HopDong) ──────────────────────────────

        [Test]
        public async Task GetDenbuById_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.GetDenbuById(999);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetDenbuById_ReturnsOk_WhenFound()
        {
            _context.DenBus.Add(new DenBu
            {
                MaDenBu = 1,
                MaHopDong = 5,
                NoiDung = "Vỡ cửa kính",
                SoTien = 500000m,
                NgayTao = DateTime.Now
            });
            await _context.SaveChangesAsync();

            var result = await _controller.GetDenbuById(5);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── CreateDenbu ────────────────────────────────────────────

        [Test]
        public async Task CreateDenbu_ReturnsOk_WhenValidModel()
        {
            var model = new CompensationCreateDto
            {
                MaHopDong = 1,
                NoiDung = "Hỏng bàn",
                SoTien = 200000m
            };

            var result = await _controller.CreateDenbu(model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateDenbu_SavesDataCorrectly()
        {
            var model = new CompensationCreateDto
            {
                MaHopDong = 3,
                NoiDung = "Vỡ gương",
                SoTien = 300000m
            };

            await _controller.CreateDenbu(model);

            var saved = await _context.DenBus.FirstOrDefaultAsync(d => d.MaHopDong == 3);
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.NoiDung, Is.EqualTo("Vỡ gương"));
        }

        /// ─── UpdateDenbu ────────────────────────────────────────────

        [Test]
        public async Task UpdateDenbu_ReturnsBadRequest_WhenModelIsNull()
        {
            var result = await _controller.UpdateDenbu(1, default!);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateDenbu_ReturnsNotFound_WhenNotExist()
        {
            var model = new CompensationCreateDto { MaHopDong = 1, NoiDung = "X", SoTien = 100 };
            var result = await _controller.UpdateDenbu(999, model);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateDenbu_ReturnsOk_WhenFound()
        {
            _context.DenBus.Add(new DenBu { MaDenBu = 1, MaHopDong = 1, NoiDung = "Old", SoTien = 100, NgayTao = DateTime.Now });
            await _context.SaveChangesAsync();

            var model = new CompensationCreateDto { MaHopDong = 1, NoiDung = "Updated", SoTien = 999 };

            var result = await _controller.UpdateDenbu(1, model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── DeleteDenbu ────────────────────────────────────────────

        [Test]
        public async Task DeleteDenbu_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.DeleteDenbu(999);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteDenbu_ReturnsOk_WhenFound()
        {
            _context.DenBus.Add(new DenBu { MaDenBu = 1, MaHopDong = 1, NoiDung = "Test", SoTien = 100, NgayTao = DateTime.Now });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteDenbu(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── GetDenbuByUser ─────────────────────────────────────────

        [Test]
        public async Task GetDenbuByUser_ReturnsOk_WhenNoContracts()
        {
            var result = await _controller.GetDenbuByUser(999);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetDenbuByUser_ReturnsOk_WithContracts()
        {
            // Setup: user 1 có hợp đồng 10
            _context.HopDongNguoiThues.Add(new HopDongNguoiThue { MaHopDong = 10, MaKhachThue = 1 });
            _context.DenBus.Add(new DenBu { MaDenBu = 1, MaHopDong = 10, NoiDung = "Hư tủ lạnh", SoTien = 1000000m, NgayTao = DateTime.Now });
            await _context.SaveChangesAsync();

            var result = await _controller.GetDenbuByUser(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── GetMyDenbu ─────────────────────────────────────────────

        [Test]
        public async Task GetMyDenbu_ReturnsUnauthorized_WhenUserNotFound()
        {
            // User name không tồn tại trong DB
            SetupFakeUser("notfound@email.com");

            var result = await _controller.GetMyDenbu();

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task GetMyDenbu_ReturnsOk_WhenNoContracts()
        {
            _context.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 1, SoDienThoai = "0987654321", VaiTro = "0" });
            await _context.SaveChangesAsync();

            var result = await _controller.GetMyDenbu();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyDenbu_ReturnsOk_WithContracts()
        {
            _context.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 1, SoDienThoai = "0987654321", VaiTro = "0" });
            _context.HopDongNguoiThues.Add(new HopDongNguoiThue { MaHopDong = 10, MaKhachThue = 1 });
            _context.DenBus.Add(new DenBu { MaDenBu = 1, MaHopDong = 10, NoiDung = "Hư bàn", SoTien = 500000m, NgayTao = DateTime.Now });
            await _context.SaveChangesAsync();

            var result = await _controller.GetMyDenbu();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyDenbu_ReturnsUnauthorized_WhenNotLoggedIn()
        {
            // Reset HttpContext - no user
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.GetMyDenbu();

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }
    }
}
