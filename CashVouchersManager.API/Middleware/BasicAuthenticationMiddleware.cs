using System.Net.Http.Headers;
using System.Text;
using CashVouchersManager.API.Configuration;
using Microsoft.Extensions.Options;

namespace CashVouchersManager.API.Middleware;

/// <summary>
/// Middleware for HTTP Basic Authentication
/// </summary>
public class BasicAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppSettings _appSettings;
    private readonly ILogger<BasicAuthenticationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticationMiddleware"/> class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="appSettings">The application settings</param>
    /// <param name="logger">The logger instance</param>
    public BasicAuthenticationMiddleware(
        RequestDelegate next,
        IOptions<AppSettings> appSettings,
        ILogger<BasicAuthenticationMiddleware> logger)
    {
        _next = next;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to process the HTTP request
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for Swagger endpoints
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Check if Authorization header exists
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            await ChallengeAsync(context);
            return;
        }

        try
        {
            var authHeaderValue = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeaderValue))
            {
                await ChallengeAsync(context);
                return;
            }

            var authHeader = AuthenticationHeaderValue.Parse(authHeaderValue);
            
            if (authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                
                if (credentials.Length == 2)
                {
                    var username = credentials[0];
                    var password = credentials[1];

                    // Validate credentials
                    if (username == _appSettings.Authentication.Username && 
                        password == _appSettings.Authentication.Password)
                    {
                        _logger.LogInformation("User {Username} authenticated successfully", username);
                        await _next(context);
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Authentication failed due to exception");
        }

        // Authentication failed
        _logger.LogWarning("Authentication failed for request to {Path}", context.Request.Path);
        await ChallengeAsync(context);
    }

    /// <summary>
    /// Sends an authentication challenge to the client
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task ChallengeAsync(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"CashVouchersManager API\"";
        await context.Response.WriteAsync("Unauthorized");
    }
}
