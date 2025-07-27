using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Ql_NhaTro_jun.Models;
using Ql_NhaTro_jun.Hubs;
using Ql_NhaTro_jun.Controllers;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // Thêm dòng này để đồng bộ với CookiePolicy
});
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
      options.LoginPath = "/Account/Login";
      options.LogoutPath = "/Account/Logout";
      options.Cookie.HttpOnly = true;
      options.ExpireTimeSpan = TimeSpan.FromHours(1); // Nếu không chọn RememberMe
      options.SlidingExpiration = true;
      options.Events = new CookieAuthenticationEvents
      {
          OnValidatePrincipal = context =>
          {
              // Có thể thêm custom check token/user bị khóa ở đây
              return Task.CompletedTask;
          }
      };
  })
  .AddGoogle(options =>
  {
      options.ClientId = "496423344091-9g7qvpmd6irtct4lpbjk1pcns9baiqr7.apps.googleusercontent.com";
      options.ClientSecret = "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";
      options.CallbackPath = "/signin-google";
  });



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JunTech API - Quản Lý Phòng Trọ",
        Version = "v1",
        Description = "API quản lý nhà trọ cho Admin và Người dùng"
    });
});


builder.Services.AddDbContext<QlNhatroContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

// Thêm cấu hình CookiePolicy để tránh lỗi SameSite khi dùng Google OAuth
// XÓA cấu hình AddGoogle và app.UseCookiePolicy liên quan đến Google OAuth

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "JunTech API V1");
    c.RoutePrefix = "swagger"; // => https://localhost:<port>/swagger
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();



app.UseRouting();

app.UseAuthentication();   // ✅ Cần thêm dòng này
app.UseAuthorization();

          // ✅ Nếu bạn dùng AES key lưu session

app.UseMiddleware<UserInfo>();  // ✅ Middleware lấy user gán context
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();  // hoặc MapDefaultControllerRoute(), tùy project
    endpoints.MapHub<ChatHub>("/chatHub");
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
