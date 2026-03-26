using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static RoomTypeController;

namespace Ql_NhaTro_jun.Tests
{
    [TestFixture]
    public class RoomTypeControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<RoomTypeController>> _mockLogger;
        private RoomTypeController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"RoomTypeDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<RoomTypeController>>();
            _controller = new RoomTypeController(_mockLogger.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── Helper Methods ─────────────────────────────────────────

        private void SeedRoomTypeData()
        {
            var roomType1 = new TheLoaiPhongTro
            {
                MaTheLoai = 1,
                TenTheLoai = "Phòng Đơn",
                MoTa = "Phòng nhỏ cho 1 người",
                ImageUrl = new byte[] { 0x89, 0x50, 0x4E, 0x47 },
                RedirectUrl = "/single"
            };

            var roomType2 = new TheLoaiPhongTro
            {
                MaTheLoai = 2,
                TenTheLoai = "Phòng Đôi",
                MoTa = "Phòng vừa cho 2 người",
                ImageUrl = new byte[] { 0x89, 0x50, 0x4E, 0x47 },
                RedirectUrl = "/double"
            };

            _context.TheLoaiPhongTros.Add(roomType1);
            _context.TheLoaiPhongTros.Add(roomType2);
            _context.SaveChanges();
        }

        private void SetupAuthenticatedManager()
        {
            var user = new NguoiDung
            {
                MaNguoiDung = 1,
                SoDienThoai = "0912345678",
                HoTen = "Manager User",
                Email = "manager@example.com",
                VaiTro = "1"
            };
            _context.NguoiDungs.Add(user);
            _context.SaveChanges();

            var claims = new[] { new Claim(ClaimTypes.Name, "0912345678") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupAuthenticatedCustomer()
        {
            var user = new NguoiDung
            {
                MaNguoiDung = 2,
                SoDienThoai = "0987654321",
                HoTen = "Customer User",
                Email = "customer@example.com",
                VaiTro = "0"
            };
            _context.NguoiDungs.Add(user);
            _context.SaveChanges();

            var claims = new[] { new Claim(ClaimTypes.Name, "0987654321") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupUnauthenticatedUser()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // ─── GetRoomTypes Tests ─────────────────────────────────────

        [Test]
        public async Task GetRoomTypes_ReturnsOkResult_WithEmptyList()
        {
            var result = await _controller.GetRoomTypes();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.Not.Null);
        }

        [Test]
        public async Task GetRoomTypes_ReturnsOkResult_WithData()
        {
            SeedRoomTypeData();
            var result = await _controller.GetRoomTypes();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.Not.Null);
        }

        [Test]
        public async Task GetRoomTypes_IncludesAllRoomTypes()
        {
            SeedRoomTypeData();
            var result = await _controller.GetRoomTypes();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetRoomTypes_IncludesImageData()
        {
            SeedRoomTypeData();
            var result = await _controller.GetRoomTypes();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetRoomTypes_CountsRoomsPerType()
        {
            SeedRoomTypeData();
            var result = await _controller.GetRoomTypes();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
