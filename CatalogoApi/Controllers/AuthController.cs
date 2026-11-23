using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("login/twitch")]
    public IActionResult LoginTwitch()
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Challenge(properties, "Twitch");
    }

    [HttpGet("login/twitter")]
    public IActionResult LoginTwitter()
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Challenge(properties, "Twitter");
    }

    [HttpGet("login/discord")]
    public IActionResult LoginDiscord()
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Challenge(properties, "Discord");
    }

    [HttpGet("login/spotify")]
    public IActionResult LoginSpotify()
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Challenge(properties, "Spotify");
    }

    [HttpGet("login/dropbox")]
    public IActionResult LoginDropbox()
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Challenge(properties, "Dropbox");
    }

    [HttpGet("login/slack")]
    public IActionResult LoginSlack()
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Challenge(properties, "Slack");
    }

    [HttpGet("login/tiktok")]
    public IActionResult LoginTikTok()
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Challenge(properties, "TikTok");
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
