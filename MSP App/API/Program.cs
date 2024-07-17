using API.Attributes.Handlers;
using API.DataAccess;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.Entities;
using SharedComponents.JWTToken.Entities;
using SharedComponents.JWTToken.Services;
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
builder.Services.AddScoped<DbUserService>();
builder.Services.AddScoped<DbTenantService>();
builder.Services.AddScoped<DbDeviceService>();
builder.Services.AddScoped<DbSecurityService>();
builder.Services.AddScoped<DbSoftwareService>();
builder.Services.AddScoped<DbAlertService>();
builder.Services.AddScoped<DbLogService>();
builder.Services.AddScoped<DbRoleService>();
builder.Services.AddScoped<DbPermissionService>();
builder.Services.AddScoped<DbInstallationKeyService>();
builder.Services.AddScoped<DbNASServerService>();
builder.Services.AddScoped<DbBackupService>();
builder.Services.AddScoped<DbJobService>();
builder.Services.AddScoped<HTTPJobService>();
builder.Services.AddScoped<HTTPDeviceService>();
builder.Services.AddScoped<HTTPNASServerService>();
builder.Services.AddScoped<IJWTService, JWTService>();

builder.Services.AddScoped<IAuthorizationHandler, HasPermissionHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HasPermission", policy =>
        policy.Requirements.Add(new HasPermissionRequirement("")));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWTSettings").Get<JWTSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
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

app.Run();
