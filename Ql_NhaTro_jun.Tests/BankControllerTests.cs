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
using static Ql_NhaTro_jun.Controllers.BankController;

namespace Ql_NhaTro_jun.Tests
{
    [TestFixture]
    public class BankControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<BankController>> _mockLogger;
        private BankController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"BankDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<BankController>>();
            _controller = new BankController(_mockLogger.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupAuthUser(string userName, string vaiTro = "1")
        {
            _context.NguoiDungs.Add(new NguoiDung
            {
                MaNguoiDung = 99,
                SoDienThoai = userName,
                HoTen = "Test User",
                VaiTro = vaiTro
            });
            _context.SaveChanges();

            var claims = new[] { new Claim(ClaimTypes.Name, userName) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupNoAuthUser()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // ─── GetBanks ───────────────────────────────────────────────

        [Test]
        public async Task GetBanks_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.GetBanks();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetBanks_ReturnsOk_WithData()
        {
            _context.Banks.Add(new Bank { Id = 1, TenNganHang = "MB Bank", SoTaiKhoan = "1234567890", Ten = "Nguyen Van A" });
            await _context.SaveChangesAsync();

            var result = await _controller.GetBanks();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── CreateBank ─────────────────────────────────────────────

        [Test]
        public async Task CreateBank_ReturnsBadRequest_WhenModelIsNull()
        {
            SetupAuthUser("0912345678", "1");

            var result = await _controller.CreateBank(default!);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateBank_ReturnsUnauthorized_WhenNotLoggedIn()
        {
            SetupNoAuthUser();

            var model = new BankDTO { TenNganHang = "VCB", SoTaiKhoan = "123", Ten = "A" };
            var result = await _controller.CreateBank(model);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task CreateBank_ReturnsBadRequest_WhenUserRoleIs0()
        {
            SetupAuthUser("0912345678", "0"); // Khách hàng

            var model = new BankDTO { TenNganHang = "VCB", SoTaiKhoan = "123", Ten = "A" };
            var result = await _controller.CreateBank(model);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateBank_ReturnsOk_WhenAdmin()
        {
            SetupAuthUser("0912345678", "1"); // Chủ trọ

            var model = new BankDTO { TenNganHang = "MB Bank", SoTaiKhoan = "9876543210", Ten = "Nguyen B" };
            var result = await _controller.CreateBank(model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── UpdateBank ─────────────────────────────────────────────

        [Test]
        public async Task UpdateBank_ReturnsBadRequest_WhenModelIsNull()
        {
            var result = await _controller.UpdateBank(1, default!);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateBank_ReturnsBadRequest_WhenIdIsZero()
        {
            var model = new BankDTO { TenNganHang = "Test", SoTaiKhoan = "123", Ten = "A" };
            var result = await _controller.UpdateBank(0, model);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateBank_ReturnsNotFound_WhenNotExist()
        {
            var model = new BankDTO { TenNganHang = "Test", SoTaiKhoan = "123", Ten = "A" };
            var result = await _controller.UpdateBank(999, model);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateBank_ReturnsOk_WhenFound()
        {
            _context.Banks.Add(new Bank { Id = 1, TenNganHang = "Old Bank", SoTaiKhoan = "111", Ten = "Old Name" });
            await _context.SaveChangesAsync();

            var model = new BankDTO { TenNganHang = "New Bank", SoTaiKhoan = "222", Ten = "New Name" };
            var result = await _controller.UpdateBank(1, model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── DeleteBank ─────────────────────────────────────────────

        [Test]
        public async Task DeleteBank_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.DeleteBank(999);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteBank_ReturnsOk_WhenFound()
        {
            _context.Banks.Add(new Bank { Id = 1, TenNganHang = "Delete Me", SoTaiKhoan = "000", Ten = "X" });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteBank(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
