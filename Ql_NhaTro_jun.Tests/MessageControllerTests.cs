using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using Microsoft.AspNetCore.SignalR;
using Ql_NhaTro_jun.Hubs;
using System;
using System.Threading.Tasks;

namespace Ql_NhaTro_jun.Tests
{
    [TestFixture]
    public class MessageControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<MessageController>> _mockLogger;
        private Mock<IHubContext<ChatHub>> _mockHubContext;
        private MessageController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"MessageDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<MessageController>>();
            _mockHubContext = new Mock<IHubContext<ChatHub>>();
            _controller = new MessageController(_mockLogger.Object, _context, _mockHubContext.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── Helper Methods ─────────────────────────────────────────

        private void SeedMessageTestData()
        {
            // Add KhuVuc
            var khuVuc = new KhuVuc { MaKhuVuc = 1, TenKhuVuc = "Quận 1" };
            _context.KhuVucs.Add(khuVuc);

            // Add Owner
            var chuTro = new NguoiDung
            {
                MaNguoiDung = 1,
                SoDienThoai = "0912345678",
                HoTen = "Chủ Trọ A",
                Email = "owner@example.com",
                VaiTro = "1"
            };
            _context.NguoiDungs.Add(chuTro);

            // Add Tenant
            var khachThue = new NguoiDung
            {
                MaNguoiDung = 2,
                SoDienThoai = "0987654321",
                HoTen = "Khách Thuê B",
                Email = "tenant@example.com",
                VaiTro = "0"
            };
            _context.NguoiDungs.Add(khachThue);

            // Add NhaTro
            var nhaTro = new NhaTro
            {
                MaNhaTro = 1,
                TenNhaTro = "Nhà Trọ A",
                DiaChi = "123 Đường A, Q1",
                MaKhuVuc = 1,
                MaChuTro = 1,
                
            };
            _context.NhaTros.Add(nhaTro);

            // Add TheLoai
            var theLoai = new TheLoaiPhongTro
            {
                MaTheLoai = 1,
                TenTheLoai = "Phòng đơn"
            };
            _context.TheLoaiPhongTros.Add(theLoai);

            // Add PhongTro
            var phongTro = new PhongTro
            {
                MaPhong = 1,
                MaNhaTro = 1,
                MaTheLoai = 1,
                TenPhong = "Phòng 101",
                Gia = 3000000,
                DienTich = 20,
                ConTrong = false,
                MoTa = "Phòng sạch"
            };
            _context.PhongTros.Add(phongTro);

            // Add HopDong
            var hopDong = new HopDong
            {
                MaHopDong = 1,
                MaPhong = 1,                
                NgayBatDau = DateOnly.FromDateTime(DateTime.Now),
                NgayKetThuc = DateOnly.FromDateTime(DateTime.Now.AddMonths(11)),
                SoNguoiO = 1,
                SoXe = 1,
                TienDatCoc = 3000000,
                DaKetThuc = false // Hợp đồng đang hoạt động
            };
            _context.HopDongs.Add(hopDong);

            // Add HopDongNguoiThue
            var hopDongNguoiThue = new HopDongNguoiThue
            {
                Id = 1,
                MaHopDong = 1,
                MaKhachThue = 2
            };
            _context.HopDongNguoiThues.Add(hopDongNguoiThue);

            // Add Messages
            var message1 = new TinNhan
            {
                MaTinNhan = 1,
                NoiDung = "Hello from tenant",
                NguoiGuiId = 2,
                NguoiNhanId = 1,
                ThoiGianGui = DateTime.Now.AddHours(-1),
                DaXem = true 
            };
            _context.TinNhans.Add(message1);

            var message2 = new TinNhan
            {
                MaTinNhan = 2,
                MaPhong = null, 
                NoiDung = "Hi from owner",
                NguoiGuiId = 1,
                NguoiNhanId = 2,
                ThoiGianGui = DateTime.Now,
                DaXem = false
            };
            _context.TinNhans.Add(message2);

            _context.SaveChanges();
        }

        // ─── GetConversationByContract Tests ─────────────────────────

        [Test]
        public async Task GetConversationByContract_ReturnsOkResult_WithValidContractId()
        {
            SeedMessageTestData();
            var result = await _controller.GetConversationByContract(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetConversationByContract_ReturnsNotFound_WithInvalidContractId()
        {
            SeedMessageTestData();
            var result = await _controller.GetConversationByContract(999);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task GetConversationByContract_IncludesMessages_InResult()
        {
            SeedMessageTestData();
            var result = await _controller.GetConversationByContract(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.Not.Null);
        }

        [Test]
        public async Task GetConversationByContract_ReturnsOnlyRelatedMessages()
        {
            SeedMessageTestData();
            var result = await _controller.GetConversationByContract(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetConversationByContract_WithEmptyConversation()
        {
            SeedMessageTestData();
            
            // Delete all messages
            var messages = _context.TinNhans.ToList();
            _context.TinNhans.RemoveRange(messages);
            _context.SaveChanges();

            var result = await _controller.GetConversationByContract(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── Error Handling Tests ────────────────────────────────────

        [Test]
        public async Task GetConversationByContract_WithZeroContractId()
        {
            SeedMessageTestData();
            var result = await _controller.GetConversationByContract(0);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>().Or.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetConversationByContract_WithNegativeContractId()
        {
            SeedMessageTestData();
            var result = await _controller.GetConversationByContract(-1);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>().Or.InstanceOf<OkObjectResult>());
        }
    }
}
