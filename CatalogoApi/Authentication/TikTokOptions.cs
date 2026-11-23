using Microsoft.AspNetCore.Authentication.OAuth;

namespace CatalogoApi.Authentication;

/// <summary>
/// Opciones del proveedor OAuth 2.0 de TikTok (Login Kit v2).
/// </summary>
public sealed class TikTokOptions : OAuthOptions
{
    public TikTokOptions()
    {
        AuthorizationEndpoint = "https://www.tiktok.com/v2/auth/authorize/";
        TokenEndpoint = "https://open.tiktokapis.com/v2/oauth/token/";
        UserInformationEndpoint = "https://open.tiktokapis.com/v2/user/info/?fields=open_id,display_name,avatar_url";
        Scope.Add("user.info.basic");

        // Login Kit Web usa el flujo con client_secret; PKCE solo es obligatorio
        // para aplicaciones mobile/desktop, por eso se mantiene deshabilitado.
        UsePkce = false;
    }

    /// <summary>
    /// URL absoluta de callback registrada en TikTok. Si está vacía, se calcula
    /// desde la petición usando la ruta de callback configurada.
    /// </summary>
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// open_id autorizado. Si está vacío, se permite cualquier cuenta en modo prueba.
    /// </summary>
    public string? AllowedOpenId { get; set; }
}
