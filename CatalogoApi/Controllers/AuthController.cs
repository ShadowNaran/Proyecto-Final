using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetFrontendRedirectUri()
    {
        var frontendUrl = _configuration["FRONTEND_URL"]
            ?? Environment.GetEnvironmentVariable("FRONTEND_URL");

        if (!string.IsNullOrWhiteSpace(frontendUrl))
        {
            return frontendUrl.EndsWith("/") ? frontendUrl : $"{frontendUrl}/";
        }

        return "/";
    }

    [HttpGet("login/sso")]
    public IActionResult LoginSso()
    {
        var properties = new AuthenticationProperties { RedirectUri = GetFrontendRedirectUri() };
        return Challenge(properties, "Auth0");
    }

    [HttpGet("login/twitch")]
    public IActionResult LoginTwitch()
    {
        var properties = new AuthenticationProperties { RedirectUri = GetFrontendRedirectUri() };
        return Challenge(properties, "Twitch");
    }

    [HttpGet("login/twitter")]
    public IActionResult LoginTwitter()
    {
        var properties = new AuthenticationProperties { RedirectUri = GetFrontendRedirectUri() };
        return Challenge(properties, "Twitter");
    }

    [HttpGet("login/discord")]
    public IActionResult LoginDiscord()
    {
        var properties = new AuthenticationProperties { RedirectUri = GetFrontendRedirectUri() };
        return Challenge(properties, "Discord");
    }

    [HttpGet("login/spotify")]
    public IActionResult LoginSpotify()
    {
        var properties = new AuthenticationProperties { RedirectUri = GetFrontendRedirectUri() };
        return Challenge(properties, "Spotify");
    }

    [HttpGet("login/dropbox")]
    public IActionResult LoginDropbox()
    {
        var properties = new AuthenticationProperties { RedirectUri = GetFrontendRedirectUri() };
        return Challenge(properties, "Dropbox");
    }

    [HttpGet("login/slack")]
    public IActionResult LoginSlack()
    {
        var properties = new AuthenticationProperties { RedirectUri = GetFrontendRedirectUri() };
        return Challenge(properties, "Slack");
    }

    [HttpGet("login/tiktok")]
    public IActionResult LoginTikTok()
    {
        var properties = new AuthenticationProperties { RedirectUri = GetFrontendRedirectUri() };
        return Challenge(properties, "TikTok");
    }

    [HttpGet("providers")]
    public IActionResult GetConfiguredProviders()
    {
        bool IsConfigured(string envVar, string configKey)
        {
            var val = Environment.GetEnvironmentVariable(envVar) ?? _configuration[configKey];
            return !string.IsNullOrWhiteSpace(val) && !val.StartsWith("dummy-", StringComparison.OrdinalIgnoreCase);
        }

        return Ok(new
        {
            sso = IsConfigured("AUTH0_CLIENT_ID", "Authentication:Auth0:ClientId"),
            tiktok = IsConfigured("TIKTOK_CLIENT_KEY", "Authentication:TikTok:ClientKey"),
            discord = IsConfigured("DISCORD_CLIENT_ID", "Authentication:Discord:ClientId"),
            spotify = IsConfigured("SPOTIFY_CLIENT_ID", "Authentication:Spotify:ClientId"),
            dropbox = IsConfigured("DROPBOX_CLIENT_ID", "Authentication:Dropbox:ClientId"),
            slack = IsConfigured("SLACK_CLIENT_ID", "Authentication:Slack:ClientId"),
            twitch = IsConfigured("TWITCH_CLIENT_ID", "Authentication:Twitch:ClientId"),
            twitter = IsConfigured("TWITTER_CLIENT_ID", "Authentication:X:ClientId")
        });
    }

    [HttpGet("me")]
    public IActionResult GetMe()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Ok(new
            {
                autenticado = false
            });
        }

        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value
                 ?? User.FindFirst("id")?.Value
                 ?? string.Empty;

        var usuario = User.FindFirst(ClaimTypes.Name)?.Value
                      ?? User.FindFirst("name")?.Value
                      ?? User.FindFirst("username")?.Value
                      ?? string.Empty;

        var proveedor = User.FindFirst("provider")?.Value
                        ?? User.Identity?.AuthenticationType
                        ?? "Desconocido";

        return Ok(new
        {
            autenticado = true,
            proveedor,
            usuario,
            id
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { mensaje = "Sesión cerrada correctamente" });
    }
}
