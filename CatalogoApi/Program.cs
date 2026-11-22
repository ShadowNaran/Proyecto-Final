using CatalogoApi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Security.Claims;

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
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "Data Source=catalogo.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Helper para obtener configuración con prioridad: variable de entorno -> IConfiguration -> dummy
string GetConfig(string envVar, string configPath, string fallback)
{
    var val = Environment.GetEnvironmentVariable(envVar);
    if (!string.IsNullOrWhiteSpace(val)) return val;
    val = builder.Configuration[configPath];
    if (!string.IsNullOrWhiteSpace(val)) return val;
    return fallback;
}

// Configuración de autenticación con cookies y OAuth 2.0 (Twitch y X)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
