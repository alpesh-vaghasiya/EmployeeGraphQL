using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ISsoService ssoService,
        RedisCacheService redis)
    {
        // 👉 Only for REST API
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authorization header missing");
            return;
        }

        var token = authHeader.Replace("Bearer ", "");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var uid = jwt.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;

        if (string.IsNullOrEmpty(uid))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid Token");
            return;
        }

        var cacheKey = $"TOKEN|{uid}";
        bool isValid = false;

        // Redis check
        var cachedToken = await redis.GetAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedToken) && cachedToken == token)
        {
            isValid = true;
        }

        // Redis miss → call SSO
        if (!isValid)
        {
            isValid = await ssoService.ValidateToken(token);
        }

        if (!isValid)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token validation failed");
            return;
        }

        // Set claims
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, uid)
        };

        var identity = new ClaimsIdentity(claims, "SSO");
        context.User = new ClaimsPrincipal(identity);

        await _next(context);
    }
}