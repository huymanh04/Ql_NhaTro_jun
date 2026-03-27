using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api_Ql_nhatro.Controllers;

namespace Ql_NhaTro_jun.Tests;

[TestFixture]
public class AuthAndContractQuickTests
{
    private static DefaultHttpContext BuildHttpContextWithSession(ISession session)
    {
        var context = new DefaultHttpContext();
        context.Features.Set<ISessionFeature>(new TestSessionFeature { Session = session });
        return context;
    }

    [Test]
    public void Auth_GetAesKey_ReturnsOk_AndStoresSessionValues()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"AuthAes_{Guid.NewGuid()}")
            .Options;

        using var db = new QlNhatroContext(options);
        var logger = new Mock<ILogger<AuthController>>();
        var emailService = new Mock<IEmailService>();
        var controller = new AuthController(logger.Object, db, emailService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContextWithSession(new TestSession())
            }
        };

        var result = controller.GetAesKey(new EmailRequest { Email = "a@test.com" });

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var key = controller.HttpContext.Session.GetString("AES_a@test.com_Key");
        var iv = controller.HttpContext.Session.GetString("AES_a@test.com_IV");
        Assert.That(string.IsNullOrEmpty(key), Is.False);
        Assert.That(string.IsNullOrEmpty(iv), Is.False);
    }

    [Test]
    public async Task Auth_VerifyEmailCode_ReturnsNotFound_WhenSessionEmailMissing()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"AuthVerify_{Guid.NewGuid()}")
            .Options;

        using var db = new QlNhatroContext(options);
        var logger = new Mock<ILogger<AuthController>>();
        var emailService = new Mock<IEmailService>();

        var controller = new AuthController(logger.Object, db, emailService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContextWithSession(new TestSession())
            }
        };

        var result = await controller.VerifyEmailCode(new AuthController.VerifyEmailCodeRequest
        {
            Email = "a@test.com",
            Code = "123456"
        });

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Auth_ResendEmailCode_ReturnsOk()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"AuthResend_{Guid.NewGuid()}")
            .Options;

        using var db = new QlNhatroContext(options);
        var logger = new Mock<ILogger<AuthController>>();
        var emailService = new Mock<IEmailService>();
        emailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var controller = new AuthController(logger.Object, db, emailService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContextWithSession(new TestSession())
            }
        };

        var result = await controller.ResendEmailCode(new EmailRequest { Email = "a@test.com" });

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Auth_GetUserInfo_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"AuthUserInfo_{Guid.NewGuid()}")
            .Options;

        using var db = new QlNhatroContext(options);
        var logger = new Mock<ILogger<AuthController>>();
        var emailService = new Mock<IEmailService>();

        var controller = new AuthController(logger.Object, db, emailService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            }
        };

        var result = await controller.GetUserInfo();
        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public void Auth_AesHelper_Decrypt_InvalidInput_Throws()
    {
        Assert.Throws<Exception>(() => AuthController.AesHelper.Decrypt("not-base64", "bad", "bad"));
    }

    [Test]
    public async Task Contract_CreateContract_ReturnsBadRequest_ForInvalidInputs()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"ContractInvalid_{Guid.NewGuid()}")
            .Options;

        using var db = new QlNhatroContext(options);
        var logger = new Mock<ILogger<ContractController>>();
        var controller = new ContractController(logger.Object, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            }
        };

        var nullResult = await controller.CreateContract(null!);
        Assert.That(nullResult, Is.InstanceOf<BadRequestObjectResult>());

        var badRoom = await controller.CreateContract(new ContractController.ContractCreateDto
        {
            RoomId = 0,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            NumberOfTenants = 1,
            Soxe = 1,
            DepositAmount = 100,
            TenantIds = new List<int> { 1 }
        });
        Assert.That(badRoom, Is.InstanceOf<BadRequestObjectResult>());

        var badDate = await controller.CreateContract(new ContractController.ContractCreateDto
        {
            RoomId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today,
            NumberOfTenants = 1,
            Soxe = 1,
            DepositAmount = 100,
            TenantIds = new List<int> { 1 }
        });
        Assert.That(badDate, Is.InstanceOf<BadRequestObjectResult>());

        var badTenant = await controller.CreateContract(new ContractController.ContractCreateDto
        {
            RoomId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            NumberOfTenants = 1,
            Soxe = 1,
            DepositAmount = 100,
            TenantIds = new List<int>()
        });
        Assert.That(badTenant, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Contract_CreateContract_ReturnsUnauthorized_WhenNoUserName()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"ContractUnauth_{Guid.NewGuid()}")
            .Options;

        using var db = new QlNhatroContext(options);
        var logger = new Mock<ILogger<ContractController>>();
        var controller = new ContractController(logger.Object, db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            }
        };

        var result = await controller.CreateContract(new ContractController.ContractCreateDto
        {
            RoomId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(30),
            NumberOfTenants = 1,
            Soxe = 1,
            DepositAmount = 100,
            TenantIds = new List<int> { 1 }
        });

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task Contract_GetContractById_ReturnsNotFound_WhenMissing()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"ContractGet_{Guid.NewGuid()}")
            .Options;

        using var db = new QlNhatroContext(options);
        var logger = new Mock<ILogger<ContractController>>();
        var controller = new ContractController(logger.Object, db);

        var result = await controller.GetContractById(99999);
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    private sealed class TestSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = null!;
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public IEnumerable<string> Keys => _store.Keys;
        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value)
        {
            _store[key] = value;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            return _store.TryGetValue(key, out value!);
        }
    }
}
