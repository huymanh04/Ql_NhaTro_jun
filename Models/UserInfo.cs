using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ql_NhaTro_jun.Models
{
    public class UserInfo
    {
        private readonly RequestDelegate _next;

        public UserInfo(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            var request = context.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Chỉ xử lý các trang chính, bỏ qua static file, login, v.v.
            if (!path.StartsWithSegments("/api") && !path.StartsWithSegments("/css") && !path.StartsWithSegments("/js"))
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };

                foreach (var cookie in context.Request.Cookies)
                {
                    handler.CookieContainer.Add(new Uri(baseUrl), new System.Net.Cookie(cookie.Key, cookie.Value));
                }

                using var client = new HttpClient(handler);
                client.BaseAddress = new Uri(baseUrl);

                try
                {
                    var response = await client.GetAsync("/api/Auth/get-user-info");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var user = Regex.Match(json, @"""hoTen"":""(.*?)""").Groups[1].Value;
                        context.Items["CurrentUser"] = user;
                    }
                   
                }
                catch
                {
                  
                }

          
            }

            await _next(context);
        }
    }
    
}
