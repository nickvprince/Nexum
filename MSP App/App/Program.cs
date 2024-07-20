//using Microsoft.AspNetCore.Authentication.Negotiate;
using App.Middleware;
using App.Services.APIRequestServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.JWTToken.Entities;
using SharedComponents.Services.APIRequestServices.Interfaces;
using SharedComponents.Utilities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient("BlazorClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WebAppSettings:APIBaseUri"] + ":" + builder.Configuration["WebAppSettings:APIBasePort"]);
});

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Nexum.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

/*builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});*/
//builder.Services.AddRazorPages();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddScoped<IAPIRequestAlertService, APIRequestAlertService>();
builder.Services.AddScoped<IAPIRequestAuthService, APIRequestAuthService>();
builder.Services.AddScoped<IAPIRequestBackupService, APIRequestBackupService>();
builder.Services.AddScoped<IAPIRequestDeviceService, APIRequestDeviceService>();
builder.Services.AddScoped<IAPIRequestInstallationKeyService, APIRequestInstallationKeyService>();
builder.Services.AddScoped<IAPIRequestJobService, APIRequestJobService>();
builder.Services.AddScoped<IAPIRequestLogService, APIRequestLogService>();
builder.Services.AddScoped<IAPIRequestNASServerService, APIRequestNASServerService>();
builder.Services.AddScoped<IAPIRequestPermissionService, APIRequestPermissionService>();
builder.Services.AddScoped<IAPIRequestRoleService, APIRequestRoleService>();
builder.Services.AddScoped<IAPIRequestTenantService, APIRequestTenantService>();
builder.Services.AddScoped<IAPIRequestUserService, APIRequestUserService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = ".Nexum.AuthCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
    options.LoginPath = "/Auth/Login";
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JWTSettings").Get<JWTSettings>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecurityUtilities.PadKey(jwtSettings.SecretKey, 32))),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
//app.MapBlazorHub();

app.UseMiddleware<CookieRefreshMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
//app.Run($"https://0.0.0.0:" + builder.Configuration["WebAppSettings:BasePort"]);
