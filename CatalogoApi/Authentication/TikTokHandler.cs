using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace CatalogoApi.Authentication;

/// <summary>
/// Se lanza cuando el open_id devuelto por TikTok no coincide con ALLOWED_TIKTOK_OPEN_ID.
/// </summary>
public sealed class TikTokAccessDeniedException : Exception
{
    public TikTokAccessDeniedException(string openId)
        : base("La cuenta de TikTok no está autorizada para iniciar sesión.")
    {
        OpenId = openId;
    }

    public string OpenId { get; }
}

/// <summary>
/// Handler de TikTok Login Kit v2. Implementa las particularidades del proveedor
/// (client_key en la autorización, formulario del token y endpoint de user info).
/// </summary>
public sealed class TikTokHandler : OAuthHandler<TikTokOptions>
{
    public TikTokHandler(
        IOptionsMonitor<TikTokOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
    {
        var url = base.BuildChallengeUrl(properties, redirectUri);
        var uri = new Uri(url);

        // TikTok Login Kit usa client_key en lugar de client_id en el query string.
        var parameters = new Dictionary<string, string?>();
        foreach (var kvp in QueryHelpers.ParseQuery(uri.Query))
        {
            var key = kvp.Key == "client_id" ? "client_key" : kvp.Key;
            parameters[key] = kvp.Key == "redirect_uri" && !string.IsNullOrWhiteSpace(Options.CallbackUrl)
                ? Options.CallbackUrl
                : kvp.Value.ToString();
        }

        return QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), parameters);
    }

    protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_key"] = Options.ClientId,
            ["client_secret"] = Options.ClientSecret,
            ["code"] = context.Code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = string.IsNullOrWhiteSpace(Options.CallbackUrl)
                ? context.RedirectUri
                : Options.CallbackUrl!,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(parameters)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await Backchannel.SendAsync(request, Context.RequestAborted);
        var body = await response.Content.ReadAsStringAsync(Context.RequestAborted);

        if (!response.IsSuccessStatusCode)
        {
            return OAuthTokenResponse.Failed(new AuthenticationFailureException(
                $"TikTok token endpoint failure: Status: {response.StatusCode}; Body: {body}"));
        }

        return OAuthTokenResponse.Success(JsonDocument.Parse(body));
    }

    protected override async Task<AuthenticationTicket> CreateTicketAsync(
        ClaimsIdentity identity,
        AuthenticationProperties properties,
        OAuthTokenResponse tokens)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await Backchannel.SendAsync(request, Context.RequestAborted);
        var body = await response.Content.ReadAsStringAsync(Context.RequestAborted);

        if (!response.IsSuccessStatusCode)
        {
            throw new AuthenticationFailureException(
                $"TikTok userinfo endpoint failure: Status: {response.StatusCode}; Body: {body}");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (root.TryGetProperty("error", out var error) &&
            error.TryGetProperty("code", out var errorCode) &&
            !string.Equals(errorCode.GetString(), "ok", StringComparison.Ordinal))
        {
            var description = error.TryGetProperty("message", out var message) ? message.GetString() : null;
            throw new AuthenticationFailureException(
                $"TikTok userinfo error: {errorCode.GetString()} {description}".Trim());
        }

        if (!root.TryGetProperty("data", out var data) ||
            !data.TryGetProperty("user", out var user))
        {
            throw new AuthenticationFailureException(
                "TikTok no devolvió los datos del usuario esperados.");
        }

        var openId = user.TryGetProperty("open_id", out var openIdElement)
            ? openIdElement.GetString()
            : null;
        var displayName = user.TryGetProperty("display_name", out var displayNameElement)
            ? displayNameElement.GetString()
            : null;
        var avatarUrl = user.TryGetProperty("avatar_url", out var avatarElement)
            ? avatarElement.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(openId))
        {
            throw new AuthenticationFailureException("TikTok no devolvió un open_id válido.");
        }

        var allowedOpenId = Options.AllowedOpenId?.Trim();

        if (!string.IsNullOrEmpty(allowedOpenId) &&
            !string.Equals(allowedOpenId, openId, StringComparison.Ordinal))
        {
            Logger.LogWarning(
                "Inicio de sesión de TikTok rechazado: open_id {OpenId} no coincide con ALLOWED_TIKTOK_OPEN_ID.",
                openId);
            throw new TikTokAccessDeniedException(openId);
        }

        if (string.IsNullOrEmpty(allowedOpenId))
        {
            Logger.LogInformation(
                "Inicio de sesión de TikTok permitido sin candado. open_id={OpenId}. " +
                "Copia este valor en ALLOWED_TIKTOK_OPEN_ID para restringir el acceso.",
                openId);
        }

        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, openId));
        identity.AddClaim(new Claim(
            ClaimTypes.Name,
            string.IsNullOrWhiteSpace(displayName) ? "Usuario de TikTok" : displayName));
        identity.AddClaim(new Claim("open_id", openId));

        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            identity.AddClaim(new Claim("avatar_url", avatarUrl));
        }

        var context = new OAuthCreatingTicketContext(
            new ClaimsPrincipal(identity),
            properties,
            Context,
            Scheme,
            Options,
            Backchannel,
            tokens,
            user);

        await Events.CreatingTicket(context);

        return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
    }
}
