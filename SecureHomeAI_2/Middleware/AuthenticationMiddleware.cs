namespace SecureHomeAI_2.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            var isPublicPath = path.StartsWithSegments("/Home/Login") ||
                              path.StartsWithSegments("/Home/Register") ||
                              path.StartsWithSegments("/Home/About");

            if (!isPublicPath && context.Session.GetString("UserId") == null)
            {
                context.Response.Redirect("/Home/Login");
                return;
            }

            await _next(context);
        }
    }

    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}