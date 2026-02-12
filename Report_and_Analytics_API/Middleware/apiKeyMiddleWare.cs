using System.Threading.Tasks;

namespace Report_and_Analytics_API.Middleware
{
    public class apiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<apiKeyMiddleware> _logger;
        private readonly string _apiKey;

        public apiKeyMiddleware(IConfiguration configuration,RequestDelegate next,ILogger<apiKeyMiddleware>logger)
        {
            _next = next;
            _logger = logger;
            _apiKey = configuration["X-API-KEY"];
        }

        public async void InvokeAsync(HttpContext context)
        {
            try
            {
                if(!context.Request.Headers.TryGetValue("X-API-KEY",out var extractedKey))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Missing Key");
                    return;
                }

                if (!extractedKey.Equals(_apiKey))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Key not matched");
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(message:$"Error:{ex.Message}");
                return;
            }
        }
    }
}
