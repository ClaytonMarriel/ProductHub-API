using ApiWeb.Data;
using ApiWeb.Middlewares;
using ApiWeb.Models.Auth;
using ApiWeb.Services.Auth;
using ApiWeb.Services.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContent>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Identity (para UserManager, hash de senha, etc.)
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContent>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Cookie policy (ajuda o cookie de correlação do OAuth a funcionar no callback)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;

    options.OnAppendCookie = cookieContext =>
        CheckSameSite(cookieContext.CookieOptions);

    options.OnDeleteCookie = cookieContext =>
        CheckSameSite(cookieContext.CookieOptions);
});

static void CheckSameSite(CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
        return;

    // Força SameSite=None para fluxos externos
    options.SameSite = SameSiteMode.None;
    options.Secure = true;
}

// Auth: JWT (API) + Cookie External (OAuth) + Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    // ponto chave pro login externo:
    options.DefaultSignInScheme = "External";
})
.AddJwtBearer(options =>
{
    var jwt = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
    };
})
.AddCookie("External", options =>
{
    options.Cookie.Name = "ApiWeb.External";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddGoogle("Google", options =>
{
    options.SignInScheme = "External";
    options.ClientId = builder.Configuration["GoogleAuth:ClientId"]!;
    options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"]!;

    // ✅ callback técnico do middleware
    options.CallbackPath = "/signin-google";

    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.SaveTokens = true;

    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dp-keys")))
    .SetApplicationName("ApiWeb");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// MUITO IMPORTANTE: ativa a política de cookies
app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();

app.Run();