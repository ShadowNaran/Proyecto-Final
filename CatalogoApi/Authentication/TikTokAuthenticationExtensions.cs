using Microsoft.AspNetCore.Authentication;

namespace CatalogoApi.Authentication;

public static class TikTokAuthenticationExtensions
{
    public static AuthenticationBuilder AddTikTok(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        Action<TikTokOptions> configureOptions)
    {
        return builder.AddOAuth<TikTokOptions, TikTokHandler>(authenticationScheme, configureOptions);
    }
}
