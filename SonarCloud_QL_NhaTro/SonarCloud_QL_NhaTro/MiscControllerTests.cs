using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SonarCloud_QL_NhaTro
{
    // ════════════════════════════════════════════════════════════════
    //  EmailService Tests
    // ════════════════════════════════════════════════════════════════
    [TestFixture]
    public class EmailServiceTests
    {
        [Test]
        public void EmailService_CanBeInstantiated_WithSmtpSettings()
        {
            // Arrange
            var settings = new SmtpSettings
            {
                Host = "smtp.gmail.com",
                Port = 587,
                UserName = "test@gmail.com",
                Password = "password",
                EnableSsl = true
            };
            var options = Options.Create(settings);

            // Act
            var service = new EmailService(options);

            // Assert
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<IEmailService>());
        }

        [Test]
        public void SmtpSettings_PropertiesSetCorrectly()
        {
            // Arrange & Act
            var settings = new SmtpSettings
            {
                Host = "smtp.test.com",
                Port = 465,
                UserName = "user@test.com",
                Password = "secret",
                EnableSsl = false
            };

            // Assert
            Assert.That(settings.Host, Is.EqualTo("smtp.test.com"));
            Assert.That(settings.Port, Is.EqualTo(465));
            Assert.That(settings.UserName, Is.EqualTo("user@test.com"));
            Assert.That(settings.EnableSsl, Is.False);
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  NewsController Tests
    // ════════════════════════════════════════════════════════════════
    [TestFixture]
    public class NewsControllerTests
    {
        private NewsController _controller;

        [SetUp]
        public void SetUp()
        {
            _controller = new NewsController();
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller is IDisposable disposable)
                disposable.Dispose();
        }

        [Test]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Detail_ReturnsViewResult_WithAnyId()
        {
            var result = _controller.Detail(1);
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Detail_ReturnsViewResult_WithZeroId()
        {
            var result = _controller.Detail(0);
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  BankingController Tests
    // ════════════════════════════════════════════════════════════════
    [TestFixture]
    public class BankingControllerTests
    {
        private BankingController _controller;

        [SetUp]
        public void SetUp()
        {
            _controller = new BankingController();
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller is IDisposable disposable)
                disposable.Dispose();
        }

        [Test]
        public void Index_ReturnsRedirect_WhenNotAuthenticated()
        {
            // Arrange: no authenticated user
            var identity = new ClaimsIdentity(); // không authenticated
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var result = _controller.Index();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        [Test]
        public void Index_ReturnsView_WhenAuthenticated()
        {
            // Arrange: authenticated user
            var claims = new[] { new Claim(ClaimTypes.Name, "user@test.com") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var result = _controller.Index();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  MotelManagementController Tests
    // ════════════════════════════════════════════════════════════════
    [TestFixture]
    public class MotelManagementControllerTests
    {
        private MotelManagementController _controller;

        [SetUp]
        public void SetUp()
        {
            _controller = new MotelManagementController();
        }

        [TearDown]
        public void TearDown()
        {
            if (_controller is IDisposable disposable)
                disposable.Dispose();
        }

        [Test]
        public void Index_ReturnsRedirect_WhenNotAuthenticated()
        {
            var identity = new ClaimsIdentity(); // không authenticated
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var result = _controller.Index();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        [Test]
        public void Index_ReturnsView_WhenAuthenticated()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "admin@test.com") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var result = _controller.Index();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  HoaDonController (MVC) Tests
    // ════════════════════════════════════════════════════════════════
    [TestFixture]
    public class HoaDonControllerTests
    {
        private HoaDonController _controller;
        private QlNhatroContext _context;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"HoaDonDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);

            var mockLogger = new Mock<ILogger<HoaDonController>>();
            _controller = new HoaDonController(mockLogger.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            if (_controller is IDisposable disposable)
                disposable.Dispose();
        }

        [Test]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Details_ReturnsViewResult()
        {
            var result = _controller.Details(1);
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void ViewBill_ReturnsViewResult()
        {
            var result = _controller.ViewBill();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Edit_Get_ReturnsViewResult()
        {
            var result = _controller.Edit(1);
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Delete_Get_ReturnsViewResult()
        {
            var result = _controller.Delete(1);
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Create_Post_RedirectsToIndex()
        {
            var collection = new FormCollection(new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
            var result = _controller.Create(collection);
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }
    }
}
