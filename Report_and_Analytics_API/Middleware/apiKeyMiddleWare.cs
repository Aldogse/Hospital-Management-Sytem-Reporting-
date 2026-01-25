using System.Threading.Tasks;
using MimeKit.Cryptography;

namespace Report_and_Analytics_API.Middleware
{
    public class apiKeyMiddleWare
    {
        private readonly string _apiKey;
        private readonly RequestDelegate _next;

        public apiKeyMiddleWare(IConfiguration config,RequestDelegate next)
        {
            _apiKey = config["X-API-KEY"];
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-API-KEY",out var extractedKey))
            {
                context.Response.StatusCode = 401;
                return;
            }

            if (!_apiKey.Equals(extractedKey))
            {
                context.Response.StatusCode = 401;
                return;
            }

            await _next(context);
        }
    }
}
