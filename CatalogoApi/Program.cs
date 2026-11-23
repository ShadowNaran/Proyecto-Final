using CatalogoApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno desde .env si existe (evitando fallos si no existe)
var envPaths = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "CatalogoApi", ".env"),
    Path.Combine(AppContext.BaseDirectory, ".env")
};

foreach (var envPath in envPaths)
{
    if (File.Exists(envPath))
    {
        DotNetEnv.Env.Load(envPath);
        break;
    }
}

// Add services to the container.
// La API responde con DTOs; IgnoreCycles queda como red de seguridad para no
// volver a fallar con JsonException si alguna entidad se serializa por error.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Render termina TLS en su proxy y envia X-Forwarded-Proto / X-Forwarded-For.
// Se limpian las redes conocidas porque las IPs del proxy de Render son dinamicas.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Helper para obtener configuración con prioridad: variable de entorno -> IConfiguration -> fallback
string GetConfig(string envVar, string configPath, string fallback)
{
    var val = Environment.GetEnvironmentVariable(envVar);
    if (!string.IsNullOrWhiteSpace(val)) return val;
    val = builder.Configuration[configPath];
    if (!string.IsNullOrWhiteSpace(val)) return val;
    return fallback;
}

var connectionString = GetConfig(
    "DB_CONNECTION_STRING",
    "ConnectionStrings:DefaultConnection",
    "Host=localhost;Port=5432;Database=catalogo_db;Username=postgres;Password=postgres");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configuración de autenticación con cookies y OAuth 2.0 (Twitch y X)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;

    // Responder 401 Unauthorized para peticiones no autenticadas a /api/* en vez de redirección 302
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
})
.AddTwitch("Twitch", options =>
{
    options.ClientId = GetConfig("TWITCH_CLIENT_ID", "Authentication:Twitch:ClientId", "dummy-twitch-client-id");
    options.ClientSecret = GetConfig("TWITCH_CLIENT_SECRET", "Authentication:Twitch:ClientSecret", "dummy-twitch-client-secret");
    options.CallbackPath = new PathString(GetConfig("TWITCH_CALLBACK_PATH", "Authentication:Twitch:CallbackPath", "/signin-twitch"));
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Twitch"));
        return Task.CompletedTask;
    };
})
.AddTwitter("Twitter", options =>
{
    options.ClientId = GetConfig("TWITTER_CLIENT_ID", "Authentication:X:ClientId", "dummy-twitter-client-id");
    options.ClientSecret = GetConfig("TWITTER_CLIENT_SECRET", "Authentication:X:ClientSecret", "dummy-twitter-client-secret");
    options.CallbackPath = new PathString(GetConfig("TWITTER_CALLBACK_PATH", "Authentication:X:CallbackPath", "/signin-twitter"));
    // OAuth 2.0 con endpoint de usuario básico sin requerir email
    options.UserInformationEndpoint = "https://api.twitter.com/2/users/me";
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Twitter"));
        return Task.CompletedTask;
    };
})
.AddDiscord("Discord", options =>
{
    options.ClientId = GetConfig("DISCORD_CLIENT_ID", "Authentication:Discord:ClientId", "dummy-discord-client-id");
    options.ClientSecret = GetConfig("DISCORD_CLIENT_SECRET", "Authentication:Discord:ClientSecret", "dummy-discord-client-secret");
    options.CallbackPath = new PathString(GetConfig("DISCORD_CALLBACK_PATH", "Authentication:Discord:CallbackPath", "/signin-discord"));
    options.Scope.Add("identify");
    options.Scope.Add("email");
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Discord"));
        return Task.CompletedTask;
    };
})
.AddSpotify("Spotify", options =>
{
    options.ClientId = GetConfig("SPOTIFY_CLIENT_ID", "Authentication:Spotify:ClientId", "dummy-spotify-client-id");
    options.ClientSecret = GetConfig("SPOTIFY_CLIENT_SECRET", "Authentication:Spotify:ClientSecret", "dummy-spotify-client-secret");
    options.CallbackPath = new PathString(GetConfig("SPOTIFY_CALLBACK_PATH", "Authentication:Spotify:CallbackPath", "/signin-spotify"));
    options.Scope.Add("user-read-email");
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Spotify"));
        return Task.CompletedTask;
    };
})
.AddDropbox("Dropbox", options =>
{
    options.ClientId = GetConfig("DROPBOX_CLIENT_ID", "Authentication:Dropbox:ClientId", "dummy-dropbox-client-id");
    options.ClientSecret = GetConfig("DROPBOX_CLIENT_SECRET", "Authentication:Dropbox:ClientSecret", "dummy-dropbox-client-secret");
    options.CallbackPath = new PathString(GetConfig("DROPBOX_CALLBACK_PATH", "Authentication:Dropbox:CallbackPath", "/signin-dropbox"));
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Dropbox"));
        return Task.CompletedTask;
    };
})
.AddSlack("Slack", options =>
{
    options.ClientId = GetConfig("SLACK_CLIENT_ID", "Authentication:Slack:ClientId", "dummy-slack-client-id");
    options.ClientSecret = GetConfig("SLACK_CLIENT_SECRET", "Authentication:Slack:ClientSecret", "dummy-slack-client-secret");
    options.CallbackPath = new PathString(GetConfig("SLACK_CALLBACK_PATH", "Authentication:Slack:CallbackPath", "/signin-slack"));
    options.Scope.Add("identity.basic");
    options.Scope.Add("identity.email");
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        context.Identity?.AddClaim(new Claim("provider", "Slack"));
        return Task.CompletedTask;
    };
});

var allowedProviders = GetConfig("REPORTE_ALLOWED_PROVIDERS", "ReporteAbraham:AllowedProviders", "Discord,Spotify")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var allowedUsers = GetConfig("REPORTE_ALLOWED_USERS", "ReporteAbraham:AllowedUsers", string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReporteAbrahamPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var provider = context.User.FindFirst("provider")?.Value;
            if (!string.IsNullOrWhiteSpace(provider) && allowedProviders.Contains(provider))
            {
                return true;
            }

            if (allowedUsers.Count > 0)
            {
                var userIdentifiers = new[]
                {
                    context.User.FindFirst(ClaimTypes.Name)?.Value,
                    context.User.FindFirst("name")?.Value,
                    context.User.FindFirst("username")?.Value,
                    context.User.FindFirst(ClaimTypes.Email)?.Value,
                    context.User.FindFirst("email")?.Value,
                    context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    context.User.FindFirst("sub")?.Value,
                    context.User.FindFirst("id")?.Value
                };

                if (userIdentifiers.Any(u => !string.IsNullOrWhiteSpace(u) && allowedUsers.Contains(u)))
                {
                    return true;
                }
            }

            return false;
        });
    });
});

var app = builder.Build();

// Debe ejecutarse antes de cualquier middleware que dependa del esquema/IP real
// (HTTPS redirection, cookies, autenticacion) para reconocer el proxy de Render.
app.UseForwardedHeaders();

// Aplica las migraciones pendientes en PostgreSQL al arrancar, con reintentos
// por si el servicio de base de datos tarda unos segundos en aceptar conexiones.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    const int maxIntentos = 5;

    for (var intento = 1; intento <= maxIntentos; intento++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (intento < maxIntentos)
        {
            logger.LogWarning(
                ex,
                "No se pudo conectar a PostgreSQL para aplicar migraciones (intento {Intento}/{MaxIntentos}). Reintentando en 2 segundos...",
                intento,
                maxIntentos);
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
