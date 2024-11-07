using _0021412438_NguyenTanHuy.Services;

namespace _0021412438_NguyenTanHuy.Middleware
{
    public class TokenFilterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenFilterMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TokenFilterMiddleware(RequestDelegate next, ILogger<TokenFilterMiddleware> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var token = authorizationHeader.ToString().Replace("Bearer ", "");

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                    if (!string.IsNullOrEmpty(token))
                    {
                        try
                        {
                            var userDTO = authService.GetUserFromExpiredToken(token);

                            if (userDTO.Roles.Contains("Admin") || userDTO.Roles.Contains("User"))
                            {
                                _logger.LogInformation("Token is valid and user has the Admin and User role.");
                            }
                            else
                            {
                                _logger.LogWarning("User does not have the required role.");
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                await context.Response.WriteAsync("Forbidden: Insufficient permissions.");
                                return;
                            }
                        }
                        catch
                        {
                            _logger.LogWarning("Invalid or expired token.");
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Unauthorized access. Invalid or expired token.");
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Token is missing.");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized access. Token is missing.");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

}
