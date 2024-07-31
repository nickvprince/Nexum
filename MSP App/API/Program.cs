using API.DataAccess;
using API.Middleware;
using API.Services.APIServices;
using API.Services.DbServices;
using API.Services.TenantServerAPIServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.Entities.DbEntities;
using SharedComponents.Handlers.Attributes.HasPermission;
using SharedComponents.JWTToken.Entities;
using SharedComponents.JWTToken.Services;
using SharedComponents.Services.APIServices.Interfaces;
using SharedComponents.Services.DbServices.Interfaces;
using SharedComponents.Services.TenantServerAPIServices.Interfaces;
using SharedComponents.Utilities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        /*options.JsonSerializerOptions.MaxDepth = 2;*/
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1-Web", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Web API",
        Version = "v1",
        Description = "API definition for communication between the web app and the web api."
    });
    options.SwaggerDoc("v1-Server", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Server API",
        Version = "v1",
        Description = "API definition for communication between the Tenant-Server and the web api."
    });
});

builder.Services.AddHttpClient("ServerClient");

// Add services to the container.
var connStr = builder.Configuration.GetConnectionString("NexumAppDb");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connStr));
builder.Services.AddScoped<IDbUserService, DbUserService>();
builder.Services.AddScoped<IDbTenantService, DbTenantService>();
builder.Services.AddScoped<IDbDeviceService, DbDeviceService>();
builder.Services.AddScoped<IDbSecurityService, DbSecurityService>();
builder.Services.AddScoped<IDbSoftwareService, DbSoftwareService>();
builder.Services.AddScoped<IDbAlertService, DbAlertService>();
builder.Services.AddScoped<IDbLogService, DbLogService>();
builder.Services.AddScoped<IDbRoleService, DbRoleService>();
builder.Services.AddScoped<IDbPermissionService, DbPermissionService>();
builder.Services.AddScoped<IDbInstallationKeyService, DbInstallationKeyService>();
builder.Services.AddScoped<IDbNASServerService, DbNASServerService>();
builder.Services.AddScoped<IDbBackupService, DbBackupService>();
builder.Services.AddScoped<IDbJobService, DbJobService>();
builder.Services.AddScoped<ITenantServerAPIJobService, TenantServerAPIJobService>();
builder.Services.AddScoped<ITenantServerAPIDeviceService, TenantServerAPIDeviceService>();
builder.Services.AddScoped<ITenantServerAPINASServerService, TenantServerAPINASServerService>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IAPIAuthService, APIAuthService>();

builder.Services.AddTransient<IAuthorizationPolicyProvider, HasPermissionPolicyProvider>();
builder.Services.AddTransient<IAuthorizationHandler, HasPermissionHandler>();

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

builder.Services.AddAuthorization();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWTSettings").Get<JWTSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudience = jwtSettings.Audience,
        ValidIssuer = jwtSettings.Issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecurityUtilities.PadKey(jwtSettings.SecretKey, 32))),
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1-Web/swagger.json", "Web API");
        options.SwaggerEndpoint("/swagger/v1-Server/swagger.json", "Server API");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    //create default superuser and custom roles
    await AppDbContext.IntitalizeUserIdentities(scope.ServiceProvider);
}

app.Run("https://0.0.0.0:7101");
